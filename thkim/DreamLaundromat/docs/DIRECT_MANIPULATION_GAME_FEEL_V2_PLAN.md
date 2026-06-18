# Direct Manipulation Game Feel V2 Plan

## Summary

이 문서는 `DreamLaundromat`의 현재 release slice를 “동작하는 UI”에서 “손으로
조작하는 모바일 퍼즐 게임”에 가까운 상태로 끌어올리기 위한 구현 플랜이다.

현재 게임은 30개 레벨, 진행 저장, item/obstacle, QA report, Android build/run,
screenshot smoke까지 갖추었고, 최근 검증에서 release gate의 unresolved warning은
`Warnings=0`으로 정리되었다. 대신 레벨 밸런싱 관찰값은 `DesignNotes=58`로
분리되어 있다.

이번 pass의 핵심은 세 가지다.

- `ReleaseGameController`에 몰린 책임을 줄여 direct manipulation을 넣을 수 있는
  구조를 만든다.
- 꿈 카드와 주문/보관/operation을 더 직접적으로 조작하게 만들어 game feel을 올린다.
- 이후 30개 레벨 수동 플레이테스트와 `DesignNotes` 기반 밸런싱을 할 수 있게
  screenshot/review 흐름을 정리한다.

## Planning References

- `DreamLaundromat/docs/FULL_GAME_ROADMAP.md`
- `DreamLaundromat/docs/PHASE_6_13_PLAN.md`
- `DreamLaundromat/docs/RELEASE_GAMEPLAY_SLICE_PLAN.md`
- `DreamLaundromat/docs/VISUAL_UX_DIRECTION_PLAN.md`
- `DreamLaundromat/docs/RELEASE_UI_DESIGN_PLAN.md`
- `docs/IMPLEMENTATION_PLANNING.md`
- `docs/UNITY_PROJECT_STRUCTURE.md`

## Prototype Goal

이번 구현의 검증 가설:

> 플레이어가 텍스트 설명을 읽기보다 카드를 고르고, 옮기고, 반응을 보면서
> “이 꿈을 어떤 주문에 맞출지”를 판단하는 감각을 얻으면, 현재 release slice의
> 퍼즐 재미를 더 정확히 평가할 수 있다.

성공 기준은 단순히 drag/drop이 구현되는 것이 아니다. 다음이 함께 확인되어야 한다.

- 선택한 꿈, 가능한 주문, 가능한 operation, 보관 후보가 즉시 읽힌다.
- 잘못된 조작은 이유와 함께 짧게 되돌아온다.
- 성공 조작은 카드 이동, pulse, haptic 후보 등으로 구분된다.
- 30개 레벨 수동 테스트에서 UI 조작감 문제와 레벨 설계 문제를 분리해 기록할 수
  있다.

## Scope

이번 plan에 포함한다.

- `ReleaseGameController` 책임 분리
- gameplay board/action rendering helper 분리
- 선택 상태와 action 가능 여부 계산을 pure/model 쪽으로 일부 이동
- tap 기반 direct manipulation 개선
- 제한적 drag gesture V1
- operation result preview와 invalid feedback 강화
- action/result feedback timing 정리
- level screenshot batch V1 설계 및 최소 구현 후보
- 30개 레벨 수동 플레이테스트 checklist와 기록 포맷
- `DesignNotes=58`을 레벨 밸런싱 review queue로 연결

## Non-Goals

이번 pass에서는 제외한다.

- 최종 art pack, 최종 animation timeline, particle polish
- 모든 UI를 prefab 또는 UI Toolkit으로 전환
- 전체 100개 이상 launch level 제작
- hint system, undo system, monetization, analytics
- Google Play store screenshot final
- release AAB/signing/keystore 작업
- 모든 `DesignNotes`를 0으로 만드는 강제 레벨 수정

`DesignNotes`는 전부 없애야 하는 오류가 아니다. 튜토리얼 또는 특정 mechanic 소개를
위해 의도적으로 단순한 레벨도 있다. 이번 pass에서는 이를 수동 평가 기준으로 삼고,
진짜 지루한 패턴만 후속 밸런싱에서 수정한다.

