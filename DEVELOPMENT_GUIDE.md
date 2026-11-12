# 📜 디펜스 게임 개발 가이드 & AI 협업 룰북

본 문서는 팀원 및 각 팀원이 사용하는 **AI(Claude, ChatGPT, Cline, Cursor 등)**가 공통적으로 참고해야 하는 개발 가이드라인입니다.  
AI에게 코딩을 맡길 때 **"이 문서(DEVELOPMENT_GUIDE.md)를 참고해서 개발해 줘"**라고 프롬프트에 전달하면 코드가 일관되게 작성됩니다.

---

## 🎯 1. 게임 구조 및 핵심 규칙

- **장르**: 2D 횡스크롤 라인 디펜스 (냥코대전쟁 스타일)
- **게임 루프**:
  1. 시간에 따라 **재화(코스트)**가 자동으로 차오름
  2. 코스트를 소모해 아군 유닛을 소환
  3. 아군 유닛은 오른쪽으로, 적군 유닛은 왼쪽으로 자동 이동
  4. 사거리 안에 적/아군/기지가 들어오면 정지 후 공격
  5. 상대방 기지(Base)를 먼저 파괴하면 승리

---

## 💻 2. C# 코딩 규칙 (AI 프롬프트용 표준)

1. **에디터 드래그 최소화 (Code-Driven Architecture)**
   - 인스펙터 창에서 빈칸에 일일이 드래그해서 연결하는 방식을 지양하고, 코드 내부에서 `GetComponent<T>()` 또는 `Awake()` 시점에 스스로 초기화하도록 작성합니다.
   - UI 버튼 이벤트도 인스펙터 `OnClick` 대신 `button.onClick.AddListener(MethodName)` 코드로 연결합니다.
2. **이름 규칙 (Naming Conventions)**
   - 클래스/구조체/메서드: `PascalCase` (예: `UnitController`, `SpawnUnit()`)
   - private 필드: `_camelCase` (예: `private float _moveSpeed;`)
   - public 프로퍼티: `PascalCase` (예: `public float CurrentHp { get; private set; }`)
   - 상속 구조: Unity 기본 기능은 `MonoBehaviour`를 상속하되, 데이터 저장은 `ScriptableObject`를 적극 활용합니다.
3. **씬(Scene) 파일 직접 수정 주의**
   - 여러 명이 `SampleScene.unity`를 동시에 수정하면 병합 충돌(Conflict)이 발생하여 파일이 손상됩니다.
   - 가급적 기능 단위는 **`Prefabs/` (프리팹)** 형태로 제작하여 분리하고, 씬 파일 자체를 직접 건드릴 때는 단톡방에 사전 공유합니다.

---

## 🎨 3. 2D 리소스 (스프라이트/이미지) 규격

- **형식**: 투명 배경의 `.png` 권장
- **위치**: `Assets/Sprites/` 아래 카테고리별 저장 (예: `Assets/Sprites/Units/`, `Assets/Sprites/UI/`)
- **Unity 텍스처 설정 (표준)**:
  - `Texture Type`: **Sprite (2D and UI)**
  - `Sprite Mode`: Single (단일 이미지) 또는 Multiple (스프라이트 시트)
  - `Pixels Per Unit (PPU)`: 기본 **100**으로 통일 (도트 그래픽일 경우 필요 시 16 또는 32 통일)
  - `Filter Mode`: 일반 일러스트는 `Bilinear`, 픽셀아트/도트일 경우 `Point (no filter)`

---

## 🛡 4. Git 커밋 메시지 규칙

커밋 메시지는 아래 접두사를 붙여 한눈에 변경 사항을 파악할 수 있게 작성합니다.

- `feat:` 새로운 기능 추가 (예: `feat: 아군 고양이 유닛 이동 및 공격 로직 추가`)
- `fix:` 버그 수정 (예: `fix: 기지 파괴 후에도 적이 계속 소환되는 버그 수정`)
- `design:` 이미지, 사운드 등 에셋 리소스 추가 (예: `design: 타워 및 기지 스프라이트 추가`)
- `refactor:` 코드 구조 개선 (기능 변화 없음)
- `docs:` 문서 수정 (README 등)
