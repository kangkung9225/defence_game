# 🏰 2D 디펜스 게임 (냥코대전쟁 스타일 미니 게임)

> **앱 개발 원정대** 팀원들이 함께 만드는 2~4주 단기 2D 디펜스 미니 게임 프로젝트입니다.  
> Unity 6 + VS Code + 각자의 AI 도구(Claude, GPT, Antigravity 등)를 활용하여 공동 개발하고 최종적으로 구글 플레이스토어 출시를 목표로 합니다.

---

## 📌 1. 프로젝트 기본 정보
- **개발 기간**: 2주 ~ 4주 단기 개발
- **엔진 버전**: **Unity 6.6 (`6000.6.9f1`)** *(소수점까지 버전 통일 필수)*
- **렌더 파이프라인**: 2D URP (Universal Render Pipeline)
- **대상 플랫폼**: Android (Google Play Store 출시 목표) & PC Editor 테스트
- **게임 방식**: 싱글 플레이 기반 횡스크롤 2D 라인 디펜스 (로컬 데이터 저장)

---

## 🛠 2. 개발 환경 필수 설치 안내

### ① 일반 개발 참여자 (최소 용량: 약 4GB만 필요)
- **Unity Hub & Unity Editor**: `Unity 6.6 (6000.6.9f1)`
  - ❌ `Visual Studio Community` 체크 해제 (VS Code 사용)
  - ❌ `Android Build Support` 체크 해제 (PC 에디터 ▶ Play 버튼으로 테스트/디버깅 100% 가능)
  - ❌ `Documentation` 체크 해제
- **Git & GitHub Desktop**: [GitHub Desktop](https://desktop.github.com/) 설치 후 본인 계정 로그인
- **VS Code**: [VS Code](https://code.visualstudio.com/) 설치 후 확장탭에서 **`Unity`** (Microsoft 제작) 확장팩 설치
- **.NET 8.0 SDK**: [.NET 8.0 다운로드](https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0) 설치

### ② 빌드 담당자 (방장 PC)
- 위 항목에 더해 **`Android Build Support`** (하위 OpenJDK, Android SDK & NDK Tools 포함) 추가 설치 완료

---

## 🚀 3. 프로젝트 시작하기 (처음 합류하는 팀원)

1. **저장소 복제 (Clone)**
   - GitHub Desktop 실행 → `File` → `Clone Repository...`
   - URL 탭에서 `https://github.com/kangkung9225/defence_game.git` 입력 후 로컬에 저장
2. **브랜치를 `dev`로 변경**
   - 상단 `Current branch`를 클릭하고 **`dev`** 브랜치로 전환합니다.
3. **Unity 프로젝트 열기**
   - Unity Hub 실행 → `프로젝트` → `추가` → `디스크에서 프로젝트 추가` 클릭
   - 방금 Clone 받은 `defence_game` 폴더 선택 후 `Unity 6.6 (6000.6.9f1)`으로 실행

---

## 🌿 4. Git 협업 규칙 & 브랜치 전략

우리 프로젝트는 **`main`**과 **`dev`** 2개의 핵심 브랜치로 운영됩니다.

- **`main` (정식/배포용)**: 항상 버그 없이 빌드 가능한 완전한 안정 버전만 보관 (방장 관리)
- **`dev` (공동 개발/테스트용)**: 팀원들이 개발한 기능이 모이는 기본 작업 브랜치

### 🔄 일일 안전 작업 루틴 (4단계)
1. **작업 시작 전: 항상 `Pull` 먼저 하기**
   - Unity 에디터를 켜기 **전**에 GitHub Desktop에서 **`Fetch origin` → `Pull origin`**을 눌러 팀원들의 최신 작업을 먼저 내려받습니다.
2. **작업 브랜치 만들기**
   - `dev` 브랜치에서 본인의 기능 브랜치를 만듭니다. (예: `feat/spawn-button`, `feat/cat-unit`, `design/ui`)
3. **작업 완료 후: 커밋 및 푸시**
   - Unity 에디터를 닫거나 저장한 상태에서 본인 브랜치로 `Commit & Push` 합니다.
4. **Pull Request (PR) 보내기**
   - GitHub 웹사이트에서 본인 브랜치 → **`dev` 브랜치로 PR**을 생성합니다.
   - 단톡방에 *"기능 PR 올렸습니다!"* 공유 후 병합(Merge)합니다.

---

## 🤖 5. AI 활용 가이드 (Claude, GPT, Cline 등)

모든 팀원이 AI를 활용해 코드를 작성할 때 프로젝트가 꼬이지 않도록 아래 규칙을 지켜주세요.

1. **Unity 메타파일(`.meta`) 보호 (가장 중요!)**
   - 파일 생성, 이동, 이름 변경, 삭제는 **AI 터미널로 하지 말고 Unity 에디터 프로젝트 창에서 직접** 해주세요.
   - 그래야 `.meta` 파일이 손상되지 않고 다른 팀원 PC에서도 정상 인식됩니다.
2. **코드 기반 자동 조립 지향**
   - 수동으로 Hierarchy 창에서 드래그 앤 드롭하지 않아도 되도록, 코드가 `Start()`나 `Awake()`에서 스스로 컴포넌트와 이벤트를 연결하는 방식(`AddListener`, `GetComponent` 등)을 권장합니다.
3. **상세한 개발/코딩 규칙**: [DEVELOPMENT_GUIDE.md](./DEVELOPMENT_GUIDE.md)를 참고해 주세요.

---

## 📁 6. 폴더 구조 가이드

```
Assets/
├── Scenes/          # 게임 씬 파일 (SampleScene, TitleScene, BattleScene 등)
├── Scripts/         # C# 스크립트 모음
│   ├── Core/        # 게임 루프, 웨이브/매니저 시스템
│   ├── Units/       # 아군/적군 유닛 로직, 스탯, FSM
│   ├── UI/          # 코스트 게이지, 소환 버튼, 결과창
│   └── Data/        # 유닛/스킬 ScriptableObject 데이터
├── Sprites/         # 2D 스프라이트 (.png) 이미지
├── Audio/           # BGM 및 효과음 (.mp3, .wav)
└── Prefabs/         # 프리팹 (유닛, 투사체, UI 팝업 등)
```

---

## 👥 팀 역할
- **총괄/배포**: 호걸 (Google Play Store 출시, 브랜치 관리, 빌드)
- **개발/디자인/기획**: 앱 개발 원정대 팀원 전원