## Key Decisions

- 구현 순서는 `controller 분리 -> Game Feel V2 -> screenshot batch -> 30레벨
  수동 테스트 -> 레벨/UX 조정`으로 한다.
- `ReleaseGameController`는 당장 제거하지 않는다. scene/app flow shell로 남기고
  board rendering, action rendering, selection/action 계산을 작은 클래스로 옮긴다.
- 기본 입력은 tap을 유지한다. drag는 V1 범위에서 꿈 카드 이동과 보관/주문 drop에
  제한한다.
- operation은 drag 대상이 아니라 선택한 꿈에 적용하는 button action으로 유지한다.
  operation 결과 preview를 강화한다.
- screenshot smoke는 crash/focus/nonblank 검증이다. 재미와 손맛은 manual gate로
  남긴다.
- `order competition`, `first-solution` 관련 `DesignNotes`는 즉시 레벨 수정 대상이
  아니라 수동 플레이테스트의 핵심 질문으로 둔다.

## Target Platforms

Primary:

- Android
- Portrait
- One-hand touch
- Emulator smoke: `PocketDodger_API36`

Secondary:

- Unity Editor PlayMode
- Windows batchmode verification

필수 실행 경로:

```powershell
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
```

주의:

- Unity batchmode/test/build는 같은 프로젝트에서 병렬 실행하지 않는다.
- Android screenshot smoke는 자동으로 에뮬레이터를 시작할 수 있지만, 실제 손맛 평가는
  사람이 직접 해야 한다.

## Architecture

### Current Problem

`ReleaseGameController`가 현재 담당하는 책임:

- screen flow: home, level select, gameplay, pause, result
- session lifecycle
- gameplay UI 생성
- dream/order/storage/modifier/action rendering
- selection state
- input handler
- feedback pulse
- settings/pause/result 상태

이 상태에서 drag/drop, animation, screenshot automation, playtest instrumentation을 더
넣으면 controller가 더 커지고, PlayMode 테스트도 작은 단위로 실패 원인을 찾기
어려워진다.

### Target Shape

첫 분리 목표는 과한 추상화가 아니라 “Game Feel V2가 들어갈 자리”를 만드는 것이다.

권장 클래스:

- `ReleaseGameController`
  - scene-owned shell
  - screen 전환
  - `ReleaseGameSession` 소유
  - presenter/renderer 호출
- `ReleaseSelectionState`
  - selected dream/order/storage
  - selection clear/toggle 규칙
  - EditMode 테스트 가능
- `ReleaseActionAvailability`
  - 현재 state와 selection 기준으로 submit/store/recall/operation/item 가능 여부 계산
  - disabled reason 제공
- `ReleaseGameplayViewModel`
  - `DynamicRoundState`, `ReleaseLevelDefinition`, `ReleaseSelectionState`를 UI 표시용
    descriptor로 변환
- `ReleaseGameplayBoardRenderer`
  - dream/order/storage/preview/modifier card UI 생성과 refresh
  - Unity UI 의존 가능
- `ReleaseActionPanelRenderer`
  - operation, submit, store, recall, item button UI 생성과 refresh
- `ReleaseFeedbackPresenter`
  - pulse, invalid shake 후보, card movement animation hook
- `ReleaseLevelScreenshotBatch`
  - level별 screenshot automation을 위한 editor/runtime entry point 후보

처음부터 모든 renderer를 완전히 독립된 prefab 구조로 바꾸지 않는다. 현재
programmatic uGUI 구조를 유지하되, 계산과 표시 책임을 파일 단위로 나눈다.

## Data Model

### Selection

`ReleaseSelectionState`:

- `SelectedDreamSlotId`
- `SelectedOrderSlotId`
- `SelectedStorageSlotId`
- `HasDreamSelection`
- `HasOrderSelection`
- `HasStorageSelection`
- `ClearDream`, `ClearOrder`, `ClearStorage`, `ClearAll`
- `SelectDream`, `SelectOrder`, `SelectStorage`

규칙:

