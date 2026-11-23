// Assets/Editor/AssetSourceLogger.cs
// 에셋 출처 기록: AssetLog/asset_log.csv (프로젝트 루트, Assets 밖 → 재임포트 루프 없음)
//
// 규칙 (모두 "경로" 기준, CSV는 git으로 팀 공유)
//   ① 이동   : movedFrom 경로의 줄을 찾아 moved 경로로 바꾼다 (폴더 이동이면 안의 줄도 같이)
//   ② 새 파일 : CSV에 없는 경로면 줄을 추가한다 (imported + moved 둘 다 확인)
//   ③ 삭제   : deleted 경로의 줄을 지운다 (폴더 삭제면 안의 줄도 같이)
//   처리 순서는 ① → ② → ③. 같은 변경을 두 번 받아도 결과가 같다 (git pull 후 재처리 대비).
//
// 매 콜백마다 CSV를 새로 읽는다 → 메모리에 낡은 목록이 남는 문제가 없다.
// 이동·삭제는 Unity Project 창에서만 할 것 (Finder에서 원본만 옮기면 .meta가 안 따라감).
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AssetSourceLogger : AssetPostprocessor
{
    static readonly string LogDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "AssetLog");
    static readonly string LogPath = Path.Combine(LogDir, "asset_log.csv");
    static readonly string[] Columns = { "file", "date", "tool", "model", "prompt", "license", "ai_hint" };
    const int FILE = 0, DATE = 1, TOOL = 2, LICENSE = 5, HINT = 6;
    const string ToolPathPrefKey = "AssetSourceLogger.C2paToolPath"; // c2patool 위치가 다르면 여기에 직접 지정

    static readonly HashSet<string> TrackedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".psd", ".tga", ".webp",
        ".wav", ".mp3", ".ogg",
        ".fbx", ".obj"
    };

    // C2PA 서명이 없을 때만 쓰는 보조 단서 (오탐 가능)
    static readonly (string needle, string hint)[] AiMarkers =
    {
        ("trainedAlgorithmicMedia", "IPTC:AI-generated"),
        ("OpenAI", "OpenAI?"),
        ("ChatGPT", "ChatGPT?"),
        ("Midjourney", "Midjourney?"),
        ("Steps:", "StableDiffusion-params?"),
    };

    static string toolPath;
    static bool toolWarned;

    // ───────────────────────── Unity가 부르는 함수 ─────────────────────────

    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (imported.Length == 0 && deleted.Length == 0 && moved.Length == 0) return;

        var log = Load();
        bool changed = log.needsRewrite;

        // ① 이동: moved[i](새 경로)와 movedFrom[i](옛 경로)는 같은 순번끼리 짝
        for (int i = 0; i < moved.Length && i < movedFrom.Length; i++)
            changed |= MoveRows(log.rows, movedFrom[i], moved[i]);

        // ② 새 파일: 이동 뒤에 해야 "옮긴 파일"을 새 파일로 착각하지 않는다
        var known = new HashSet<string>();
        foreach (var r in log.rows) known.Add(r[FILE]);
        var candidates = new List<string>(imported);
        candidates.AddRange(moved); // 확장자를 바꿔 추적 대상이 된 파일(hero.txt → hero.png)도 잡기 위해
        foreach (var path in candidates)
        {
            if (!TrackedExt.Contains(Path.GetExtension(path)) || known.Contains(path)) continue;
            known.Add(path);
            log.rows.Add(NewRow(path));
            changed = true;
        }

        // ③ 삭제: 마지막에 한다
        foreach (var path in deleted)
            changed |= DeleteRows(log.rows, path) > 0;

        if (changed) Save(log.rows);
    }

    // ───────────────────────── ①②③ 처리 ─────────────────────────

    // from 경로(파일 또는 폴더)의 줄을 to 경로로 바꾼다. 바뀐 줄이 있으면 true
    static bool MoveRows(List<string[]> rows, string from, string to)
    {
        bool any = false;
        foreach (var r in rows)
        {
            if (r[FILE] == from) { r[FILE] = to; any = true; }
            else if (r[FILE].StartsWith(from + "/", StringComparison.Ordinal))
            {
                r[FILE] = to + r[FILE].Substring(from.Length); // 폴더 이동: 앞부분만 교체
                any = true;
            }
        }
        return any;
    }

    // path(파일 또는 폴더)의 줄을 지운다. 지운 줄 수를 돌려준다
    static int DeleteRows(List<string[]> rows, string path) =>
        rows.RemoveAll(r => r[FILE] == path || r[FILE].StartsWith(path + "/", StringComparison.Ordinal));

    static string[] NewRow(string path)
    {
        var row = new string[Columns.Length];
        for (int i = 0; i < row.Length; i++) row[i] = "";
        var (tool, hint) = Inspect(path);
        row[FILE] = path;
        row[DATE] = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        row[TOOL] = tool;          // C2PA Trusted일 때만 자동
        row[LICENSE] = "확인 필요"; // model, prompt, license는 사람이 채움
        row[HINT] = hint;
        return row;
    }

    // ───────────────────────── CSV 읽기·쓰기 ─────────────────────────

    // CSV 전체를 읽어 줄 목록으로. 헤더가 지금 형식과 다르면(옛 형식) 칸 이름으로 옮겨 담고 다시 쓰게 표시
    static (List<string[]> rows, bool needsRewrite) Load()
    {
        var rows = new List<string[]>();
        if (!File.Exists(LogPath)) return (rows, true); // 첫 실행: 헤더까지 새로 씀

        var all = ParseCsv(File.ReadAllText(LogPath, Encoding.UTF8).TrimStart('﻿'));
        if (all.Count == 0) return (rows, true);

        var head = all[0];
        bool sameHeader = head.Count == Columns.Length;
        var map = new int[Columns.Length];
        for (int i = 0; i < Columns.Length; i++)
        {
            map[i] = head.IndexOf(Columns[i]);
            if (map[i] != i) sameHeader = false;
        }
        int oldStatus = head.IndexOf("status"); // 옛 형식에서 deleted로 표시된 줄은 새 규칙대로 버림

        for (int n = 1; n < all.Count; n++)
        {
            var src = all[n];
            if (src.Count == 1 && src[0] == "") continue; // 빈 줄
            if (oldStatus >= 0 && oldStatus < src.Count && src[oldStatus] == "deleted") continue;
            var row = new string[Columns.Length];
            for (int i = 0; i < Columns.Length; i++)
                row[i] = map[i] >= 0 && map[i] < src.Count ? src[map[i]] : "";
            if (row[FILE] != "") rows.Add(row);
        }
        if (!sameHeader)
            Debug.Log("[AssetSourceLogger] 옛 형식 로그를 새 형식(" + string.Join(",", Columns) + ")으로 바꿔 저장합니다.");
        return (rows, !sameHeader);
    }

    // 파일 중간만 고치는 기능은 없으므로 항상 통째로 다시 쓴다 (CSV는 작아서 순식간)
    static void Save(List<string[]> rows)
    {
        Directory.CreateDirectory(LogDir);
        var sb = new StringBuilder();
        sb.Append(string.Join(",", Columns)).Append('\n');
        foreach (var r in rows)
        {
            for (int i = 0; i < r.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(Csv(r[i]));
            }
            sb.Append('\n');
        }
        File.WriteAllText(LogPath, sb.ToString(), new UTF8Encoding(true)); // BOM → 엑셀에서 한글 안 깨짐
    }

    // CSV 전체 파싱: 따옴표 칸 안의 쉼표·줄바꿈·"" 처리 (엑셀에서 칸 안에 줄바꿈을 넣어도 안전)
    static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var cur = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { cur.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else cur.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == ',') { row.Add(cur.ToString()); cur.Clear(); }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                row.Add(cur.ToString()); cur.Clear();
                rows.Add(row); row = new List<string>();
            }
            else cur.Append(c);
        }
        if (cur.Length > 0 || row.Count > 0) { row.Add(cur.ToString()); rows.Add(row); }
        return rows;
    }

    static string Csv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        bool quote = s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
        return quote ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    // ───────────────────────── 출처 검사 ─────────────────────────

    // 반환: (tool 칸, ai_hint 칸)
    static (string tool, string hint) Inspect(string assetPath)
    {
        string ext = Path.GetExtension(assetPath);
        if (!C2paVerifier.SupportedExt.Contains(ext)) return ("", "");
        string full = Path.GetFullPath(assetPath);

        var r = C2paVerifier.Verify(full, ResolveTool());
        if (r.State == "tool-missing" && !toolWarned)
        {
            toolWarned = true;
            Debug.LogWarning("[AssetSourceLogger] c2patool을 못 찾음. 터미널에서: brew install c2patool && c2patool init trust");
        }
        if (r.State == "error")
            Debug.LogWarning($"[AssetSourceLogger] c2pa 검증 실패: {assetPath} ({r.Failures})");

        string tool = r.Trusted ? r.Generator : "";
        string hint = r.ToHint();
        if (r.State == "none" || r.State == "tool-missing")
        {
            string scan = ScanMarkers(full);
            hint = r.State == "tool-missing" ? Join("c2patool-missing", scan) : scan;
        }
        return (tool, hint);
    }

    static string ScanMarkers(string full)
    {
        try
        {
            var info = new FileInfo(full);
            if (!info.Exists || info.Length > 20 * 1024 * 1024) return "";

            // 메타데이터는 대부분 ASCII라서 Latin1로 읽어 문자열 검색
            string text = Encoding.GetEncoding("ISO-8859-1").GetString(File.ReadAllBytes(full));
            var found = new List<string>();
            foreach (var (needle, hint) in AiMarkers)
                if (text.IndexOf(needle, StringComparison.Ordinal) >= 0 && !found.Contains(hint))
                    found.Add(hint);
            return string.Join(" ", found);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AssetSourceLogger] scan failed: {full} ({e.Message})");
            return "scan-error";
        }
    }

    // c2patool 위치: 못 찾았으면 다음 호출 때 다시 찾는다 (Unity를 켠 뒤 설치해도 인식)
    static string ResolveTool()
    {
        if (toolPath == null || !File.Exists(toolPath))
            toolPath = C2paVerifier.FindTool(EditorPrefs.GetString(ToolPathPrefKey, ""));
        return toolPath;
    }

    static string Join(string a, string b) => b == "" ? a : a + " " + b;

    // ───────────────────────── 메뉴 ─────────────────────────

    [MenuItem("Tools/Asset Log/Open log folder")]
    static void OpenFolder()
    {
        if (!File.Exists(LogPath)) Save(new List<string[]>());
        EditorUtility.RevealInFinder(LogPath);
    }

    [MenuItem("Tools/Asset Log/Check c2patool")]
    static void CheckTool()
    {
        toolPath = null; // 메뉴로 확인할 때는 항상 새로 찾는다
        if (ResolveTool() == null)
        {
            Debug.LogWarning("[AssetSourceLogger] c2patool 없음. 터미널에서: brew install c2patool && c2patool init trust");
            return;
        }
        var (code, stdout, stderr) = C2paVerifier.Run(toolPath, "--version", 5000);
        Debug.Log($"[AssetSourceLogger] {toolPath} → {stdout.Trim()} {stderr.Trim()} (exit {code})");
    }

    // 이미 있는 에셋 전부를 "새 파일"로 넘겨 CSV에 없는 것만 추가한다 (실제 재임포트는 없음)
    [MenuItem("Tools/Asset Log/Scan all existing assets")]
    static void ScanAll()
    {
        var all = AssetDatabase.GetAllAssetPaths();
        OnPostprocessAllAssets(Array.FindAll(all, p => p.StartsWith("Assets/")),
                               new string[0], new string[0], new string[0]);
        Debug.Log($"[AssetSourceLogger] scanned. log: {LogPath}");
    }
}
#endif
