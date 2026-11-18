# design_v2/design — 공격형 라인 디펜스 (제출 기준)

> 작성일: 2026-11-15 · 상태: **v2 확정 (밸런스 시뮬 검증 완료)** · 작성: lazycat0813
> © 2026 lazycat0813. All rights reserved. (평가/선정용 · 선정 시 협의 라이선스로 공개)

## 이 폴더가 v2 정본입니다
이 패키지는 **v2 단독**입니다. v1(수호형 · 두 기둥) 자료는 포함하지 않습니다.
제출·구현 기준은 **v2**입니다.

## v2 한 줄 정의
> **아군 본진(🌱나무)을 지키며 🔥불꽃 군단으로 적 AI 코어를 부수는 공격형 라인 디펜스.**

## 핵심 구조
| 진영 | 역할 |
|---|---|
| 🔥 불꽃 | 전진 공격·병력 (적 본진 공략) |
| 🌱 새싹 | 본진 방어선 고정 · **해킹 드론 요격** · **인구/생산** 관리 |

- **승리** = 적 본진 파괴 / **패배** = 아군 본진 파괴
- 인구(생산력)↑ → 코스트 회복·생산↑
- 적 지상 유닛은 본진으로 진격, **해킹 드론은 유닛을 무시하고 본진 직행** → 새싹만 요격 가능
- **동시 소환 상한 12기**로 눈덩이(데스볼) 방지
- **캠페인**: 5챕터 125스테이지(25×5) — [stage_storyline_v2.md](./stage_storyline_v2.md)

## 확정 밸런스 한눈에
> 아래 수치/공식은 **24스테이지 시뮬(Chapter 1 일반 구간) 기준**입니다. s25 챕터 보스와 Chapter 2~5는 챕터 스케일([combat_balance_v2.md](./combat_balance_v2.md) §11)로 확장합니다.

| 항목 | 값 |
|---|---|
| 아군 본진 | 2000 |
| 적 본진 | 4000 × (1+0.20(s−1)) |
| 적 HP / ATK | ×(1+0.13(s−1)) / ×(1+0.08(s−1)) |
| 적 스폰 | max(1.0, 2.4−0.05(s−1)) s |
| 해킹 드론 | 3스폰마다 1기, 새싹만 요격 |
| 기대 성장 | 7.5%/스테이지 |

**Chapter 1 검증 곡선(80회/스테이지)**: s1 100% → s18 100% → s20 95% → s22 82.5% → **s24 61.3%** (s25 챕터 보스)
**역할 검증**: 전부 공격 0% · 전부 방어 0% · 균형(새싹 3기) 61.3%

## 문서
- [master_design_v2.md](./master_design_v2.md) — **v2 통합 정본**
- [worldbuilding_v2.md](./worldbuilding_v2.md) — **세계관 설정집(제로·마더 트리·진영·지역)**
- [combat_balance_v2.md](./combat_balance_v2.md) — 전투·경제·난이도 확정 수치 및 검증 결과
- [stage_storyline_v2.md](./stage_storyline_v2.md) — **5챕터 125스테이지 스토리라인**
- [wireframe_ui_v2.md](./wireframe_ui_v2.md) — 화면 흐름·전투 HUD·목업 캡처 가이드
- 검증 도구: [`../tools/V2Sim.cs`](../tools/V2Sim.cs)

## v2 재사용 수치 (reference/)
- 특성/등급/성향/결합/영웅: `../reference/traits_and_grades.md`, `../reference/trait_details.md`, `../reference/combine_and_resonance.md`, `../reference/heroes_and_enemies.md`
- 경제/재화: `../reference/economy.md`, `../reference/currency_shop_iap.md`
- 성장/아이템: `../reference/progression_and_items.md`
- 화면/목업: [`wireframe_ui_v2.md`](./wireframe_ui_v2.md), [`../ui/mockups/`](../ui/mockups/) (v2로 수정 완료)

## 완료/다음
- ✅ 전투 HUD 목업 v2 수정(좌 아군 본진 / 우 적 본진 / 드론 경고)
- ✅ 제출 문서(`../submission/`) v2 갱신
- ⬜ 인게임 구현