- 빈 dream/order/storage 선택은 상태를 바꾸지 않고 invalid feedback을 낸다.
- dream을 선택하면 operation/submit/store 후보가 갱신된다.
- storage를 선택하면 recall 후보가 갱신된다.
- clear/fail/restart/next level에서는 selection을 초기화한다.

### Action Availability

`ReleaseActionAvailability`가 계산할 항목:

- operation별 가능 여부와 after-state preview
- submit 가능 여부와 대상 order
- store 가능 여부와 대상 storage slot
- recall 가능 여부와 대상 active dream slot
- item 사용 가능 여부와 대상
- disabled reason
- highlighted compatible targets

이 계산은 가능하면 Unity UI 없이 EditMode 테스트 가능해야 한다.

### Feedback Event

`ReleaseFeedbackEvent` 후보:

- `SelectionChanged`
- `ActionSucceeded`
- `ActionFailed`
- `InvalidTarget`
- `ItemUsed`
- `ObstacleBlocked`
- `LevelCleared`
- `LevelFailed`

V2에서는 event hook과 간단한 visual response까지만 목표로 한다. 최종 SFX/haptic
mixing은 후속 pass로 둔다.

## Scene And UI Plan

### Tap Flow V2

기본 흐름:

1. dream card tap
2. compatible order/storage/operation이 강조됨
3. order tap 또는 submit button tap으로 제출
4. storage tap으로 보관
5. operation button tap으로 변환
6. 실패 시 짧은 invalid feedback과 disabled reason 표시

중요한 변화:

- `Pick D`, `Pick D + O` 같은 상태 문구에 의존하지 않고, 카드 강조와 action 상태로
  이해되게 한다.
- submit/store/recall은 가능한 대상이 하나뿐이면 한 번의 tap으로 처리할 수 있게
  한다.
- 대상이 여러 개이면 대상 highlight 후 한 번 더 tap하게 한다.

### Drag Flow V1

지원 후보:

- active dream card -> order card: `SubmitDream`
- active dream card -> storage slot: `StoreDream`
- storage card -> active dream empty slot: `RecallDream`

제외:

- operation으로 drag
- modifier를 card에 drag
- multi-touch gesture
- physics-like free movement

drag 기준:

- 일정 threshold 전까지는 tap으로 처리한다.
- drag 중 ghost card 또는 lifted state를 표시한다.
- drop 가능한 target은 halo로 표시한다.
- invalid drop은 원래 위치로 복귀하고 짧은 reason을 표시한다.

### Feedback

V2 최소 feedback:

- selected card halo
- compatible target halo
- operation after-state chip preview
- successful action card travel 또는 pulse
- invalid action shake 또는 red pulse
- clear/fail transition pulse
- optional haptic call hook

Animation은 rules state와 결합하지 않는다. `DynamicRulesEngine.Apply` 결과가 먼저
확정되고, UI가 그 결과를 표현한다.

## Milestones

### Milestone 1 - Baseline And Controller Split

목표:

- 현재 동작을 보존하면서 `ReleaseGameController`의 책임을 줄인다.

산출물:

- `ReleaseSelectionState`
- `ReleaseActionAvailability`
- `ReleaseGameplayViewModel`
- board/action renderer 후보 클래스
- 기존 PlayMode 테스트 통과

Exit Criteria:

- `ReleaseGameController`가 session/screen shell에 가까워진다.
- gameplay action 가능 여부는 별도 테스트 가능한 코드에서 계산된다.
- UI 동작은 이전과 같거나 더 명확하다.

### Milestone 2 - Direct Manipulation V2

목표:

- tap/drag 조작이 카드와 target 중심으로 느껴지게 한다.

산출물:

- tap target highlight
- drag dream to order/storage
- drag storage to active slot
- invalid drop feedback
- operation preview 강화

Exit Criteria:

- Android에서 최소 한 레벨을 tap/drag 혼합으로 클리어할 수 있다.
- invalid action이 왜 실패했는지 이해 가능하다.

### Milestone 3 - Feedback And Game Feel

목표:

- 반복 action이 덜 건조하게 느껴지게 한다.

산출물:

- action success/fail pulse
- card movement or lift feedback
- clear/fail transition feedback
- `ReleaseFeedbackPresenter` 테스트 가능한 event mapping

Exit Criteria:

- clear/fail/result 순간이 단순 텍스트 교체보다 명확하다.
- feedback이 solver/rules 결과와 어긋나지 않는다.

### Milestone 4 - Screenshot Batch V1

목표:

- 30개 레벨 화면 확인 비용을 줄인다.

산출물 후보:

- `scripts/run-level-screenshots.ps1`
- `level-screenshots.cmd`
- `Logs/level-screenshots/` 산출물
- level index, screenshot path, focused package, crash 여부 report

구현 옵션:

1. Android runtime automation:
   - 앱에 test launch mode 또는 PlayerPrefs 기반 level override를 둔다.
   - ADB로 level을 열고 screenshot을 캡처한다.
   - 실제 Android 화면에 가장 가깝다.
2. Unity PlayMode/editor capture:
   - PlayMode에서 level별 화면을 열고 screenshot을 남긴다.
   - 빠르지만 Android 실제 화면과 차이가 날 수 있다.

권장 기본값:

- V1은 Android runtime automation을 목표로 한다.
- 단, 구현 부담이 커지면 PlayMode/editor capture를 먼저 만들고 Android batch는 다음
  pass로 분리한다.

Exit Criteria:

- 최소 5개 대표 레벨 screenshot batch가 CLI에서 생성된다.
- 이후 30개 전체로 확장 가능한 구조다.

### Milestone 5 - 30 Level Manual Playtest Loop

목표:

- UI/game feel 문제와 레벨 설계 문제를 분리해서 기록한다.

산출물:

- 30레벨 수동 테스트 checklist
- `DesignNotes` review 기준
- 레벨별 조정 후보 목록

Exit Criteria:

- “이 레벨은 왜 재미없었는가”를 조작감, 판독성, order competition, first-solution,
  preview relevance, item/obstacle 의미 중 하나로 분류할 수 있다.

## Task Breakdown

### DGF-001 - Baseline Capture

- Outputs:
  - 최신 screenshot smoke artifact
  - 현재 QA report 요약
  - `DesignNotes` 종류별 집계
- Work:
  - 현재 `Warnings=0`, `DesignNotes=58` 상태를 baseline으로 기록한다.
  - 현재 Android screenshot을 수동 비교 기준으로 둔다.
- Verification:
  - `release-slice.cmd`
  - `qa-balance.cmd`
  - `screenshot-smoke.cmd`
- Done criteria:
  - 이후 변경이 좋아졌는지 비교할 기준이 있다.

### DGF-002 - Selection State Extraction

- Outputs:
  - `ReleaseSelectionState`
  - EditMode tests
- Work:
  - selected dream/order/storage 상태와 toggle/clear 규칙을 분리한다.
  - restart, next, level load 때 selection 초기화 규칙을 고정한다.
- Verification:
  - selection toggle/clear EditMode tests
  - 기존 PlayMode selection tests 통과
- Done criteria:
  - selection 계산이 `ReleaseGameController` 내부 필드에만 묶여 있지 않다.

### DGF-003 - Action Availability Extraction

- Outputs:
  - `ReleaseActionAvailability`
  - `ReleaseActionOption` 또는 유사 descriptor
  - EditMode tests
- Work:
  - operation/submit/store/recall/item 가능 여부를 pure 계산으로 분리한다.
  - disabled reason과 compatible target 정보를 제공한다.
- Verification:
  - operation preview, submit 가능 여부, invalid reason tests
- Done criteria:
  - UI renderer가 직접 rules 상태를 매번 해석하지 않는다.

### DGF-004 - Gameplay ViewModel And Renderer Split

- Outputs:
  - `ReleaseGameplayViewModel`
  - `ReleaseGameplayBoardRenderer`
  - `ReleaseActionPanelRenderer`
- Work:
  - card label, chip, halo, button state 생성을 controller 밖으로 옮긴다.
  - `ReleaseGameController`는 view model을 만들고 renderer에 넘기는 흐름으로 줄인다.
