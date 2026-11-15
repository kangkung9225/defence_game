# [사운드초안] 전투 메인 BGM 및 타격/소환 효과음 제안 (호걸)

## 1. [필수 첨부] 프로토타입 효과음 샘플 파일 (.wav)

프로젝트 규격(`16bit, 44.1kHz` WAV)에 맞춘 테스트용 프로토타입 효과음 3종을 저장소에 함께 첨부했습니다.

- UI 버튼 클릭음: [sfx_ui_click.wav](./audio/sfx_ui_click.wav) (`docs/drafts/week1/audio/sfx_ui_click.wav`)
- 유닛 소환 팝 사운드: [sfx_unit_spawn.wav](./audio/sfx_unit_spawn.wav) (`docs/drafts/week1/audio/sfx_unit_spawn.wav`)
- 근접 타격 임팩트 사운드: [sfx_hit_impact.wav](./audio/sfx_hit_impact.wav) (`docs/drafts/week1/audio/sfx_hit_impact.wav`)

---

## 2. [음향 분위기] BGM 및 SFX 방향성

### 2-1. 배경음악 (BGM - MP3 / OGG 루프 규격)
- 로비 / 편성 화면: 따뜻하고 포근한 아코디언, 우쿨렐레 또는 밝은 피아노 중심의 경쾌한 동네 제과점 테마(BPM 105~115).
- 일반 전투 스테이지 (Stage 1~4, 6~9): 맑고 활기찬 행진곡풍 어드벤처/전투 루프(BPM 130~140). 반복 청취 시 귀가 피로하지 않도록 고음이 날카롭지 않고 통통 튀는 베이스 리듬이 살아있는 트랙 위주.
- 보스전 (Stage 5, Stage 10) & 피버 타임: 기존 멜로디보다 템포와 박진감이 오른(BPM 145~160) 브라스·드럼 중심의 코믹 긴박 테마. 피버 타임 발동 시에는 짧은 팡파르 스팅거(1초)를 얹어 고조감을 준다.
- 승리 / 패배 징글 (3초 내외 단발음):
  - 승리: 밝은 트럼펫/실로폰 팡파르 ("따라란~ 빠밤!").
  - 패배: 튜브/바순이 아래로 꺾이는 코믹한 아쉬움 효과음 ("뿌우~웅...").

### 2-2. 핵심 효과음 (SFX - 16bit / 44.1kHz WAV 규격)
단기 개발에서도 게임의 손맛(타격감)을 가장 빠르게 끌어올릴 수 있는 필수 SFX 10종 리스트입니다.

| 분류 | 이벤트명 | 음향 질감 설명 | 우선순위 |
| --- | --- | --- | --- |
| UI | 버튼 클릭 (`ui_click`) | 가볍고 맑은 나무 블록 두드리는 소리 | 최상 |
| UI | 소환 불가 / 에러 (`ui_error`) | 낮고 짧은 둔탁음 ("뾱 / 띡" 거절음) | 최상 |
| UI | 지갑 업그레이드 (`wallet_up`) | 동전 찰랑임 + 상승 차임벨 | 상 |
| 전투 | 유닛 소환 (`unit_spawn`) | 오븐에서 갓 구워져 튀어나오는 "퐁!" 팝 사운드 | 최상 |
| 전투 | 근접 타격 (`hit_melee`) | 묵직하지만 귀엽게 튕기는 타격음(칼/식빵/포크) | 최상 |
| 전투 | 원거리 발사/피격 (`hit_ranged`) | "휙~" 날아가는 소리와 크림/도토리 맞는 "톡/퍽" 소리 | 상 |
| 전투 | 피버 발동 (`fever_on`) | 불꽃 타오르는 소리와 밝은 오븐 종소리("땡!") | 상 |
| 전투 | 전체 폭격 아이템 (`item_bomb`) | 휘파람 낙하음 + 경쾌한 대폭발 "콰광!" | 중 |
| 전투 | 기지 수리 아이템 (`item_heal`) | 반짝이는 회복 차임벨 + 반죽 부풀어 오르는 소리 | 중 |
| 전투 | 기지 피격 / 파괴 (`base_hit`, `base_break`) | 벽돌/나무 부딪히는 묵직한 충격음 및 붕괴음 | 상 |

---

## 3. [무료 에셋 링크] (CC0 / 퍼블릭 도메인 검증 완료)

별도 저작권 표기 없이(CC0 Public Domain) 상업적·비상업적 Unity 프로젝트에 바로 임포트해 가공할 수 있는 에셋 소스 목록입니다.

1. UI 및 인터페이스 클릭/에러/팝 사운드
   - Kenney UI Audio (CC0): https://kenney.nl/assets/ui-audio
   - Kenney Interface Sounds (CC0): https://kenney.nl/assets/interface-sounds
2. 타격 / 충돌 / 폭격 임팩트 사운드
   - Kenney Impact Sounds (CC0): https://kenney.nl/assets/impact-sounds
   - Kenney 오디오 전체 카탈로그 (CC0): https://kenney.nl/assets?q=audio
3. 루프 전투 BGM 및 보스 테마 (CC0 / CC-BY 필터 지원)
   - OpenGameArt.org 오디오 검색: https://opengameart.org
   - 참고 CC0 검색 키워드: "8-Bit Battle Loop", "Boss Battle Loop", "TinyWorlds", "Cheerful Battle Loop" (Audio 탭 -> License: CC0 필터 적용 시 무료 루프 트랙 다수 확보 가능)

---

## 4. 사운드 최적화 및 구현 규칙 (메모)
- 동일 프레임에 유닛 5~10마리가 동시에 타격할 때 소리가 찢어지지(Clipping) 않도록, 동일 `AudioClip` 재생 시 최소 재호출 간격(예: `0.05초` 쿨다운) 또는 동시 재생 상한(Max Voices 4개)을 `SoundManager`에 두는 구조를 함께 제안한다.
