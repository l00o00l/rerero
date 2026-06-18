# Visual UX Direction Plan

## Summary

이 문서는 `DreamLaundromat`을 현재의 기능 중심 release slice에서 내부 테스트 가능한
Alpha 후보로 끌어올리기 위한 Visual/UX 구현 계획이다.

현재 Phase 6-13 작업으로 30개 레벨, item/obstacle, tutorial/save/settings,
QA/balance report, Android build/install/launch smoke 기반은 마련됐다. 하지만 화면은
아직 "동작하는 디버그풍 제품 UI"에 가깝고, 꿈 세탁소라는 컨셉을 플레이어가 직관적으로
읽게 만드는 visual identity, card language, feedback, level review UX가 부족하다.

이 계획의 목적은 다음 Phase 14 Alpha Build에 들어가기 전에 Phase 9-11을 출시 후보
수준으로 한 단계 보강하는 것이다. 동시에 Phase 6 item/obstacle이 실제로 의미 있는
선택을 만드는지 자동 평가를 강화해, 사람이 플레이하기 전에 명백히 약한 레벨을 걸러낸다.

## Planning References

- `DreamLaundromat/docs/FULL_GAME_ROADMAP.md`
- `DreamLaundromat/docs/PHASE_6_13_PLAN.md`
- `DreamLaundromat/docs/RELEASE_GAMEPLAY_SLICE_PLAN.md`
- `DreamLaundromat/docs/RELEASE_UI_DESIGN_PLAN.md`
- `DreamLaundromat/docs/MODIFIER_ENGINE_PLAN.md`
- `docs/IMPLEMENTATION_PLANNING.md`
- `concepts/puzzle/dream-laundromat.md`

## Prototype Goal

플레이어가 Android 세로 화면 스크린샷만 봐도 다음을 구분할 수 있어야 한다.

- 어떤 것이 꿈이고 어떤 것이 주문인지
- 꿈의 네 상태 축이 무엇을 의미하는지
- 어떤 action이 지금 가능하고, 누르면 어떻게 바뀌는지
- item과 obstacle이 어떤 제약/도구인지
- clear/fail/restart/next/settings가 어디에 있는지

성공 기준은 "예쁘다"가 아니라 "규칙 이해와 조작 판단이 빨라졌다"이다. 장식은 이 기준을
통과한 뒤에 추가한다.

## Scope

포함 범위:

- Dream card, Order card, Operation controls, Storage, Preview, Item/Obstacle UI의
  visual language 재정의
- 꿈 세탁소 컨셉에 맞는 기본 색상, icon/symbol, card frame, 상태 badge 설계
- `Wash`, `Soothe`, `Clarify`, `Settle`의 affordance와 feedback timing 정의
- action success/fail, item use, obstacle block, clear/fail moment의 juice V1
- Android emulator screenshot smoke를 정식 검증 플로우로 편입
- Phase 6 modifier impact 자동 평가 기준 추가
- 30개 level 전체의 visual/UX review checklist 작성

보강할 Phase:

- Phase 6: item/obstacle meaningfulness 자동 평가
- Phase 7: first 10 levels onboarding text와 guided prompt 표시 품질
- Phase 9: art direction V1을 실제 화면 언어로 확장
- Phase 10: audio/haptic/animation feedback timing 구체화
- Phase 11: mobile layout, text clipping, tap target, settings/pause/result polish
- Phase 13: screenshot review와 level review notes를 QA report에 연결

## Non-Goals

이번 pass에서 제외한다.

- Google Play store asset, app icon final, screenshots final
- release signing, AAB signing secret, Play Console 등록
- monetization, analytics, cloud save
- 고해상도 최종 일러스트 전체 제작
- 복잡한 컷신, narrative dialogue system
- 외부 유료 asset 구매
- Unity UI Toolkit 전면 전환
- 전체 scene prefab architecture 재작성

## Key Decisions

- 현재 `ReleaseGameplaySlice` scene과 programmatic UI를 유지하되, view helper와 style
  token을 정리한다.
