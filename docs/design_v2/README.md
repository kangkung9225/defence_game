# design_v2 — 「인류의 불씨」 v2 단독 제출 패키지

> **v2(공격형 라인 디펜스)만 담은 업로드용 패키지**입니다.
> v1(수호형·두 기둥) 자료는 포함하지 않습니다. v2에 필요한 재사용 수치는 `reference/`에 정리했습니다.
> © 2026 lazycat0813. All rights reserved. (팀 평가/선정용 · 선정 시 협의 라이선스로 공개)

---

## 바로 보기
| 용도 | 파일 |
|---|---|
| **제출용 기획초안** | [submission/planning_draft.md](./submission/planning_draft.md) |
| 3분 요약 | [submission/summary.md](./submission/summary.md) |
| 제출 체크리스트 | [submission/submission_checklist.md](./submission/submission_checklist.md) |
| **v2 통합 정본** | [design/master_design_v2.md](./design/master_design_v2.md) |
| **세계관 설정집** | [design/worldbuilding_v2.md](./design/worldbuilding_v2.md) |
| **125스테이지 스토리라인** | [design/stage_storyline_v2.md](./design/stage_storyline_v2.md) |
| 밸런스 확정 | [design/combat_balance_v2.md](./design/combat_balance_v2.md) |
| UI/목업 가이드 | [design/wireframe_ui_v2.md](./design/wireframe_ui_v2.md) |
| 화면 PNG 7종(참고용) | [ui/mockups/](./ui/mockups/) |
| 밸런스 시뮬레이터 | [tools/V2Sim.cs](./tools/V2Sim.cs) |
| 라이선스 | [LICENSE](./LICENSE) |

---

## 폴더 구조
```
design_v2/
├─ README.md                      ← 이 파일
├─ submission/
│  ├─ planning_draft.md           ← [기획초안] 제출 본문(원본)
│  ├─ issue_body.md               ← 이슈 게시용 본문(H1 제거)
│  ├─ summary.md                  ← 3분 요약
│  └─ submission_checklist.md     ← 일정·이슈·첨부 체크리스트
├─ design/
│  ├─ master_design_v2.md         ← v2 통합 정본
│  ├─ worldbuilding_v2.md         ← 세계관 설정집
│  ├─ combat_balance_v2.md        ← 전투·경제·난이도 확정 수치
│  ├─ stage_storyline_v2.md       ← 5챕터 125스테이지 스토리라인
│  └─ wireframe_ui_v2.md          ← 화면 흐름·전투 HUD
├─ reference/
│  ├─ traits_and_grades.md        ← 특성·등급·적/보스
│  ├─ trait_details.md            ← 특성 상세 수치
│  ├─ combine_and_resonance.md    ← 결합·공명 규칙
│  ├─ heroes_and_enemies.md       ← 영웅·적 상성
│  ├─ economy.md                  ← 골드 경제
│  ├─ currency_shop_iap.md        ← 재화·상점·IAP
│  └─ progression_and_items.md    ← 성장·아이템
├─ ui/
│  ├─ wireframe_mockups.html      ← 목업 원본(브라우저 캡처용)
│  └─ mockups/screen1~7.png       ← 1920×1080 PNG
├─ tools/
│  └─ V2Sim.cs                    ← 80회/스테이지 밸런스 시뮬레이터
└─ LICENSE
```

---

## 핵심 요약
- **장르**: 2D 횡스크롤 공격형 라인 디펜스
- **승리**: 적 본진(AI 코어) HP 0 / **패배**: 아군 본진(나무) HP 0
- **🔥 불꽃(Ember)**: 전진 공격·병력 → 적 코어 공략
- **🌱 새싹(Verdant)**: 본진 방어선 고정 · **해킹 드론 요격** · **인구/생산**
- **캠페인**: **5챕터 125스테이지** (챕터당 25 = 5 ACT × 5)
- **보스**: 5스테이지마다 미니보스, 25스테이지마다 챕터 보스 (미니 20 + 챕터 5)
- **검증(Chapter 1, 80회)**: s24 **61.3%** / 전부 공격 **0%** / 전부 방어 **0%**

---

## 업로드 상태: ✅ 게시 완료

**이슈 [#9](https://github.com/kangkung9225/defence_game/issues/9)** — 2026-11-19 게시 (작성자 `lazycat0813`, 이미지 미첨부)

1. GitHub `kangkung9225/defence_game` → **Issues** → **New issue**
2. 제목:
   ```
   [기획초안] 세계관 컨셉 및 아군/적군 캐릭터 기믹 제안 (lazycat0813)
   ```
3. 본문: **`submission/issue_body.md` 전체** (제목 중복 H1 제거본)
4. 이미지 첨부는 **선택**입니다. 이번 제출은 **본문만** 올립니다.
   - 참고용 목업: `ui/mockups/` (미첨부)
5. 일정·체크리스트: [submission/submission_checklist.md](./submission/submission_checklist.md)

> 이 패키지 안의 문서는 `reference/`까지 포함해 **v1 파일 없이 단독으로 읽을 수 있습니다.**

---

## 라이선스
- [LICENSE](./LICENSE) — 저작권 **lazycat0813** 귀속.
- 팀 평가/선정 목적 전용. **프로젝트 채택/선정 시** 협의 라이선스로 공개.
