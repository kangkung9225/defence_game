// Assets/Editor/C2paVerifier.cs
// c2patool(외부 프로그램)을 실행해 이미지의 C2PA 서명을 검증하고 결과를 정리한다.
// - Unity API를 쓰지 않는다 → Unity 밖(일반 .NET)에서도 테스트 가능
// - 설치: zsh에서  brew install c2patool  →  c2patool init trust  (공식 신뢰 목록 내려받기)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

public sealed class C2paResult
{
    public string State = "";      // none / Valid / Trusted / Invalid / error / tool-missing
    public string Issuer = "";     // 서명한 인증서의 발급 대상(조직) 예: "OpenAI"
    public string Generator = "";  // 만든 도구 이름 예: "ChatGPT"
    public bool AiDeclared;        // 매니페스트가 "AI로 생성"이라고 선언했는지
    public string Failures = "";   // 검증 실패 코드들 (공백 구분)

    public bool Trusted => State == "Trusted";

    // CSV ai_hint 칸에 넣을 짧은 문자열
    public string ToHint()
    {
        if (State == "" || State == "none") return "";
        var sb = new StringBuilder("C2PA:" + State);
        if (State == "Valid") sb.Append("-untrusted");          // 서명 수학은 맞지만 서명자가 신뢰 목록에 없음
        if (Issuer != "") sb.Append(" issuer=" + Issuer);
        if (Generator != "" && !Trusted) sb.Append(" gen=" + Generator); // Trusted면 tool 칸에 들어감
        if (AiDeclared) sb.Append(" AI-declared");
        if (Failures != "") sb.Append(" fail=" + Failures);
        return sb.ToString();
    }
}

public static class C2paVerifier
{
    public static readonly HashSet<string> SupportedExt =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

    // Unity(GUI 앱)는 zsh의 PATH를 물려받지 않아서 설치 위치를 직접 찾는다.
    public static string FindTool(string overridePath)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidates = new[]
        {
            overridePath,
            "/opt/homebrew/bin/c2patool",              // Homebrew (Apple Silicon)
            "/usr/local/bin/c2patool",                 // Homebrew (Intel) / 수동 설치
            Path.Combine(home, ".cargo/bin/c2patool"), // cargo install
        };
        foreach (var c in candidates)
            if (!string.IsNullOrEmpty(c) && File.Exists(c)) return c;
        return null;
    }

    public static C2paResult Verify(string fullPath, string toolPath, int timeoutMs = 10000)
    {
        if (toolPath == null) return new C2paResult { State = "tool-missing" };

        var (code, stdout, stderr) = Run(toolPath, "\"" + fullPath + "\"", timeoutMs);
        if (stdout.Contains("\"active_manifest\""))
        {
            try { return ParseReport(stdout); }
            catch (Exception e) { return new C2paResult { State = "error", Failures = "parse:" + e.Message }; }
        }

        // 매니페스트가 없으면 c2patool은 오류로 끝난다 (메시지는 버전마다 조금씩 다름)
        string err = (stderr + " " + stdout).ToLowerInvariant();
        if (err.Contains("no claim") || err.Contains("not found") || err.Contains("jumbf") || err.Contains("no manifest"))
            return new C2paResult { State = "none" };
        return new C2paResult { State = "error", Failures = "exit" + code + ":" + FirstLine(stderr) };
    }

    public static (int code, string stdout, string stderr) Run(string exe, string args, int timeoutMs)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        using (var p = Process.Start(psi))
        {
            // 두 출력을 동시에 읽어야 버퍼가 가득 차서 멈추는 일(교착)이 없다
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            if (!p.WaitForExit(timeoutMs))
            {
                try { p.Kill(); } catch { }
                return (-1, "", "timeout " + timeoutMs + "ms");
            }
            return (p.ExitCode, outTask.Result, errTask.Result);
        }
    }

    // c2patool 기본 출력(JSON)에서 "활성 매니페스트" 하나만 골라 필요한 값만 뽑는다.
    public static C2paResult ParseReport(string json)
    {
        var root = MiniJson.Parse(json) as Dictionary<string, object>;
        var r = new C2paResult();
        if (root == null || !(root.TryGetValue("active_manifest", out var labelObj) && labelObj is string label))
        {
            r.State = "none";
            return r;
        }

        var manifests = Get(root, "manifests") as Dictionary<string, object>;
        var m = manifests != null && manifests.TryGetValue(label, out var mo) ? mo as Dictionary<string, object> : null;
        if (m != null)
        {
            r.Issuer = Get(Get(m, "signature_info"), "issuer") as string ?? "";
            if (Get(m, "claim_generator_info") is List<object> gi && gi.Count > 0)
                r.Generator = Get(gi[0], "name") as string ?? "";
            if (r.Generator == "" && Get(m, "claim_generator") is string cg)
                r.Generator = cg.Split(' ')[0];
            r.AiDeclared = ContainsText(Get(m, "assertions"), "trainedAlgorithmicMedia");
        }

        // 실패 코드: 새 형식(validation_results) → 없으면 옛 형식(validation_status)
        var codes = new List<string>();
        var fails = Get(Get(Get(root, "validation_results"), "activeManifest"), "failure") as List<object>
                    ?? Get(root, "validation_status") as List<object>;
        if (fails != null)
            foreach (var f in fails)
                if (Get(f, "code") is string c && !codes.Contains(c)) codes.Add(c);
        r.Failures = string.Join(" ", codes.FindAll(c => c != "signingCredential.untrusted"));

        // 상태: c2patool이 준 값을 우선 사용, 옛 버전이라 없으면 실패 코드로 판단
        r.State = Get(root, "validation_state") as string
                  ?? (r.Failures == "" ? "Valid" : "Invalid");
        if (r.State == "Trusted" && codes.Contains("signingCredential.untrusted")) r.State = "Valid";
        return r;
    }

    static object Get(object o, string key) =>
        o is Dictionary<string, object> d && d.TryGetValue(key, out var v) ? v : null;

    static bool ContainsText(object o, string needle)
    {
        switch (o)
        {
            case string s: return s.IndexOf(needle, StringComparison.Ordinal) >= 0;
            case List<object> l: foreach (var x in l) if (ContainsText(x, needle)) return true; return false;
            case Dictionary<string, object> d: foreach (var x in d.Values) if (ContainsText(x, needle)) return true; return false;
            default: return false;
        }
    }

    static string FirstLine(string s)
    {
        s = (s ?? "").Trim();
        int i = s.IndexOf('\n');
        return i < 0 ? s : s.Substring(0, i);
    }
}