- 외부 art asset은 당장 필수로 보지 않는다. 먼저 code-native shape, color, text,
  icon/symbol 조합으로 readable visual language를 만든다.
- 상태 축은 color-only로 표현하지 않는다. 최소한 label + color + symbol을 함께 쓴다.
- `Dream`은 "세탁물 + 기억 조각"처럼 보이게 하고, `Order`는 손님 요청서처럼 보이게
  한다.
- `Operation`은 기계 버튼이 아니라 "꿈 처리 기계"의 조작으로 보이게 한다.
- `Item`은 특수 도구, `Obstacle`은 고장/예약/제약으로 보이게 한다.
- Android emulator screenshot은 자동/반자동 검증에 포함한다. 실제 손맛과 재미는 manual
  gate로 남긴다.
- Phase 6 자동 평가는 solver metric과 counterfactual comparison으로 한다. 재미 판단을
  완전히 대체하지는 않는다.

## Target Platforms

Primary:

- Android
- Portrait
- One-hand touch
- 1080x1920 emulator baseline

Secondary checks:

- 720x1280 또는 작은 세로 화면
- 1440x2960 또는 긴 세로 화면
- Windows Editor PlayMode

Run scripts:

- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
- `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 240 -BuildTimeoutSeconds 1200`

## Architecture

기존 구조를 유지한다.

- `Thkim.DreamLaundromat.DynamicLab`
  - rules, solver, metrics, modifier impact audit
- `Thkim.DreamLaundromat.Gameplay.ReleaseSlice`
  - release level pack
  - game session
  - progress/settings/feedback
  - visual style
  - accessibility audit
  - balance/QA report
- `Thkim.DreamLaundromat.Editor.ReleaseSlice`
  - batch validation
  - QA/balance report writer
  - screenshot smoke helper 후보

UI 구현 방향:

- 단기적으로는 `ReleaseGameController`의 programmatic UI를 유지한다.
- visual token과 component creation helper를 더 분리한다.
- card/state rendering 함수는 의미별 helper로 분리해 테스트 가능하게 만든다.
- 장기적으로 prefab/serialized reference 기반 UI로 옮길 수 있도록 card model과 view
  mapping을 먼저 분리한다.

## Data Model

추가하거나 정리할 데이터:

- `ReleaseVisualStyle`
  - palette
  - typography scale
  - spacing
  - touch target
  - semantic colors
- `ReleaseStateVisualDescriptor`
  - `DreamTaint`, `DreamMood`, `DreamClarity`, `DreamStability`별 label/symbol/color
- `ReleaseActionVisualDescriptor`
  - operation별 icon/symbol, preview text, success/fail text
- `ReleaseModifierImpactReport`
  - item use count
  - obstacle blocked action count
  - min move delta with/without modifier
  - first solution includes modifier-relevant action 여부
  - warning flags
- `ReleaseScreenshotSmokeReport`
  - device id
  - resolution
  - focused package/activity
  - screenshot path
  - basic image sanity result

## Scene And UI Plan

화면 정보 구조:

1. Header
   - level id, name, difficulty
   - concise guidance
   - current guided prompt
2. Board
   - Active Dreams
   - Active Orders
   - Preview
   - Storage
   - Tools And Obstacles
3. Action Area
   - operation row
   - submit/store row
   - recall row
   - settings row
4. Footer
   - last action result
   - restart / next

개선 방향:

- Header는 최대 3줄 guidance를 안전하게 표시한다.
- Dream card는 네 상태 축을 한 줄 텍스트만이 아니라 badge cluster로 표시한다.
- Order card는 요구 조건을 "필수 badge"로 표시하고 fulfilled count를 시각화한다.
- Operation button은 선택된 꿈 기준 결과 preview를 compact하게 보여준다.
- 불가능한 action은 disabled만 하지 않고 reason을 메시지로 남긴다.
- Item/Obstacle은 같은 영역에 있더라도 item과 obstacle의 시각 계층을 분리한다.
- Clear/Fail은 footer message만으로 끝내지 않고 결과 state를 화면 구조에서 명확히 보인다.

