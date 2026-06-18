# Alpha Readiness Plan

## Summary

이 문서는 `DreamLaundromat`을 현재의 release slice에서 내부 Alpha 후보로 올리기
위한 다음 실행 계획이다.

현재 프로젝트는 30개 fixed level, 제품형 scene, local save, tutorial, item/obstacle,
generated UI assets, Android build/run/screenshot smoke, level validation, QA/balance
report를 갖추었다. 하지만 출시 기준으로는 아직 다음 판단이 끝나지 않았다.

- 실제 30레벨이 재미있는가?
- 현재 UI/비주얼이 처음 보는 사람에게 게임처럼 보이는가?
- tap/drag 조작과 feedback이 모바일 퍼즐의 손맛을 주는가?
- `ReleaseGameController`와 UI 코드가 다음 polish와 대량 레벨 검증을 감당할 수 있는가?
- 80-120개 level로 확장할 때 후보 생성, 선별, 검증, 수동 승격 흐름이 버틸 수 있는가?

따라서 이번 계획은 “새 기능을 많이 추가하는 작업”이 아니라, Alpha Build로 넘어가기
전에 현재 게임이 진짜 게임으로 느껴지는지 검증하고, 부족한 부분을 고치는 실행
계획이다.

## Planning References

- [Full Game Roadmap](FULL_GAME_ROADMAP.md)
- [Release Gameplay Slice Plan](RELEASE_GAMEPLAY_SLICE_PLAN.md)
- [Phase 6-13 Alpha Foundation Plan](PHASE_6_13_PLAN.md)
- [Release UI Design Plan](RELEASE_UI_DESIGN_PLAN.md)
- [Direct Manipulation Game Feel V2 Plan](DIRECT_MANIPULATION_GAME_FEEL_V2_PLAN.md)
- [Release Manual Playtest Checklist](RELEASE_MANUAL_PLAYTEST_CHECKLIST.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)
- [Unity project conventions](../../docs/CONVENTIONS.md)
- [Mobile Android guidance](../../docs/MOBILE_ANDROID.md)

## Input Maturity

현재 입력 성숙도는 `Concept + prototype`이다.

- Concept: 꿈 세탁소, deterministic operation, preview fairness, assignment-first
  puzzle이라는 방향이 문서화되어 있다.
- Prototype/implementation: `Dynamic Puzzle Lab`, release gameplay scene, 30개 level,
  UI V1, Android 검증 스크립트가 존재한다.

따라서 이번 계획은 concept 탐색이 아니라, 현재 구현물을 Alpha 후보로 검증하고
조정하는 release-facing plan이다.

## Prototype Goal

이번 Alpha readiness pass의 핵심 가설:

`DreamLaundromat`은 현재 구현된 30레벨과 release UI를 기반으로, UI/비주얼 완성도와
조작 feedback을 한 단계 올린 뒤 사람이 직접 30레벨을 플레이하면 core fun과 출시
가능성의 병목을 구체적으로 판정할 수 있다.

이번 pass가 증명해야 하는 것:

- 첫 화면, level select, gameplay, result가 출시 후보 모바일 게임처럼 보인다.
- 플레이어가 긴 설명보다 카드, 주문서, storage, tool/obstacle, feedback을 보고
  판단할 수 있다.
- 30레벨 playtest에서 재미 문제와 UI/조작 문제를 분리해서 기록할 수 있다.
- 레벨 조정은 감이 아니라 solver, QA report, manual note, level intent를 함께 보고
  이루어진다.
- 80-120개 level로 확장하기 전에 level production loop의 최소 단위가 정해진다.

## Scope

포함 범위:

- current baseline 재확인
- UI/비주얼 완성도 V2
- Game Feel / Direct Manipulation V2
- `ReleaseGameController` 책임 분리와 renderer/presenter 정리
- 30레벨 수동 playtest 준비와 결과 기록 구조
- playtest 결과 기반 level/UX tuning
- 80-120레벨 production loop V1
- Alpha readiness verification gate

이번 pass는 기존 로드맵의 Phase 14 `Alpha Build` 진입 전 보강 단계다. Phase 14 자체를
release candidate로 착각하지 않는다.

## Non-Goals

이번 pass에서 제외한다.

- Google Play store listing
- release signing, keystore, secret 입력
- release AAB finalization
- monetization, ads, IAP
- analytics/crash SDK 도입
- cloud save
- localization
- final commercial illustration pack
- final app icon, splash, store screenshot
- 100개 이상의 launch level 완성
- full hint system
- liveops, daily puzzle, event content
- 모든 `DesignNotes`를 0으로 만드는 숫자 중심 작업

## Key Decisions

- 작업 문서: `DreamLaundromat/docs/ALPHA_READINESS_PLAN.md`
- 기본 PR 단위: 하나의 PR 안에서 sub-gate를 분리한다.
- merge 정책: Codex는 PR merge, `git merge`, protected branch push를 하지 않는다.
- target platform: Android portrait, one-hand touch.
- release level count 목표는 아직 확정하지 않는다. 이번 pass는 30레벨 Alpha readiness와
  80-120레벨 production loop까지 다룬다.
