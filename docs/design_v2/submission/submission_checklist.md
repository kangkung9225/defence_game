# 제출 체크리스트 — 「인류의 불씨」 v2

> 기준일: 2026-11-15 · 작성: lazycat0813
> © 2026 lazycat0813. All rights reserved. (팀 평가/선정용 · 선정 시 협의 라이선스로 공개)

---

## 1. 일정
| 단계 | 마감 | 상태 |
|---|---|---|
| 참여자 기본 댓글 | **11/15(일) 22:00** | ⬜ |
| `[기획초안]` 이슈 등록 | **11/19(목) 12:00** | ✅ **#9 게시 완료** |
| 팀 투표 참여 | **11/21(토) 22:00** | ⬜ |

## 2. 제출물 준비 상태
| 항목 | 파일 | 상태 |
|---|---|---|
| 기획초안(필수 4항목 + 유닛 표) | [planning_draft.md](./planning_draft.md) | ✅ |
| 3분 요약 | [summary.md](./summary.md) | ✅ |
| v2 통합 정본 | [../design/master_design_v2.md](../design/master_design_v2.md) | ✅ |
| 세계관 설정집 | [../design/worldbuilding_v2.md](../design/worldbuilding_v2.md) | ✅ |
| v2 밸런스 확정 | [../design/combat_balance_v2.md](../design/combat_balance_v2.md) | ✅ |
| 5챕터 125스테이지 스토리라인 | [../design/stage_storyline_v2.md](../design/stage_storyline_v2.md) | ✅ |
| v2 UI 정의 | [../design/wireframe_ui_v2.md](../design/wireframe_ui_v2.md) | ✅ |
| 전투 시뮬레이터 | [../tools/V2Sim.cs](../tools/V2Sim.cs) | ✅ |
| 화면 목업 7종(1920×1080, 참고용) | [../ui/mockups/](../ui/mockups/) | ✅ |
| 라이선스 | [../LICENSE](../LICENSE) | ✅ |
| GitHub 이슈 게시 | [#9](https://github.com/kangkung9225/defence_game/issues/9) · [issue_body.md](./issue_body.md) | ✅ |
| 이미지 첨부 | — | — (이번 제출 미첨부) |

## 3. 이슈 등록 결과

**게시 완료 — [#9](https://github.com/kangkung9225/defence_game/issues/9)**
- 작성자: `lazycat0813` · 상태: open · 이미지 미첨부
- 제목 검증: `[기획초안] 세계관 컨셉 및 아군/적군 캐릭터 기믹 제안 (lazycat0813)` (45자, 일치)
- 본문: [issue_body.md](./issue_body.md) (7,328자, 제목 중복 H1 제거본)

### 수동 등록 절차 (참고)
1. GitHub `kangkung9225/defence_game` → **Issues** → **New issue**
2. 제목:
   ```
   [기획초안] 세계관 컨셉 및 아군/적군 캐릭터 기믹 제안 (lazycat0813)
   ```
3. 본문: **`issue_body.md` 전체를 복사·붙여넣기**
4. 이미지 첨부는 **선택**입니다. 이번 제출은 **본문만** 올립니다.
   - 참고용 목업은 `../ui/mockups/`에 보관(미첨부).
5. 등록 후 링크를 팀에 공유

> GitHub 이슈 본문은 마크다운을 지원하므로 `planning_draft.md`의 표·코드블록이 그대로 렌더링됩니다.

## 4. 제출 규격 자체 점검
- [x] 배경 스토리·대립 구조 (3~5줄)
- [x] **세계관 상세(설정집 링크 + 요약)**
- [x] 아군/적군 페르소나 (외형·공격 방식)
- [x] 화면 흐름도 + **소환 코스트/쿨타임 판정 흐름**
- [x] **[필수] 유닛 스탯/코스트/쿨타임 테이블**
- [x] 적/보스 스탯 + 난이도 공식
- [x] 저작권·라이선스(선정 시 공개)
- [x] 1920×1080 목업 7종(참고용, 미첨부)

## 5. 계정/권한
- 작성자 GitHub: **lazycat0813**
- 레포 소유자: **kangkung9225**
- 레포가 public이므로 `lazycat0813` 계정으로 **이슈 등록 가능**
- 파일 PR은 **협업자 초대** 또는 **포크 후 PR** 필요
- 저장소에 파일을 올리지 않고 **이슈 본문 + 이미지 첨부**만으로 제출 가능
- 이슈 게시는 **GitHub REST API**로 처리했습니다(`lazycat0813` 소유의 classic PAT, `public_repo`).
  - 토큰 파일: `C:\Users\mkh08\AppData\Local\Temp\opencode\gh_pat.txt` (**로컬 전용 · 레포에 커밋 금지**)
  - ⚠️ Windows PowerShell 5.1은 `.ps1` 안의 한글 리터럴을 ANSI로 읽어 깨뜨립니다. 한글 문자열은 **UTF-8 파일에서 읽어** 사용하세요.

## 6. 참여자 기본 댓글 템플릿
> 아래 `{{ }}`를 채워서 사용하세요.

```
안녕하세요, lazycat0813입니다.
2D 라인 디펜스 프로젝트에 참여하고 싶습니다.

- 가능한 요일/시간: {{예: 평일 20:00~24:00, 주말 13:00~22:00}}
- 메인 파트: {{기획·사운드 / 아트·UI / 개발}}
- 관심 분야: 세계관·유닛 기믹·밸런스 설계, UI 목업
- 제안 초안: [기획초안] 세계관 컨셉 및 아군/적군 캐릭터 기믹 제안 (lazycat0813)
- 초안 링크: https://github.com/kangkung9225/defence_game/issues/9
```

## 7. 남은 입력 (작성자 확인 필요)
- [ ] 가능한 요일·시간대
- [ ] 메인 파트 (기획·사운드 / 아트·UI / 개발)
- [x] GitHub 이슈 등록 후 URL → https://github.com/kangkung9225/defence_game/issues/9

## 8. 참고 링크
- 레포: https://github.com/kangkung9225/defence_game
- 제출 매뉴얼: [https://github.com/kangkung9225/defence_game/blob/main/docs/workflow/guidelines.md](https://github.com/kangkung9225/defence_game/blob/main/docs/workflow/guidelines.md)
- 프로젝트 방향: https://github.com/kangkung9225/defence_game/blob/main/DEVELOPMENT_GUIDE.md