## Milestones

### M1 - Visual Language Lock

- 상태 축별 symbol/label/color 확정
- Dream/Order/Operation/Item/Obstacle 시각 규칙 확정
- Android screenshot baseline 저장

### M2 - Card And Control Redesign

- Dream card badge cluster 구현
- Order requirement badge 구현
- Operation preview/disabled reason 개선
- Item/Obstacle affordance 개선

### M3 - Feedback And Result UX

- action success/fail feedback timing
- item use/block feedback
- clear/fail result state
- restart/next/settings 흐름 정리

### M4 - Screenshot And Accessibility Automation

- Android screenshot smoke wrapper 추가
- screenshot artifact path 정리
- resolution/focus/process/logcat check 포함
- accessibility audit와 QA report 연결

### M5 - Phase 6 Modifier Impact Audit

- modifier별 자동 impact metric 추가
- 30 level pack에서 modifier가 의미 없는 레벨 경고
- QA/balance report에 modifier impact summary 추가

### M6 - Full Visual UX Verification

- EditMode/PlayMode/release-slice/qa-balance/Android build/run/screenshot smoke
- manual gate checklist 업데이트

## Task Breakdown

### VUX-001 - Baseline Screenshot Audit

- Outputs:
  - `DreamLaundromat/Logs/android-launch-smoke.png`
  - screenshot review notes in plan or QA report
- Work:
  - current Android emulator screen capture
  - clipped text, hierarchy, action visibility, spacing issue 기록
- Verification:
  - emulator launch succeeds
  - screenshot is non-empty and focused on `com.rerero.dreamlaundromat`
- Done criteria:
  - 현재 UI의 가장 큰 visual/UX 문제가 문서화된다.

### VUX-002 - State Visual Descriptor

- Outputs:
  - state axis descriptor model
  - EditMode tests
- Work:
  - `Clean/Nightmare`, `Calm/Anxious`, `Blurry/Vivid`, `Stable/Unsettled`의
    label/symbol/color를 정의
  - color-only 구분을 피한다.
- Verification:
  - descriptor completeness test
  - accessibility contrast audit
- Done criteria:
  - 모든 dream state가 text + symbol + color로 표현 가능하다.

### VUX-003 - Dream And Order Card Redesign

- Outputs:
  - dream card renderer
  - order card renderer
- Work:
  - dream card badge cluster
  - order requirement badge cluster
  - selected/disabled/locked 상태 표시
- Verification:
  - PlayMode UI text/symbol presence
  - Android screenshot review
- Done criteria:
  - screenshot에서 dream과 order 역할이 즉시 구분된다.

### VUX-004 - Operation And Modifier Controls

- Outputs:
  - operation visual descriptor
  - item/obstacle visual descriptor
- Work:
  - operation result preview 강화
  - disabled reason message 정리
  - item과 obstacle 시각 구분
  - `DreamRefresh`, `OrderPin`, `OperationSoftBlock` 표시 보강
- Verification:
  - PlayMode: item/obstacle levels load and show expected UI
  - EditMode: descriptor completeness
- Done criteria:
  - item/obstacle이 도구인지 제약인지 화면만 보고 구분 가능하다.

### VUX-005 - Result, Pause, Settings UX

- Outputs:
  - clear/fail result state
  - pause/settings surface V1
- Work:
  - clear/fail 이후 next/restart 우선순위 정리
  - settings toggle이 gameplay action과 섞여 보이지 않게 정리
  - reduced motion/high contrast/large text 표시
- Verification:
  - PlayMode: clear/fail/result controls
  - manual: one-hand reachability
- Done criteria:
  - player가 라운드 후 다음 행동을 망설이지 않는다.

### VUX-006 - Feedback Timing Table

- Outputs:
  - feedback timing constants
  - generated SFX/haptic mapping 정리
- Work:
  - operation, submit, item use, obstacle block, clear, fail feedback 구분
  - solver/rules와 animation timing 결합 금지