- UI/비주얼은 Game Feel과 분리된 독립 gate로 둔다. “게임처럼 보이는가”는 출시 기준의
  핵심 판정이다.
- 30레벨 수동 playtest는 UI/비주얼 V2와 최소 Game Feel 보강 이후 수행하는 것을
  기본값으로 한다. 낡은 화면을 기준으로 재미를 판정하면 잘못된 결론이 날 수 있다.
- `ReleaseGameController` 분리는 대규모 아키텍처 전환이 아니라, UI/입력/feedback을
  안전하게 고칠 수 있는 최소 구조 정리로 제한한다.
- 80-120레벨 production loop는 실제 120레벨 완성이 아니라, 많이 만들고 검증하고
  선별하는 반복 체계를 만드는 작업이다.

## Target Platforms

Primary:

- Android
- Portrait
- One-hand touch
- Unity build target: Android

Secondary:

- Unity Editor PlayMode
- Windows batchmode validation

Required scripts:

```powershell
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900
```

Manual platform checks:

- 실제 Android 기기 또는 emulator에서 한손 조작감 확인
- 작은 세로 화면과 긴 세로 화면에서 UI 겹침 확인
- haptic/audio 감각 확인
- 30레벨 전체 수동 playtest

## Architecture

현재 구조:

- `Thkim.DreamLaundromat.DynamicLab`
  - pure puzzle model, rules, modifier, solver, generator, metrics, validation
- `Thkim.DreamLaundromat.Gameplay.ReleaseSlice`
  - release level pack, session, progression, settings, visual descriptors, UI art,
    balance report, drag/action/feedback helpers
- `Thkim.DreamLaundromat.Editor.ReleaseSlice`
  - scene setup, validation report, balance report, UI art generation

이번 pass에서 강화할 구조:

- `ReleaseGameController`
  - scene-owned shell과 screen flow를 담당한다.
  - gameplay rendering, action availability, feedback 세부 책임을 계속 흡수하지 않게 한다.
- `ReleaseGameplayViewModel`
  - session state와 level metadata를 UI 표시 모델로 변환한다.
- `ReleaseGameplayCardRenderer`
  - dream/order/storage/tool/obstacle visual을 책임진다.
- `ReleaseActionAvailability`
  - 선택 상태와 round state를 보고 가능한 action과 disabled reason을 계산한다.
- `ReleaseFeedbackPresenter`
  - action success/fail, invalid target, clear/fail feedback을 한 곳에서 관리한다.
- `ReleaseLevelProduction`
  - 새로 만들 후보 영역이다. 80-120레벨 production loop에서 candidate, accepted level,
    manual note, validation summary를 관리할 수 있는 경량 구조를 둔다.

구현 원칙:

- rules/model은 UI에 의존하지 않는다.
- level validation은 scene 없이 가능해야 한다.
- UI polish는 solver/replay 결과를 바꾸면 안 된다.
- feedback은 rules state와 결합하지 않는다. rules 결과가 먼저 확정되고 feedback이
  따라온다.
- scene/YAML 변경은 필요한 만큼만 한다.

## Data Model

이번 pass에서 쓰거나 보강할 데이터:

- `ReleaseLevelDefinition`
  - `LevelId`
  - `DisplayName`
  - `DifficultyBand`
  - `DesignIntent`
  - `PlayerQuestion`
  - `RiskNote`
  - `ManualReviewNote`
  - `KnownIssueNote`
- `ReleasePlaytestRecord`
  - level id
  - clear 여부
  - attempts
  - time
  - move pressure
  - input feel
  - target readability
  - state readability
  - feedback clarity
  - puzzle interest
  - repetition/boredom
  - item/obstacle fairness
  - recommended action
- `ReleaseVisualReviewRecord`
  - screen id
  - screenshot path
  - viewport/device
  - clipping/overlap
  - visual identity issue
  - touch target issue
- `ReleaseLevelCandidateRecord`
  - recipe id
  - seed
  - validation result
  - solver result
  - design notes
  - manual triage status
  - accepted/rejected/deferred reason

이번 pass에서 바로 확정하지 않는 것:

- remote content format
- cloud save schema
- store telemetry format
- liveops/event data model

## Scene And UI Plan

UI/비주얼 V2의 목표:

- 화면을 보자마자 `DreamLaundromat`이 꿈 세탁소 퍼즐이라는 신호가 보여야 한다.
- card, order sheet, storage shelf, machine action, tool/obstacle이 서로 다른 물체처럼
  느껴져야 한다.
- text-only explanation이 아니라 icon, frame, badge, motion, feedback으로 규칙을 읽게
  해야 한다.

Gameplay 화면 보강 방향:

- Dream card는 “상태가 붙은 꿈 조각”처럼 보이게 한다.
- Order card는 “손님 요청서”처럼 보이게 한다.
- Storage는 빈칸 관리가 아니라 “보관 선반”처럼 보이게 한다.
- Operation은 button grid가 아니라 “꿈 처리 기계 조작부”처럼 느껴지게 한다.
- Tool과 Obstacle은 같은 strip에 있어도 시각적 역할이 분명히 달라야 한다.
- Result는 footer message가 아니라 clear/fail 순간으로 보여야 한다.

Direct manipulation 방향:

- tap flow는 유지한다.
- dream -> order, dream -> storage, storage -> active slot drag를 지원한다.
- operation은 drag 대상이 아니라 selected dream에 적용하는 action으로 둔다.
- invalid drop은 move를 쓰지 않고 이유를 보여준다.
- drag threshold와 feedback은 manual gate로 확인한다.

## Milestones

### M1 - Baseline And Plan Lock

목표:

- 현재 자동 검증 상태와 Android screenshot baseline을 고정한다.
- 이번 계획의 범위와 수동 gate를 명확히 한다.

산출물:

- `ALPHA_READINESS_PLAN.md`
- 최신 validation 결과 요약
- 대표 screenshot baseline
- 현재 manual gap 목록

Exit Criteria:

- 다음 변경의 품질을 비교할 기준이 있다.
- 자동 검증과 사람이 판단할 항목이 분리되어 있다.

### M2 - Visual/UI Polish V2

목표:

- “동작하는 UI”가 아니라 “출시 후보 게임 화면”으로 보이게 한다.

산출물:

- card/object visual treatment 개선
- title/home, level select, gameplay, pause/result screen polish
- icon/frame/background asset usage audit
- multi-resolution screenshot review
- visual review checklist update

Exit Criteria:

- Android screenshot만 봐도 꿈, 주문, storage, operation, tool, obstacle, result 역할이
  구분된다.
- 주요 화면에서 text clipping과 UI overlap이 없다.
- UI가 퍼즐 판독성을 밀어내지 않는다.

### M3 - Game Feel And Direct Manipulation V2

목표:

- 카드와 target을 직접 조작하는 모바일 퍼즐 감각을 강화한다.

산출물:

- tap target highlight 보강
- drag/drop V1 안정화
- invalid action feedback 보강
- action success/fail feedback 보강
- haptic/audio hook manual check note

Exit Criteria:

- 최소 대표 레벨을 tap/drag 혼합으로 플레이할 수 있다.
- 잘못된 조작이 move를 쓰지 않고 납득 가능한 feedback으로 끝난다.
- Game Feel 변경 후 기존 rules/solver/test가 깨지지 않는다.

### M4 - Controller And UI Structure Hardening

목표:

- UI/feedback/level review 작업이 계속 들어와도 `ReleaseGameController`가 과도하게
  커지지 않게 한다.

산출물:

- renderer/presenter/helper 책임 재정리
- action availability와 selection state 테스트 보강
- PlayMode UI regression test 보강

Exit Criteria:

- 다음 visual/game-feel pass가 controller 하나에 계속 코드를 누적하지 않는다.
- UI state 계산과 rendering 책임을 테스트 가능한 단위로 추적할 수 있다.

### M5 - 30 Level Manual Playtest And Triage

목표:

- 현재 30레벨이 내부 Alpha 후보로 충분한지 사람이 직접 판정한다.

산출물:

- 30레벨 playtest 기록
- 재미있었던 레벨/지루했던 레벨/top issue 요약
- `DesignNotes` triage 결과
- level/UX tuning backlog

Exit Criteria:

- 각 레벨이 `keep`, `tune UI`, `tune level`, `add tutorial`, `retest` 중 하나로 분류된다.
- UI 문제와 level design 문제가 섞이지 않고 분리되어 있다.

### M6 - Level/UX Tuning Pass

목표:

- 수동 playtest에서 나온 문제를 실제로 반영한다.

산출물:

- 수정된 level data 또는 level pack metadata
- tutorial/guidance 보강
- UX/feedback 보강
- validation report와 manual note 업데이트

Exit Criteria:

- 수정된 레벨은 solver/replay/release validation을 통과한다.
- 수정 이유가 playtest record와 연결되어 있다.
- 단순히 `DesignNotes` 수치만 줄이는 수정이 아니다.

### M7 - 80-120 Level Production Loop V1

목표:

- Beta/Soft Launch Candidate로 가기 위한 대량 레벨 생산 체계를 만든다.

산출물:

- candidate generation recipe set
- candidate report archive policy
- accepted/rejected/deferred triage format
- level acceptance criteria
- 80-120 level roadmap
- production batch command 또는 기존 `qa-balance.cmd` 확장 계획

Exit Criteria:

- 새 후보 level을 만들고, 검증하고, 사람이 선별해 fixed pack으로 승격하는 흐름이
  문서와 스크립트로 재현 가능하다.