// 최소 JSON 파서: object → Dictionary<string,object>, array → List<object>,
// 숫자 → double, true/false → bool, null → null. (Unity JsonUtility는 Dictionary를 못 읽어서 직접 구현)
public static class MiniJson
{
    public static object Parse(string s)
    {
        int i = 0;
        var v = Value(s, ref i);
        Ws(s, ref i);
        if (i != s.Length) throw new FormatException("trailing data at " + i);
        return v;
    }

    static void Ws(string s, ref int i) { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }

    static object Value(string s, ref int i)
    {
        Ws(s, ref i);
        if (i >= s.Length) throw new FormatException("unexpected end");
        char c = s[i];
        if (c == '{') return Obj(s, ref i);
        if (c == '[') return Arr(s, ref i);
        if (c == '"') return Str(s, ref i);
        if (Lit(s, ref i, "true")) return true;
        if (Lit(s, ref i, "false")) return false;
        if (Lit(s, ref i, "null")) return null;
        return Num(s, ref i);
    }

    static bool Lit(string s, ref int i, string w)
    {
        if (string.CompareOrdinal(s, i, w, 0, w.Length) != 0) return false;
        i += w.Length;
        return true;
    }

    static Dictionary<string, object> Obj(string s, ref int i)
    {
        var d = new Dictionary<string, object>();
        i++; Ws(s, ref i);
        if (s[i] == '}') { i++; return d; }
        while (true)
        {
            Ws(s, ref i);
            string k = Str(s, ref i);
            Ws(s, ref i);
            if (s[i++] != ':') throw new FormatException("':' expected at " + (i - 1));
            d[k] = Value(s, ref i);
            Ws(s, ref i);
            char c = s[i++];
            if (c == '}') return d;
            if (c != ',') throw new FormatException("',' or '}' expected at " + (i - 1));
        }
    }

    static List<object> Arr(string s, ref int i)
    {
        var l = new List<object>();
        i++; Ws(s, ref i);
        if (s[i] == ']') { i++; return l; }
        while (true)
        {
            l.Add(Value(s, ref i));
            Ws(s, ref i);
            char c = s[i++];
            if (c == ']') return l;
            if (c != ',') throw new FormatException("',' or ']' expected at " + (i - 1));
        }
    }

    static string Str(string s, ref int i)
    {
        if (s[i] != '"') throw new FormatException("'\"' expected at " + i);
        i++;
        var sb = new StringBuilder();
        while (true)
        {
            char c = s[i++];
            if (c == '"') return sb.ToString();
            if (c != '\\') { sb.Append(c); continue; }
            char e = s[i++];
            switch (e)
            {
                case 'n': sb.Append('\n'); break;
                case 't': sb.Append('\t'); break;
                case 'r': sb.Append('\r'); break;
                case 'b': sb.Append('\b'); break;
                case 'f': sb.Append('\f'); break;
                case 'u': sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber)); i += 4; break;
                default: sb.Append(e); break; // \" \\ \/
            }
        }
    }

    static double Num(string s, ref int i)
    {
        int start = i;
        while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
        if (i == start) throw new FormatException("unexpected '" + s[i] + "' at " + i);
        return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
    }
}