- Verification:
  - PlayMode UI presence tests
  - no regression in clear/fail/pause flow
- Done criteria:
  - direct manipulation 코드가 들어갈 renderer/input surface가 분리되어 있다.

### DGF-005 - Tap Target Highlight V2

- Outputs:
  - compatible target halo
  - selected target state
  - compact disabled reason
- Work:
  - dream 선택 시 compatible order/storage/operation을 강조한다.
  - storage 선택 시 recall 가능한 active slot을 강조한다.
  - invalid target tap은 짧은 feedback으로 처리한다.
- Verification:
  - PlayMode: selecting D0 shows compatible target state
  - PlayMode: invalid target does not spend move
- Done criteria:
  - 플레이어가 다음 가능한 행동을 텍스트 없이도 추론할 수 있다.

### DGF-006 - Drag Gesture V1

- Outputs:
  - drag source detection
  - drop target detection
  - drag ghost/lift state
  - invalid drop feedback
- Work:
  - dream -> order submit
  - dream -> storage store
  - storage -> active dream recall
  - operation은 기존 button flow 유지
- Verification:
  - PlayMode: drag dream to compatible order clears or submits
  - PlayMode: invalid drop keeps state and move count
  - Android screenshot/smoke after drag support
- Done criteria:
  - 최소 핵심 카드 이동이 direct manipulation으로 동작한다.

### DGF-007 - Feedback Presenter V2

- Outputs:
  - `ReleaseFeedbackPresenter`
  - feedback event mapping
  - pulse/shake/lift constants
- Work:
  - action success/fail/result feedback을 한 곳에서 관리한다.
  - haptic/audio hook은 optional로 유지한다.
- Verification:
  - EditMode: event type maps to expected timing profile
  - PlayMode: action succeeded/failed feedback object state 확인
- Done criteria:
  - feedback 변경이 rules/session code를 흔들지 않는다.

### DGF-008 - Level Screenshot Batch Plan And V1

- Outputs:
  - `level-screenshots.cmd` 또는 구현 후보 문서화
  - screenshot batch report
- Work:
  - 대표 레벨 5개를 batch로 열고 screenshot을 저장하는 최소 경로를 만든다.
  - 30개 전체 확장은 같은 구조로 가능해야 한다.
- Verification:
  - screenshot files exist and are non-empty
  - focused package check
  - crash logcat check
- Done criteria:
  - 수동 UI review가 한 화면씩 직접 찾아 들어가는 방식에만 의존하지 않는다.

### DGF-009 - 30 Level Manual Playtest Checklist

- Outputs:
  - playtest checklist section or separate note template
- Work:
  - 레벨별 질문을 정한다.
  - `DesignNotes`를 사람이 판단할 수 있는 기준으로 번역한다.
- Verification:
  - checklist가 QA report의 지표와 연결된다.
- Done criteria:
  - 수동 플레이 후 어떤 레벨을 왜 조정할지 기록 가능하다.

### DGF-010 - DesignNotes Triage After Playtest

- Outputs:
  - 조정 대상 레벨 목록
  - 유지 가능한 의도적 단순화 목록
  - validator threshold 조정 후보
- Work:
  - `No obvious order competition`이 튜토리얼 의도인지 지루함인지 분류한다.
  - `First solution has a mechanical operation-submit cadence`가 실제 플레이에서도
    단조로운지 확인한다.
  - `Preview is unlikely`가 정보 과잉/부족 문제인지 확인한다.
- Verification:
  - 변경된 레벨은 solver/replay/release validation 통과
  - manual note가 남아 있다.
- Done criteria:
  - 레벨 수정이 감으로만 이뤄지지 않는다.

## PR Plan

권장 방식:

- 이번 작업은 하나의 큰 PR 안에서 진행하되, PR 본문과 commit/log는 sub-gate로 나눈다.
- sub-gate 순서:
  1. controller split
  2. tap highlight/action availability
  3. drag V1
  4. feedback V2
  5. screenshot batch V1
  6. playtest checklist and DesignNotes triage

분리 PR이 필요한 신호:

- `ReleaseGameController` 분리만으로도 scene/PlayMode 테스트 변경이 크게 흔들린다.
- drag gesture 구현이 Android에서 불안정해서 controller split 검증을 막는다.
- screenshot batch가 별도 runtime automation mode를 많이 요구한다.

그 경우 우선순위는 `controller split`을 먼저 안정화하고, drag/screenshot batch를 뒤로
미룬다.

## Verification And Test Plan

기본 검증:

```powershell
git status --short --branch -uall
git diff --check
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
```

추가 자동 검증:

- `.meta` 누락 검사
- changed text/YAML trailing whitespace 검사
- Android screenshot PNG 유효성 검사
- PlayMode drag/tap action dispatch tests
- EditMode action availability tests
- `Warnings=0` 유지 확인
- `DesignNotes` count와 category report 확인

수동 검증:

- Android portrait에서 한손 조작감
- card drag threshold가 너무 민감하거나 둔하지 않은지
- 손가락이 주요 정보를 가리지 않는지
- invalid feedback이 불쾌하거나 과하지 않은지
- 첫 30레벨에서 지루한 반복 패턴이 어디서 나타나는지
- `order competition`이 부족한 레벨이 튜토리얼 의도인지 실제 문제인지
- `first-solution`이 너무 기계적인 레벨이 반복되는지

## Manual Playtest Checklist

30개 레벨 수동 테스트에서 각 레벨마다 기록할 항목:

- Level ID
- Clear 여부
- 실패했다면 실패 이유가 납득되는지
- 조작감 문제:
  - tap/drag 오입력
  - target 판독 어려움
  - feedback 부족
  - 손가락 가림
- 퍼즐 문제:
  - order competition 부족
  - first solution이 너무 기계적임
  - preview가 의미 없음
  - storage가 puzzle이 아니라 정리 노동처럼 느껴짐
  - item이 정답 버튼처럼 느껴짐
  - obstacle이 불공정하게 느껴짐
- 난이도:
  - 너무 쉬움
  - 적당함
  - 갑자기 어려움
  - move limit이 너무 빡빡함
  - move limit이 너무 느슨함
- 다음 조치:
  - 유지
  - UI/feedback 수정
  - level data 수정
  - tutorial 보강
  - 후속 검토

## DesignNotes Triage Policy

`DesignNotes`는 다음처럼 처리한다.

### Keep

유지 가능한 경우:

- 첫 소개 레벨이라 의도적으로 order competition을 줄였다.
- item/obstacle을 처음 배우는 레벨이라 active order를 하나로 제한했다.
- operation 하나를 익히는 레벨이라 first solution이 단순하다.
- 수동 플레이에서 재미를 해치지 않고 학습 목적이 명확하다.

### Adjust

수정해야 하는 경우:

- tutorial 이후에도 같은 단순 패턴이 반복된다.
- preview가 있어도 실제 결정에 영향을 주지 않는다.
- order가 하나뿐이라 assignment 판단이 거의 없다.
- operation-submit cadence가 여러 레벨 연속으로 반복된다.
- storage가 선택을 만들지 않고 move tax처럼만 작동한다.

### Reclassify

validator/리포트 기준을 바꿔야 하는 경우:

- 같은 warning이 모든 튜토리얼 레벨에서 당연히 발생해 noise가 된다.
- 반대로 현재 `DesignNotes`로 낮춘 메시지가 실제로는 release gate warning이어야 한다.
- metric은 좋지만 수동 플레이에서 지루한 패턴이 반복된다.

## CLI And Manual Boundary

Codex가 CLI에서 할 수 있는 것:

- controller split 구현
- pure state/action availability 테스트
- PlayMode tap/drag dispatch 테스트
- release validation
- QA balance report
- Android build/install/launch
- screenshot smoke
- screenshot batch V1
- DesignNotes category report

사람이 해야 하는 것:

- 실제 조작감 평가
- drag threshold 감각 평가
- visual taste 평가
- 30개 레벨 재미/피로도 판단
- PR merge
- store/signing/release secret 처리

## Risks

### R1 - Controller Split이 과한 리팩터링으로 커질 수 있음

대응:

- 순수 계산부터 분리한다.
- scene/UI hierarchy를 한 번에 갈아엎지 않는다.
- PlayMode regression을 자주 돌린다.

### R2 - Drag가 모바일에서 오입력을 늘릴 수 있음

대응:

- tap flow를 유지한다.
- drag threshold와 cancel feedback을 둔다.
- drag는 dream/order/storage에만 제한한다.

### R3 - Animation이 rules state와 어긋날 수 있음

대응:

- rules result가 먼저 확정되고 UI feedback이 따라오게 한다.
- animation 중 입력 lock 범위를 명확히 한다.
- solver/replay 결과와 runtime state가 다르지 않게 테스트한다.

### R4 - Screenshot batch가 구현 부담을 키울 수 있음

대응:

- 먼저 5개 대표 레벨만 대상으로 V1을 만든다.
- Android automation이 커지면 PlayMode/editor capture를 먼저 만든다.
- screenshot batch는 manual quality judgment가 아니라 review cost 절감 도구로 둔다.

### R5 - DesignNotes를 숫자 줄이기 게임으로 오해할 수 있음

대응:

- `Warnings=0`은 release gate이고, `DesignNotes`는 balance review queue다.
- 수동 플레이에서 재미를 해치지 않는 의도적 단순화는 유지한다.
- 조정 대상은 level intent와 manual note가 함께 있어야 한다.

## Deferred Or Out Of Scope

game-local backlog:

- undo system
- full hint system
- final SFX/haptic tuning
- full animation timeline
- prefab/UI Toolkit migration
- 100+ launch level production
- store screenshot final

repo-level `docs/TODO.md` 대상은 아니다. 이번 항목들은 특정 게임의 feature/backlog이며
공통 개발 환경이나 저장소 정책 문제가 아니다.

## First Implementation Step

가장 먼저 할 일:

1. `DGF-001`로 현재 `release-slice`, `qa-balance`, screenshot baseline을 확정한다.
2. `DGF-002`와 `DGF-003`을 구현해 selection/action availability를 controller 밖으로
   뺀다.
3. 기존 PlayMode 테스트가 그대로 통과하는지 확인한다.

이유:

- selection/action 가능 여부가 분리되어야 tap highlight와 drag drop을 안전하게 붙일
  수 있다.
- 이 분리 없이 direct manipulation을 넣으면 `ReleaseGameController`가 더 커지고,
  나중에 screenshot batch나 playtest instrumentation을 넣을 때 비용이 커진다.

## Open Decisions

현재 구현을 막는 결정은 없다. 기본값은 다음과 같다.

- drag는 dream/order/storage에만 적용한다.
- operation은 button tap과 preview chip으로 유지한다.
- screenshot batch V1은 Android runtime automation을 목표로 하되, 부담이 크면
  PlayMode/editor capture를 먼저 허용한다.
- 30레벨 수동 플레이테스트는 Game Feel V2 이후 진행한다.
- `DesignNotes`는 수동 테스트 전에는 레벨 수정 기준으로만 보관한다.

나중에 결정하면 좋은 항목:

- drag를 launch 기본 조작으로 둘지, accessibility option으로 끌 수 있게 할지
- haptic을 기본 켤지
- PlayMode screenshot batch와 Android screenshot batch 중 어느 것을 CI 후보로 둘지
- `DesignNotes` 중 어떤 category를 release gate warning으로 다시 올릴지

## Self-Review

검토 결과:

- 구현 순서는 `ReleaseGameController` 분리 후 direct manipulation으로 잡았다.
- screenshot batch는 game feel 변경 이후에 붙이는 것으로 배치했다.
- 30레벨 수동 플레이테스트는 UI/game feel 개선 후 수행하도록 했다.
- `order competition`, `first-solution`, `preview relevance`는 즉시 레벨 수정이 아니라
  playtest triage 기준으로 연결했다.
- 자동 검증과 수동 감각 평가를 분리했다.
- `docs/TODO.md` 대상이 아닌 game-local backlog로 분류했다.

남은 약점:

- 실제 drag 감각은 CLI로 판단할 수 없다.
- screenshot batch의 Android automation 방식은 구현 중 부담을 다시 평가해야 한다.
- `DesignNotes=58`은 숫자 자체보다 category별 반복과 수동 재미 평가가 중요하다.

이 약점들은 구현 차단 요소는 아니며, 각 milestone의 manual gate와 risk 대응으로
분리했다.

## Current Execution Batch

이번 실행 배치는 이미 완료된 `DGF-002`부터 `DGF-005`의 기반 위에서 남은 조작감
작업을 한 번에 이어서 진행한다. 목표는 release slice를 버튼 중심 퍼즐에서 카드 직접
조작이 가능한 모바일 퍼즐로 끌어올리고, 이후 30레벨 수동 평가를 시작할 수 있는
검증 흐름까지 마련하는 것이다.

### Execution Scope

포함한다:

- `DGF-006`: dream card를 order/storage로 drag하고, storage card를 빈 dream slot으로
  drag하는 V1 조작.
- `DGF-007`: 성공/실패/invalid 조작을 같은 방식으로 표현하는 feedback presenter V2.
- `DGF-008`: 대표 레벨 screenshot batch V1의 CLI 진입점.
- `DGF-009`: 30레벨 수동 playtest checklist와 기록 포맷.
- `DGF-010`: `DesignNotes`를 수동 평가 항목으로 연결하는 triage 기준 보강.

포함하지 않는다:

- 최종 drag animation timeline.
- undo/hint system.
- 최종 SFX/haptic mix.
- 모든 레벨의 수동 플레이 결과 작성. 이번 배치는 기록 틀과 자동 보조 도구까지만 만든다.

### Execution Order

1. Drag action resolver를 먼저 만든다.
   - `DynamicRoundState`, drag source, drop target을 받아 `DynamicPlayerAction`을 만든다.
   - dream -> order, dream -> storage, storage -> active dream만 허용한다.
   - 실패 이유는 UI message로 돌려준다.
2. UI drag handle/drop target을 붙인다.
   - tap flow는 유지한다.
   - drag 중 source card는 lift/alpha feedback만 준다.
   - invalid drop은 move를 쓰지 않고 실패 feedback으로 끝낸다.
3. Feedback presenter를 분리한다.
   - message pulse, invalid pulse, reduced motion 처리를 한 곳으로 모은다.
   - rules/session state를 바꾸는 코드는 feedback presenter에 넣지 않는다.
4. Screenshot batch V1을 만든다.
   - 우선 Android screenshot smoke를 확장하기 쉬운 CLI wrapper와 report 포맷을 둔다.
   - 구현 부담이 크면 현재 launch/screenshot smoke를 재사용한 대표 레벨 batch로 제한한다.
5. Manual playtest checklist를 문서화한다.
   - 조작감, target 이해도, 정보 가독성, level boredom, `DesignNotes` category를 분리한다.

### Verification Gates

자동 검증:

```powershell
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
```

추가 확인:

- Unity batchmode 명령은 병렬 실행하지 않는다.
- drag resolver는 EditMode test로 성공/실패 action을 검증한다.
- UI drag path는 PlayMode test에서 test helper로 action dispatch를 검증한다.
- Android screenshot은 non-empty PNG와 실제 렌더링 화면을 확인한다.

수동으로 남는 판단:

- drag threshold가 손에 맞는지.
- card lift/invalid feedback이 과하거나 약하지 않은지.
- 30레벨이 실제로 재미있고 지루하지 않은지.
- `DesignNotes` 중 의도적 튜토리얼 단순화와 실제 문제를 구분하는 판단.

### Batch Self-Review

검토 결과:

- 조작 rules는 UI보다 먼저 resolver로 분리해 테스트 가능하게 했다.
- drag를 모든 UI 요소에 확대하지 않고 dream/order/storage로 제한했다.
- screenshot batch는 최종 시각 품질 판단이 아니라 review cost 절감 도구로 제한했다.
- 30레벨 수동 테스트는 이번 배치에서 실제 결과를 채우지 않고, 평가 루틴과 기준을 만든다.
- 사용자 결정을 막는 항목은 없다. 기본값으로 진행한다.