- launch level count를 결정하기 전까지 필요한 생산 속도와 검증 비용을 추정할 수 있다.

### M8 - Alpha Readiness Verification Gate

목표:

- Alpha Build로 넘어갈지, 같은 단계에서 더 반복할지 판단한다.

산출물:

- 자동 검증 결과
- Android build/run/screenshot 결과
- manual gate summary
- known issues
- Go/Iterate/Pause 판정

Exit Criteria:

- 내부 플레이어에게 30레벨 빌드를 전달해도 기본 품질 문제가 즉시 드러나지 않는다.
- 남은 문제는 Alpha 이후 backlog인지, Alpha 진입 차단 문제인지 분류되어 있다.

## Task Breakdown

### AR-001 - Baseline Verification

- Outputs:
  - 최신 validation summary
  - screenshot baseline
  - current manual gap list
- Work:
  - `release-slice`, `qa-balance`, EditMode/PlayMode 상태를 확인한다.
  - 대표 level screenshot을 갱신한다.
  - `Warnings`, `DesignNotes`, accessibility 상태를 요약한다.
- Verification:
  - `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
  - `.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`
  - `.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900`
- Done criteria:
  - 이후 UI/level 변경 전후 비교 기준이 있다.

### AR-002 - Visual Issue Inventory

- Outputs:
  - visual issue list
  - screen-by-screen review note
- Work:
  - Title/Home, Level Select, Gameplay, Pause, Result screenshot을 검토한다.
  - “게임처럼 보이지 않는 이유”를 구체 항목으로 분해한다.
  - text-heavy, flat panel, object identity, result satisfaction, icon clarity 문제를 분류한다.
- Verification:
  - screenshot artifact 존재
  - `RELEASE_UI_DESIGN_PLAN.md`의 manual gate와 연결
- Done criteria:
  - UI/비주얼 수정 항목이 취향 표현이 아니라 화면별 문제로 정리되어 있다.

### AR-003 - Visual/UI Polish V2 Implementation

- Outputs:
  - 개선된 screen surfaces
  - card/object treatment updates
  - result/pause/level select polish
  - PlayMode UI tests
- Work:
  - Dream/Order/Storage/Operation/Tool/Obstacle의 시각 역할을 더 분명하게 만든다.
  - result clear/fail moment를 강화한다.
  - gameplay screen의 text 의존도를 추가로 줄인다.
  - 작은 화면에서 clipping을 줄인다.
- Verification:
  - PlayMode UI presence tests
  - screenshot smoke
  - level screenshot batch
  - `.meta` 누락 검사
- Done criteria:
  - 대표 Android screenshot에서 UI가 현재보다 명확히 게임 화면에 가까워진다.

### AR-004 - Controller Responsibility Audit

- Outputs:
  - `ReleaseGameController` responsibility map
  - split target list
- Work:
  - controller가 담당하는 screen flow, rendering, input, feedback, state 계산을 분류한다.
  - 즉시 분리할 것과 지금 유지할 것을 나눈다.
  - scene/YAML 변경이 커질 위험을 표시한다.
- Verification:
  - code reference 기반 audit
  - no behavior change
- Done criteria:
  - 구조 정리 작업이 “큰 리팩터링”으로 번지지 않게 경계가 있다.

### AR-005 - Controller/UI Structure Hardening

- Outputs:
  - renderer/helper/presenter 정리
  - focused EditMode/PlayMode tests
- Work:
  - UI 표시 descriptor와 rendering 책임을 더 작은 파일 단위로 정리한다.
  - selection/action availability 테스트를 보강한다.
  - feedback presenter와 visual update의 책임 경계를 확인한다.
- Verification:
  - EditMode tests
  - PlayMode tests
  - release-slice validation
- Done criteria:
  - UI/비주얼과 Game Feel 작업이 controller에 계속 누적되지 않는다.

### AR-006 - Game Feel V2 Implementation

- Outputs:
  - drag/tap feedback improvements
  - invalid action feedback
  - action success/fail feedback
  - optional haptic/audio check hooks
- Work:
  - compatible target halo를 강화한다.
  - drag/drop feedback과 invalid drop feedback을 정리한다.
  - operation preview와 action success/fail pulse를 보강한다.
  - reduced motion setting을 존중한다.
- Verification:
  - EditMode feedback mapping tests
  - PlayMode drag/tap dispatch tests
  - Android build/run smoke
- Done criteria:
  - 조작 결과가 더 즉각적이고 납득 가능하게 느껴진다.

### AR-007 - Manual Playtest Kit Update

- Outputs:
  - updated `RELEASE_MANUAL_PLAYTEST_CHECKLIST.md`
  - playtest result template
  - batch summary template
- Work:
  - UI/비주얼 V2와 Game Feel V2 이후 기준으로 평가 항목을 갱신한다.
  - level별 기록 외에 전체 session 피로도, 가장 좋은 레벨/나쁜 레벨, 즉시 수정 대상을
    기록하게 한다.
- Verification:
  - checklist가 `qa-balance` report 지표와 연결된다.
- Done criteria:
  - 사람이 30레벨을 플레이한 뒤 바로 tuning backlog로 옮길 수 있는 형식이다.

### AR-008 - 30 Level Manual Playtest

- Outputs:
  - 30 level playtest records
  - top issues summary
  - keep/tune/retest classification
- Work:
  - 사람이 Android 기기 또는 emulator에서 30레벨을 직접 플레이한다.
  - 각 레벨의 재미, 지루함, 조작감, 판독성, item/obstacle 공정성을 기록한다.
  - `DesignNotes`가 실제 문제인지 의도적 단순화인지 분류한다.
- Verification:
  - manual record completeness
  - 자동 검증과 manual note가 서로 모순되지 않는지 확인
- Done criteria:
  - level/UX tuning의 입력이 생긴다.
- Manual boundary:
  - 이 task는 Codex가 단독 완료할 수 없다. Codex는 기록 양식과 분석을 도울 수 있지만,
    실제 재미와 조작감 평가는 사람이 해야 한다.

### AR-009 - Playtest Triage

- Outputs:
  - tuning backlog
  - release blocker list
  - accepted intentional simplicity list
- Work:
  - 30레벨 기록을 `UI`, `game feel`, `level data`, `tutorial`, `visual`, `audio/haptic`
    문제로 나눈다.
  - 수정 우선순위를 정한다.
  - `DesignNotes` 중 release gate로 올릴 항목을 선별한다.
- Verification:
  - 각 tuning item이 playtest evidence와 연결된다.
- Done criteria:
  - 수정할 이유와 수정하지 않을 이유가 모두 기록되어 있다.

### AR-010 - Level/UX Tuning Implementation

- Outputs:
  - tuned levels
  - updated guidance/tutorial notes
  - updated QA report
- Work:
  - 지루하거나 불공정한 레벨을 수정한다.
  - 정보 부족으로 실수하게 만드는 UI/guidance를 보강한다.
  - item/obstacle이 정답 버튼처럼 느껴지는 레벨을 조정한다.
- Verification:
  - `release-slice.cmd`
  - `qa-balance.cmd`
  - changed level solver/replay validation
  - selected level screenshot review
- Done criteria:
  - 수정된 레벨은 자동 검증을 통과하고, 수동 기록의 문제를 직접 해결한다.

### AR-011 - Production Loop Design

- Outputs:
  - 80-120 level production loop spec
  - candidate acceptance rules
  - report/archive policy
- Work:
  - recipe 작성, candidate generation, validation, manual review, fixed pack 승격 흐름을
    정리한다.
  - accepted/rejected/deferred reason format을 정의한다.
  - level arc를 chapter/skill progression 단위로 나눈다.
- Verification:
  - `FULL_GAME_ROADMAP.md`의 Content Production Model과 일치
- Done criteria:
  - 대량 레벨 제작이 “그때그때 감으로 추가”가 아니라 반복 가능한 pipeline으로 정의된다.

### AR-012 - Production Loop Tooling V1

- Outputs:
  - candidate report improvements 또는 new wrapper
  - production batch summary
  - seed/recipe tracking
- Work:
  - 기존 generator와 `qa-balance` 결과를 production review에 쓰기 쉽게 정리한다.
  - candidate를 fixed pack으로 승격하기 전에 볼 metric과 manual note를 연결한다.
- Verification:
  - sample candidate batch runs
  - output report exists and is readable
- Done criteria:
  - 80-120레벨 확장을 시작하기 전에 후보 생산 비용과 검증 비용을 알 수 있다.

### AR-013 - Alpha Readiness Gate

- Outputs:
  - Alpha readiness summary
  - known issues
  - Go/Iterate/Pause decision
- Work:
  - 자동 검증과 manual 결과를 합쳐 Alpha Build 진입 여부를 판단한다.
  - 남은 문제를 Alpha blocker와 post-Alpha backlog로 나눈다.
- Verification:
  - 전체 Verification And Test Plan 실행
  - manual gate summary 작성
- Done criteria:
  - Phase 14 `Alpha Build`로 넘어가도 되는지 근거가 있다.

## PR Plan

사용자가 PR을 하나로 유지하고 싶다는 이전 방향을 반영해 기본값은 하나의 PR이다.

권장 branch:

```text
game/dream-laundromat-alpha-readiness
```

PR sub-gates:

1. `AR-001` - baseline and plan lock
2. `AR-002` to `AR-003` - UI/visual polish V2
3. `AR-004` to `AR-006` - controller hardening and game feel
4. `AR-007` to `AR-010` - manual playtest, triage, tuning
5. `AR-011` to `AR-012` - 80-120 level production loop
6. `AR-013` - full Alpha readiness gate

분리 PR을 고려해야 하는 신호:

- UI/비주얼 작업만으로 scene/UI diff가 너무 커진다.
- controller 구조 정리가 예상보다 큰 리팩터링이 된다.
- 30레벨 playtest 결과가 UI/level 대규모 재작업을 요구한다.
- production loop tooling이 release slice와 독립적으로 커진다.

Codex 금지:

- `gh pr merge`
- `git merge`
- protected branch push
- signing secret이나 store credential commit

## Verification And Test Plan

기본 자동 검증:

```powershell
git status --short --branch -uall
git diff --check
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900
.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900
```

추가 자동/반자동 검증:

- `.meta` 누락 검사
- changed text/YAML trailing whitespace 검사
- generated folder/build output/log exclusion 확인
- changed bitmap/audio/font asset LFS 대상 확인
- Android screenshot PNG non-empty 확인
- focused package/activity 확인
- logcat fatal/crash pattern 확인
- accessibility contrast/touch target audit
- release level pack `Warnings=0` 유지
- `DesignNotes` category summary 확인

Manual checks:

- 30레벨 전체 플레이
- 실제 Android 한손 조작감
- drag threshold와 invalid feedback 감각
- UI/비주얼 첫 인상
- 작은 화면/긴 화면 text clipping
- haptic/audio 감각
- 레벨 지루함과 반복감
- item/obstacle이 의미 있는 선택인지
- tutorial이 첫 플레이어에게 충분한지

Do not claim:

- 수동 playtest를 하지 않았는데 재미를 검증했다고 말하지 않는다.
- screenshot smoke만으로 visual taste가 좋다고 말하지 않는다.
- solver 통과만으로 level이 재미있다고 말하지 않는다.

## CLI And Manual Boundary

Codex가 CLI에서 할 수 있는 것:

- 문서 작성과 갱신
- code implementation
- UI renderer/helper refactor
- generated asset 생성과 import 설정 확인
- EditMode/PlayMode tests
- release validation
- QA/balance report
- Android build/install/launch smoke
- screenshot smoke
- level screenshot batch
- playtest record 분석
- PR 생성과 PR review

사람이 해야 하는 것:

- PR merge
- protected branch push
- 실제 30레벨 재미/피로도 판단
- 실제 기기 조작감 판단
- 최종 visual taste 판단
- 최종 haptic/audio 감각 판단
- 유료 asset 구매와 license 동의
- signing secret, keystore, Play Console 작업

## Gate Policy

### Go

다음 조건이면 Phase 14 `Alpha Build`로 넘어간다.

- automated validation이 통과한다.
- 30레벨 manual playtest에서 release blocker가 없다.
- UI/비주얼이 “임시 개발 UI”처럼 보이지 않는다.
- 조작감 문제는 minor backlog로 남길 수 있는 수준이다.
- production loop가 80-120레벨 확장을 시작할 만큼 명확하다.

### Iterate

다음 조건이면 이번 pass 안에서 더 수정한다.

- 퍼즐 재미는 보이지만 특정 레벨군이 지루하다.
- UI/비주얼이 일부 화면에서만 약하다.
- drag/tap 조작이 유용하지만 threshold나 feedback이 거슬린다.
- item/obstacle이 일부 레벨에서 정답 버튼처럼 느껴진다.
- tutorial/guidance 부족으로 초반 실수가 반복된다.

### Pause Or Replan

다음 조건이면 Phase 14로 가지 않고 로드맵을 재검토한다.

- core fun이 30레벨 playtest에서 확인되지 않는다.
- 4개 상태 축이 모바일에서 계속 읽히지 않는다.
- UI/비주얼 polish를 해도 게임 정체성이 전달되지 않는다.
- level production loop가 자동 검증과 수동 선별을 감당하지 못한다.
- 출시 준비가 gameplay 개선보다 우선순위를 빼앗기기 시작한다.

## Risks

### R1 - UI/비주얼 작업이 규칙 판독성을 해칠 수 있음

대응:

- decorative art보다 state/action readability를 우선한다.
- color-only 구분은 금지한다.
- PlayMode UI tests와 screenshot review를 같이 유지한다.

### R2 - 수동 playtest 전에 너무 많은 polish를 할 수 있음

대응:

- UI/비주얼 V2는 Alpha 판단에 필요한 수준으로 제한한다.
- final art, store screenshot, app icon은 제외한다.

### R3 - `ReleaseGameController` 정리가 과한 리팩터링이 될 수 있음

대응:

- 먼저 responsibility audit을 한다.
- pure calculation과 renderer helper부터 분리한다.
- scene hierarchy 전환이나 prefab migration은 이번 scope에서 제외한다.

### R4 - 30레벨 playtest가 주관적 감상으로만 남을 수 있음

대응:

- record format을 유지한다.
- 문제를 `UI`, `Game Feel`, `Level Data`, `Tutorial`, `Visual`, `Audio/Haptic`로
  분류한다.
- 수정은 playtest evidence와 연결한다.

### R5 - 80-120레벨 production loop가 실제 콘텐츠 제작으로 비대해질 수 있음

대응:

- 이번 pass는 pipeline과 sample batch까지만 목표로 한다.
- 실제 80-120레벨 완성은 Beta/Soft Launch Candidate phase로 둔다.

### R6 - Android automation이 환경 상태에 흔들릴 수 있음

대응:

- emulator/device 상태를 검증 결과에 명시한다.
- Android smoke 실패는 build 문제인지 device 연결 문제인지 분리한다.
- manual device checks는 자동 검증과 분리해서 기록한다.

### R7 - LFS/asset 용량 문제가 다시 생길 수 있음

대응:

- 새 bitmap/audio/font asset은 `.gitattributes` 대상인지 확인한다.
- generated asset은 source note와 `.meta`를 함께 유지한다.
- 외부 무료/유료 asset은 license 확인 전 repository에 넣지 않는다.

## Deferred Or Out Of Scope

game-local backlog:

- final commercial art pack
- final SFX pack and audio mix
- app icon, splash, store screenshots
- release AAB signing
- Google Play listing
- analytics/privacy/ads/IAP
- cloud save
- localization
- liveops/event content
- full hint system
- complete prefab/UI Toolkit migration

`docs/TODO.md` 대상은 아니다. 이번 항목들은 특정 게임의 Alpha/출시 준비 backlog이며,
공통 개발 환경, 저장소 정책, 공유 workflow deferred work가 아니다.

## Open Decisions

현재 구현을 막는 결정은 없다. 기본값으로 진행한다.

기본값:

- UI/비주얼 V2를 30레벨 playtest 전에 먼저 수행한다.
- Game Feel V2는 tap flow 유지 + 제한적 drag/drop으로 진행한다.
- controller 구조 정리는 최소 책임 분리로 제한한다.
- 30레벨 playtest는 사람이 수행하고, Codex는 기록/분석/수정 작업을 맡는다.
- 80-120레벨 production loop는 실제 대량 제작이 아니라 pipeline V1로 둔다.

나중에 결정하면 좋은 항목:

- drag를 launch 기본 조작으로 둘지, 접근성 옵션으로 끌 수 있게 할지
- 최종 art style을 더 painterly/card-like로 강화할지
- 무료/유료 asset을 실제로 도입할지
- haptic을 기본 활성화할지
- production loop에서 ScriptableObject/JSON export를 언제 도입할지
- launch level count를 100으로 할지 150 이상으로 할지

## First Implementation Step

첫 구현 단계는 `AR-001 - Baseline Verification`이다.

이유:

- UI/비주얼 V2를 시작하기 전에 현재 화면과 검증 결과를 고정해야 개선 여부를 판단할 수
  있다.
- 30레벨 playtest는 UI/비주얼과 Game Feel 보강 이후가 더 정확하다.
- production loop는 현재 30레벨의 재미와 병목을 확인한 뒤 설계하는 편이 낫다.

구체 첫 작업:

1. `release-slice`, `qa-balance`, EditMode/PlayMode 상태를 재확인한다.
2. 대표 level screenshot `0,4,9,14,29`를 갱신한다.
3. 현재 UI/비주얼 issue inventory를 작성한다.
4. 그 결과를 바탕으로 `AR-003 - Visual/UI Polish V2 Implementation`의 구체 수정 항목을
   확정한다.

## Current Execution Status

2026-06-18 기준 `AR-001`, `AR-002`, `AR-003`의 첫 pass, `Gameplay Layout V2`,
`Action Dock Readability` 추가 pass를 완료했다.

완료한 작업:

- `AR-001 - Baseline Verification`
  - `release-slice`, `qa-balance`, 대표 Android screenshot baseline을 확인했다.
- `AR-002 - Visual Issue Inventory`
  - `ALPHA_VISUAL_ISSUE_INVENTORY.md`를 추가하고 text-heavy section, empty state,
    modifier label, footer/status, header 밀도 문제를 분류했다.
- `AR-003 - Visual/UI Polish V2 Implementation`
  - gameplay section label을 `Dreams`, `Requests`, `Workbench`, `Shelf`,
    `Tools / Faults` 중심으로 정리했다.
  - storage가 없는 레벨에서 `No storage in this level.` 문장을 숨겼다.
  - 빈 storage slot의 `Empty` 문구를 제거하고, card slot label을 `D1`, `O1`, `S1`
    같은 1-based compact label로 바꿨다.
  - 기본 footer `Ready` 메시지를 숨기고, submit 가능 상태는 `Match`로 표현했다.
  - item/obstacle label을 `Tool`/`Fault`로 정리했다.
  - 관련 PlayMode/EditMode 테스트 기대값을 새 UI 계약에 맞췄다.
- `Gameplay Layout V2`
  - `GAMEPLAY_LAYOUT_V2_PLAN.md`를 추가했다.
  - footer navigation을 header compact controls로 옮겼다.
  - `Workbench`, 빈 `Shelf`, 비활성 `Store` 버튼을 조건부 노출로 바꿨다.
  - dream/order card row를 키우고 section title을 줄였다.
  - 화면 기본 상태를 `Dreams + Requests + Actions` 중심으로 재정렬했다.
- `Action Dock Readability`
  - `ACTION_DOCK_READABILITY_PLAN.md`를 추가했다.
  - `Submit Order`를 `Submit`으로 줄였다.
  - `Store 1`, `Recall 1`을 `Store S1`, `Recall D1` 형식으로 바꿨다.
  - modifier label은 내부 `DisplayName` 대신 `Swap`, `Refresh`, `Lock D1`,
    `Pin O2`, `Jam Wash`, `x1` 같은 release UI용 compact label을 사용한다.
  - operation button은 전체 단어 대신 `W`, `So`, `Cl`, `Se` marker와 icon을 함께
    쓰도록 바꿨다.
  - 관련 PlayMode/EditMode 테스트를 새 UI 계약에 맞췄다.

검증 결과:

```powershell
git diff --check
# Passed