- Verification:
  - PlayMode smoke
  - Android run smoke
  - manual audio/haptic check
- Done criteria:
  - action 종류별 feedback hook이 명확히 분리된다.

### VUX-007 - Android Screenshot Smoke Script

- Outputs:
  - screenshot smoke wrapper
  - screenshot report file
- Work:
  - `adb devices`, `pidof`, focused activity, logcat fatal check
  - `screencap` pull
  - screenshot path 출력
- Verification:
  - emulator에서 screenshot artifact 생성
  - zero-byte/invalid image 실패 처리
- Done criteria:
  - UI 변경 PR마다 Android 화면 smoke를 CLI에서 재현할 수 있다.

### VUX-008 - Phase 6 Modifier Impact Audit

- Outputs:
  - modifier impact report
  - QA/balance report section
- Work:
  - modifier가 있는 레벨과 없는 레벨을 분리 집계
  - first solution item use 여부
  - obstacle blocked action count
  - modifier 제거/무효화 후 solver 결과와 비교
  - minMoves delta, branching delta, warning flags 작성
- Verification:
  - EditMode tests with fixture levels
  - `qa-balance.cmd` report includes modifier impact summary
- Done criteria:
  - Phase 6 item/obstacle이 자동 report에서 의미 있음/약함/위험으로 분류된다.

### VUX-009 - 30 Level Visual Review Checklist

- Outputs:
  - level visual/UX checklist section
- Work:
  - 각 level의 screenshot/manual review 항목 정의
  - onboarding, modifier, storage pressure, preview relevance 관점 포함
- Verification:
  - QA report에 checklist link 또는 summary 포함
- Done criteria:
  - 사람이 플레이할 때 무엇을 봐야 하는지 명확하다.

### VUX-010 - Full Verification Gate

- Outputs:
  - verification summary
- Work:
  - 필수 검증 명령 순차 실행
  - Android screenshot artifact 확인
  - manual gate 업데이트
- Verification:
  - 아래 Verification And Test Plan 전체 실행
- Done criteria:
  - Phase 14 Alpha Build로 넘어갈 수 있는 Visual/UX 기준이 확보된다.

## PR Plan

사용자가 현재 작업을 하나의 PR로 유지하고 싶다고 했으므로, 구현 PR은 기본적으로 하나로
묶는다. 다만 내부 commit/task 단위는 위 task id로 나눈다.

권장 PR 제목:

- `DreamLaundromat Visual UX Direction Pass`

PR에는 다음을 포함한다.

- Visual/UX plan
- UI/card/control redesign
- modifier impact audit
- screenshot smoke tooling
- EditMode/PlayMode/release-slice/qa-balance/Android build/run 검증 결과
- 수동 gate

## Verification And Test Plan

필수 자동 검증:

```powershell
git status --short --branch -uall
git diff --check
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900
.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 240 -BuildTimeoutSeconds 1200
.\DreamLaundromat\screenshot-smoke.cmd
```

추가 자동/반자동 검증:

- Android screenshot smoke
- focused package/activity check
- app process check
- logcat fatal/crash pattern check
- screenshot non-empty check
- accessibility audit
- modifier impact audit

수동 검증:

- 30개 레벨을 실제로 플레이하며 재미, 피로도, 반복감 기록
- 첫 10레벨 onboarding 이해도 확인
- item/obstacle이 억지로 느껴지는지 확인
- 실제 기기 haptic/audio 감각 확인
- 작은 화면/긴 화면에서 텍스트 겹침 확인

## CLI And Manual Boundary

CLI로 가능한 것:

- solver/replay validation
- level pack validation
- modifier impact metric
- UI text/symbol existence test
- accessibility contrast/touch target audit
- Android build/install/launch
- emulator screenshot capture
- focused activity, process, logcat smoke

CLI로 일부만 가능한 것:

- screenshot 기반 판독성 검토
- action feedback timing의 존재 확인
- level difficulty metric

사람이 봐야 하는 것:

- 실제 재미
- 조작감
- 반복 피로도
- 시각적 매력
- SFX/haptic 감각 품질
- 튜토리얼 문구가 처음 보는 사람에게 충분한지