.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
# Passed. Total=20 Passed=20 Failed=0 Skipped=0

.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
# Passed. Total=88 Passed=88 Failed=0 Skipped=0

.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
# Valid=True, Levels=30, Errors=0, Warnings=0, DesignNotes=58

.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
# Valid=True, AccessibilityValid=True, Levels=30, Warnings=0, DesignNotes=58

.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900
# Passed. Total=16 Accepted=12 Rejected=4

.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 1200
# Passed. Android debug APK build/install/launch succeeded.

.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
# Passed. Android screenshot smoke succeeded.

.\DreamLaundromat\level-screenshots.cmd -Build -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 1200 -BuildTimeoutSeconds 1200
# Passed. Action Dock Readability APK를 빌드/설치한 뒤 대표 screenshot set을 갱신했다.
```

생성/갱신된 대표 screenshot:

- `DreamLaundromat/Logs/level-screenshots/level-01.png`
- `DreamLaundromat/Logs/level-screenshots/level-05.png`
- `DreamLaundromat/Logs/level-screenshots/level-10.png`
- `DreamLaundromat/Logs/level-screenshots/level-15.png`
- `DreamLaundromat/Logs/level-screenshots/level-30.png`

주의:

- `DreamLaundromat/Logs/`는 repository에 commit하지 않는 generated output이다.
- 이번 pass는 긴 debug-like 문구와 불필요한 empty state를 줄인 뒤, 기본 화면의 정보량을
  `Dreams + Requests + Actions` 중심으로 다시 줄인 시각 개선이다.
- submit/action/modifier label의 큰 텍스트 의존도는 이번 branch 안에서 처리했다.
- 남은 핵심 gate는 사람이 실제로 30레벨을 플레이하며 재미, 조작감, 피로도,
  visual taste를 기록하는 것이다.
- `AR-004 - Controller Responsibility Audit`은 다음 대형 UI/feedback pass 전에 유용하지만,
  현재 Alpha 후보 자동 검증의 blocker는 아니다.

## Self-Review

검토 기준:

- `docs/IMPLEMENTATION_PLANNING.md`의 필수 섹션을 포함했다.
- 자동 검증과 manual gate를 분리했다.
- UI/비주얼 완성도를 Game Feel이나 구조 정리 안에 묻지 않고 독립 gate로 분리했다.
- 30레벨 수동 playtest를 Alpha 진입의 핵심 gate로 두었다.
- `DesignNotes`를 숫자 줄이기 대상이 아니라 manual triage queue로 두었다.
- 80-120레벨 작업은 실제 대량 제작이 아니라 production loop V1로 제한했다.
- PR은 하나로 유지하되 sub-gate를 나누는 방식을 명시했다.
- `docs/TODO.md` 대상과 game-local backlog를 구분했다.

자체 수정한 점:

- 처음에는 playtest를 가장 먼저 둘 수 있었지만, 현재 사용자가 UI/비주얼 완성도를 주요
  우려로 보고 있으므로 UI/비주얼 V2를 full 30레벨 playtest 전에 배치했다.
- controller 구조 정리를 독립 목표로 두되, UI/비주얼과 Game Feel을 막지 않도록 최소
  hardening으로 제한했다.
- 80-120레벨을 바로 만들겠다고 하지 않고, 후보 생산과 승격 기준을 먼저 만드는 것으로
  범위를 줄였다.

남은 약점:

- 실제 재미, visual taste, drag threshold, haptic/audio 감각은 Codex가 단독 완료할 수
  없다.
- Android screenshot smoke는 visual quality를 증명하지 않는다.
- Alpha readiness는 자동 검증만으로 판정할 수 없고, 30레벨 manual record가 필요하다.

이 약점은 task의 manual boundary와 gate policy에 반영했다.