## Phase 6 Automation Answer

Phase 6의 "item/obstacle이 실제로 재미 있는 선택을 만드는가"는 상당 부분 자동으로 볼 수
있다. 다만 자동 검증은 "좋은 재미"를 증명하지 못하고, 명백히 약한 레벨을 걸러내는 데
강하다.

자동으로 가능한 평가:

- modifier가 포함된 레벨 수와 분포
- solver first solution이 item을 실제로 사용하는지
- obstacle이 blocked candidate를 실제로 만드는지
- modifier 제거 시 minMoves, branching, solvability가 바뀌는지
- item을 쓰지 않는 해법이 더 짧거나 같은지
- obstacle이 단순 move tax처럼만 작동하는지
- modifier가 특정 레벨에만 몰려 있는지
- `OrderPin`, `DreamRefresh`, `OperationSoftBlock` 각각의 coverage와 warning

자동으로 어려운 평가:

- item을 썼을 때 플레이어가 영리하다고 느끼는지
- obstacle이 답답함이 아니라 계획 압박으로 느껴지는지
- 같은 패턴 반복으로 지루해지는지
- 실제 손가락 조작 중 실수가 늘어나는지

권장 판단:

- Phase 6은 `VUX-008 - Phase 6 Modifier Impact Audit`으로 먼저 자동화한다.
- 자동 report에서 약한 레벨을 걸러낸 뒤, 남은 후보를 사람이 플레이한다.
- 이 방식이면 사람이 모든 것을 맨눈으로 찾는 것보다 훨씬 효율적이다.

## Risks

### R1 - Visual polish가 rules clarity를 해칠 수 있음

대응:

- 장식보다 state readability를 우선한다.
- color-only 표현을 금지하고 text/symbol을 같이 쓴다.

### R2 - Programmatic UI가 커져 유지보수가 어려워질 수 있음

대응:

- 이번 pass에서는 helper와 descriptor로 정리한다.
- prefab 전환은 별도 PR/phase로 판단한다.

### R3 - Screenshot smoke가 시각 품질을 과대평가할 수 있음

대응:

- screenshot은 nonblank, clipping, hierarchy 확인용으로 둔다.
- 재미와 감각은 manual gate로 남긴다.

### R4 - Modifier impact metric이 좋은 퍼즐을 잘못 탈락시킬 수 있음

대응:

- 자동 metric은 hard fail보다 warning 중심으로 시작한다.
- 사람이 승인한 예외는 risk note로 남긴다.

### R5 - Audio/haptic이 플랫폼별로 다르게 느껴질 수 있음

대응:

- emulator는 launch smoke까지만 신뢰한다.
- 실제 기기 haptic/audio는 manual gate로 유지한다.

## Deferred Or Out Of Scope

game-local backlog:

- prefab 기반 UI architecture 전환
- 실제 일러스트 asset 제작/수급
- 최종 SFX pack 제작
- level select map
- challenge/event visual variants

repo-level TODO로 올리지 않는다. 현재 항목들은 특정 게임의 Visual/UX 품질 보강이므로
`docs/TODO.md` 대상이 아니다.

## Implementation Status

완료:

- Phase 6 item/obstacle 자동 영향 평가를 추가했다.
- Phase 7 guided prompt가 Android screenshot에서 잘리는 문제를 보강했다.
- Phase 9 state marker와 operation affordance V1을 추가했다.
- Dream/Order/Storage 카드를 `T:`, `M:` 같은 축약 표기에서
  `Taint: Clean`, `Mood: Calm`처럼 읽히는 card badge 언어로 보강했다.
- `Tool Item`과 `Visible Obstacle` label을 분리해 item과 obstacle의 역할 구분을
  강화했다.
- settings toggle을 gameplay action 영역에서 별도 settings strip으로 분리했다.
- clear/fail/playing 상태 메시지를 `Round Status` / `Round Result` 형태로 정리했다.
- feedback timing table을 코드 상수와 QA/balance report 섹션으로 추가했다.
- Phase 11 텍스트 줄 높이 보정과 PlayMode UI presence test를 보강했다.
- Phase 12 Android screenshot smoke wrapper를 추가했다.
- Phase 13 QA/balance report에 modifier impact와 visual/UX checklist를 추가했다.
- Dream/Order/Storage 카드, operation, submit/store, footer/navigation 버튼에 전용
  code-generated surface PNG를 적용했다.
- `ReleaseUiArtCatalog`와 `ReleaseUiArtGenerator`가 card/action/navigation surface를
  안정적으로 참조하고, 기존 PNG를 보존하도록 정리했다.
- Android level screenshot batch wrapper를 추가하고 대표 레벨 screenshot으로 visual
  regression을 확인했다.

남은 자동화 후보:

- pause overlay의 PlayMode 검증 강화.
- screenshot smoke report를 PR summary에 자동 첨부하는 wrapper.
- 여러 viewport 또는 font scale을 대상으로 한 screenshot batch 확장.

남은 manual gate:

- 30개 level 실제 플레이 재미, 피로도, 반복감.
- 실제 기기 haptic/audio 감각.
- 작은 화면과 긴 화면에서 touch comfort와 visual taste.

## First Implementation Step

첫 Visual/UX 구현 단계는 완료됐다. 다음 추천 순서는 아래와 같다.

1. `ReleaseGameController` 안에 남아 있는 card/action rendering helper를 더 작은
   presenter/helper로 분리할지 판단한다.
2. pause overlay와 large text/reduced motion 표시를 실제 UI 표면으로 확장한다.
3. level별 screenshot batch를 여러 대표 레벨과 viewport/font scale로 확장한다.

이유:

- modifier impact audit과 screenshot smoke가 생겼기 때문에 이후 UI 변경은 더 쉽게 검증할 수
  있다.
- 현재 card label은 좋아졌지만 아직 `ReleaseGameController` 안에 rendering 책임이 몰려 있다.
- pause, large text, reduced motion은 설정 데이터가 있지만 UI 표면이 아직 얕다.
- level별 screenshot은 생겼으므로, 다음에는 coverage를 넓혀 visual regression과 manual
  review 비용을 더 줄인다.

## Open Decisions

현재 구현을 막는 사용자 결정은 없다.

기본값:

- 외부 asset 없이 code-native visual language로 시작한다.
- Phase 6 자동 평가는 warning 중심으로 시작한다.
- Android emulator screenshot은 PR 검증에 포함한다.

나중에 결정할 항목:

- 무료 asset을 실제로 도입할지
- generated bitmap asset을 만들지
- programmatic UI를 prefab UI로 전환할지
- 최종 art style을 flat/vector-like로 갈지, painterly/card-like로 갈지

## Self-Review

검토 기준:

- `docs/IMPLEMENTATION_PLANNING.md` 필수 섹션을 포함했다.
- `FULL_GAME_ROADMAP.md` Phase 9-11과 Phase 14 진입 조건을 연결했다.
- Phase 6 추가 작업을 Visual/UX와 분리하지 않고 QA/balance 관점으로 연결했다.
- 자동 검증과 manual gate를 분리했다.
- `docs/TODO.md`에 넣을 공통 인프라 항목과 game-local backlog를 구분했다.

수정한 점:

- 처음에는 Visual/UX만 계획할 수 있었지만, 사용자 질문이 Phase 6 자동 평가까지 포함하므로
  `VUX-008`과 `Phase 6 Automation Answer`를 추가했다.
- "예쁘게 만들기"가 아니라 "규칙 이해와 조작 판단을 빠르게 만들기"로 성공 기준을
  좁혔다.
- Android screenshot smoke가 가능해진 현재 상태를 검증 계획에 반영했다.

남은 위험:

- 실제 visual taste와 재미는 자동 검증으로 증명할 수 없다.
- 현재 UI 구조가 커지고 있어, Visual/UX pass 이후 prefab 전환 여부를 다시 판단해야 한다.
