# Modifier Engine Implementation Plan

## Summary

이 계획은 `DreamLaundromat`의 `Dynamic Puzzle Lab`에 아이템과 방해요소를 수용할 수 있는
modifier engine을 추가하기 위한 구현 계획이다.

이번 pass의 목적은 많은 아이템을 넣는 것이 아니다. 핵심 목표는 `DynamicRulesEngine`에 예외
분기를 계속 추가하지 않고도, 아이템과 방해요소가 solver, generator, replay, debug play surface에
일관되게 들어갈 수 있는 구조를 만드는 것이다.

## Planning References

- [Dynamic Puzzle Lab Implementation Plan](DYNAMIC_PUZZLE_LAB_PLAN.md)
- [Dream Laundromat concept](../../../concepts/puzzle/dream-laundromat.md)
- [Core Rules](../../../concepts/puzzle/dream-laundromat-planning/03-core-rules.md)
- [Puzzle Grammar](../../../concepts/puzzle/dream-laundromat-planning/04-puzzle-grammar.md)
- [Content Production](../../../concepts/puzzle/dream-laundromat-planning/06-content-production.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)

## Prototype Goal

검증할 가설:

> 아이템과 방해요소를 `Modifier`라는 공통 구조로 표현하면, core puzzle rules를 훼손하지 않고도
> solver/generator가 modifier 포함 라운드의 clear 가능성, 선택지 가치, 방해 비용을 검증할 수 있다.

구체적으로는 다음을 확인한다.

- `UseItem`이 player action으로 solver와 debug UI에 자연스럽게 노출되는가
- obstacle이 기존 action의 가능 여부나 결과를 바꾸되, hidden random trap처럼 느껴지지 않는가
- modifier가 들어간 라운드도 같은 seed와 action list에서 항상 같은 결과를 내는가
- 아이템이 정답 버튼이 아니라 선택지를 늘리거나 비용을 바꾸는 도구로 작동하는가
- generator report가 modifier 사용 수, modifier 영향, reject reason을 기록할 수 있는가

## Scope

이번 계획에 포함한다.

- `DynamicModifierDefinition`
- `DynamicModifierState`
- `DynamicModifierType`: `Item`, `Obstacle`
- `DynamicModifierTrigger`: `Manual`, `CanApplyAction`, `BeforeAction`, `AfterAction`
- `DynamicModifierScope`: `Round`, `Dream`, `Order`, `Slot`, `Storage`, `Preview`
- `DynamicPlayerAction.UseItem`
- modifier hook pipeline
- solver action enumeration에서 item action 열거
- replay verifier의 modifier 포함 검증
- generator recipe의 allowed items / obstacles
- modifier metrics와 design validator 보강
- 실험용 item 1개: `Preview Swap`
- 실험용 obstacle 1개: `Locked Slot`
- focused Edit Mode tests
- debug play surface의 최소 modifier 표시와 item 사용 버튼

## Non-Goals

이번 계획에서 제외한다.

- 많은 아이템/방해요소의 production content 추가
- 과금형 booster, 소모품 inventory, 저장/계정 동기화
- real-time event나 hidden random trap
- analytics 기반 밸런싱
- 출시 UI polish
- full tutorial flow
- Addressables 기반 modifier asset pipeline
- 기존 prototype `stain/moisture` rules와 modifier engine 통합

## Key Decisions

- 아이템과 방해요소는 모두 `Modifier`로 표현한다.
- `Item`은 플레이어가 `UseItem` action으로 직접 사용한다.
- `Obstacle`은 별도 player action이 아니라 action 가능 여부나 action 결과를 바꾼다.
- modifier effect는 deterministic이어야 한다.
- 중간 random 발동은 첫 구현에서 금지한다.
- modifier는 `DynamicRoundDefinition`과 `DynamicRoundState`에 포함한다.
- `DynamicRulesEngine`에는 modifier별 `if`를 흩뿌리지 않고 hook pipeline을 둔다.
- 첫 hook은 `CanApplyAction`, `BeforeAction`, `AfterAction`, `EnumerateExtraActions` 네 종류만 둔다.
- 첫 item은 `Preview Swap`으로 한다.
- 첫 obstacle은 `Locked Slot`으로 한다.
- 첫 pass의 built-in 지원 scope는 `Preview`와 `Slot`으로 제한한다. 다른 scope enum은 이후 확장을
  위한 이름으로만 두고, 지원하지 않는 effect/scope 조합은 validator가 막는다.
- modifier가 solver state hash에 반드시 들어가야 한다.
- modifier 사용/발동은 metrics와 generator report에 기록한다.
- item이 필수인 라운드는 recipe에서 명시적으로 `RequiresItem = true` 같은 플래그를 둔다.

## Baseline And Branching

이 계획은 `Dynamic Puzzle Lab`이 기준선으로 존재한다는 전제 위에서 진행한다. modifier engine은
독립 기능이 아니라 `DynamicRoundDefinition`, `DynamicRoundState`, `DynamicRulesEngine`,
`DynamicActionEnumerator`, solver, generator에 걸쳐 들어가는 확장이기 때문이다.

진행 기준:

- `Dynamic Puzzle Lab` 변경이 `master`에 merge된 뒤 새 branch에서 시작하는 것을 기본으로 한다.
- 만약 바로 이어서 실험해야 한다면 현재 Dynamic Lab branch를 base로 한 stacked PR로 표시한다.
- Dynamic Lab 구현과 modifier engine 구현을 한 PR에 섞지 않는다. 단, 사용자가 명시적으로 한 PR을
  선택하면 PR 본문에 의존 범위와 review risk를 적는다.
- generated report, Unity `Logs/`, local smoke output은 계속 commit하지 않는다.

## Target Platforms

- Primary target: Unity Edit Mode / pure C# rules tests
- Runtime target: Android mobile 유지
- Orientation: Portrait 유지
- Input assumption: 한 손 tap 기반
- Build target: Android batchmode import/compile smoke
- Manual boundary:
  - modifier가 실제 화면에서 읽히는지
  - item 사용 버튼이 모바일에서 헷갈리지 않는지
  - obstacle이 불공정한 함정처럼 느껴지지 않는지

## Architecture

예상 폴더:

```text
Assets/_Project/Scripts/DynamicLab/
  Modifiers/
    DynamicModifierDefinition.cs
    DynamicModifierState.cs
    DynamicModifierEnums.cs
    DynamicModifierEffect.cs
    DynamicModifierContext.cs
    DynamicModifierPipeline.cs
    DynamicBuiltInModifiers.cs
```

주요 흐름:

```text
RoundDefinition
  -> modifier definitions
  -> RoundInitializer
  -> RoundState.Modifiers
  -> DynamicActionEnumerator
  -> ModifierPipeline.EnumerateExtraActions
  -> DynamicRulesEngine.Apply
     -> ModifierPipeline.CanApplyAction
     -> if UseItem: ModifierPipeline.ResolveManualAction
     -> else: ModifierPipeline.BeforeAction
              -> base action
              -> ModifierPipeline.AfterAction
  -> Solver / Replay / Metrics / Generator
```

`DynamicRulesEngine`은 계속 core action 처리를 담당한다. modifier는 core action 전후의 제약과
효과만 담당한다.

첫 built-in modifier에서 실제로 쓰는 경로는 `Preview Swap`의 `UseItem` manual path와
`Locked Slot`의 `CanApplyAction` path다. `BeforeAction`과 `AfterAction`은 interface에는 두되,
첫 pass에서는 no-op 기본 동작과 hook order test만 둔다.

## Data Model

### Modifier Definition

```text
DynamicModifierDefinition
  id
  displayName
  type: Item | Obstacle
  trigger
  scope
  charges
  effect
  targetRule
  tags
```

### Modifier State

```text
DynamicModifierState
  modifierId
  remainingCharges
  boundTargetKind
  boundTargetId
  isResolved
```

첫 구현에서는 `cooldown`, turn timer, real-time timer를 state에 넣지 않는다. 이 필드들은 live event,
booster, 시간 제한 modifier가 실제로 필요해졌을 때 별도 PR에서 추가한다. 지금은 `Preview Swap`과
`Locked Slot`을 검증하는 데 필요한 charge와 target binding만 둔다.

### Modifier Effect

첫 구현은 enum 기반 effect로 충분하다.

```text
DynamicModifierEffect
  PreviewSwap
  LockActiveDreamSlot
```

effect가 늘어나기 전까지는 복잡한 class hierarchy를 만들지 않는다. 다만 `DynamicModifierPipeline`
안에서 effect별 처리를 모아 `DynamicRulesEngine`의 분기를 오염시키지 않는다.

### Player Action

```text
DynamicPlayerAction
  ApplyOperation(activeDreamSlotId, operation)
  SubmitDream(activeDreamSlotId, activeOrderSlotId)
  StoreDream(activeDreamSlotId, storageSlotId)
  RecallDream(storageSlotId, activeDreamSlotId)
  UseItem(modifierId, optionalTargetId)
```

### Round Definition Extension

```text
DynamicRoundDefinition
  modifierDefinitions
```

### Round State Extension

```text
DynamicRoundState
  modifierStates
```

## Scene And UI Plan

첫 구현은 `DynamicLabDebugGame`에만 최소 UI를 연결한다.

화면 추가:

- active modifier list
- item 사용 버튼
- obstacle 설명 text
- locked slot 표시
- `Preview Swap` 사용 가능 횟수 표시

하지 않을 것:

- production HUD
- modifier icon art
- tutorial overlay
- inventory UI

## Milestones

### M1 - Modifier Core Model

modifier definition/state/enums를 추가하고 round definition/state에 연결한다.

Gate:

- modifier 없는 기존 round가 동일하게 초기화된다.
- modifier state가 clone/hash/replay에 들어간다.

### M2 - Action And Hook Pipeline

`UseItem` action과 modifier hook pipeline을 추가한다.

Gate:

- item action이 solver에서 열거된다.
- obstacle이 action 가능 여부를 막을 수 있다.
- base rules에 modifier별 분기가 흩어지지 않는다.

### M3 - First Item And Obstacle

`Preview Swap`과 `Locked Slot`을 구현한다.

Gate:

- `Preview Swap`은 dream preview 2개의 순서를 바꾼다.
- `Locked Slot`은 지정 active dream slot의 operation/submit/store를 막는다.
- 둘 다 deterministic replay가 가능하다.

### M4 - Solver, Generator, Metrics

solver, replay, generator, metrics, design validator가 modifier를 고려한다.

Gate:

- modifier 포함 sample round가 solver로 clear 가능하다.
- generator report가 modifier 사용과 obstacle 영향을 출력한다.
- item 필수 라운드와 item optional 라운드가 구분된다.

### M5 - Debug Play Surface

debug scene에서 modifier 상태를 보고 item을 직접 사용할 수 있게 한다.

Gate:

- `DynamicLabDebug.unity`에서 item 사용을 수동으로 확인할 수 있다.
- Play Mode smoke test가 item action을 통과한다.

## Task Breakdown

### DLMOD-001 - Modifier Enums And Definition

- Outputs:
  - `DynamicModifierEnums`
  - `DynamicModifierDefinition`
  - `DynamicModifierEffect`
- Work:
  - type, trigger, scope, effect enum을 정의한다.
  - definition은 immutable에 가깝게 운용한다.
- Verification:
  - definition 생성 tests
  - invalid modifier validation tests
- Done criteria:
  - modifier를 round definition에 넣을 수 있다.

### DLMOD-002 - Modifier Runtime State

- Outputs:
  - `DynamicModifierState`
  - round state modifier list
- Work:
  - remaining charges, target binding, resolved state를 표현한다.
  - cooldown과 timer는 첫 pass에서 제외하고, 필요해질 때 state versioning과 함께 추가한다.
  - `Clone`과 state hash에 포함한다.
- Verification:
  - clone isolation tests
  - state hash changes when modifier state changes
- Done criteria:
  - undo/replay가 modifier 상태를 잃지 않는다.

### DLMOD-003 - UseItem Action

- Outputs:
  - `DynamicActionType.UseItem`
  - `DynamicPlayerAction.UseItem`
- Work:
  - item id와 target slot/order/dream id를 표현한다.
  - action cost 정책을 정한다.
- Verification:
  - item action equality/replay smoke
  - invalid item action tests
- Done criteria:
  - solver와 UI가 item 사용을 action으로 다룰 수 있다.

기본 정책:

- `UseItem`은 1 move를 소비한다.
- 단, recipe나 modifier definition에서 `ConsumesMove = false`를 나중에 추가할 수 있게 열어둔다.

### DLMOD-004 - Modifier Pipeline

- Outputs:
  - `DynamicModifierContext`
  - `DynamicModifierPipeline`
- Work:
  - `CanApplyAction`
  - `BeforeAction`
  - `AfterAction`
  - `EnumerateExtraActions`
  - `ResolveManualAction`
  - 각 hook이 result/reason을 반환하게 한다.
- Verification:
  - hook order tests
  - blocked action does not spend move
  - manual item action can consume charge and spend move in the correct order
- Done criteria:
  - `DynamicRulesEngine`은 pipeline 호출만 하고 modifier별 세부 분기는 갖지 않는다.

### DLMOD-005 - Preview Swap Item

- Outputs:
  - built-in modifier definition
  - preview swap effect
- Work:
  - dream preview가 2개 이상일 때 앞 2개 순서를 바꾼다.
  - charge를 1 소비한다.
  - preview가 2개 미만이면 실패하고 move를 쓰지 않는다.
- Verification:
  - preview order swap tests
  - replay verifier test
  - solver can choose item action
- Done criteria:
  - item이 정답을 직접 만들지 않고 stream timing 선택지를 만든다.

### DLMOD-006 - Locked Slot Obstacle

- Outputs:
  - locked active dream slot effect
- Work:
  - 지정 active dream slot의 operation/submit/store를 막는다.
  - recall target으로도 잠긴 slot을 사용할 수 없게 한다.
  - locked 상태는 round 시작부터 visible하다.
- Verification:
  - locked slot blocks actions
  - blocked action does not spend move
  - solver avoids locked slot
- Done criteria:
  - obstacle이 공간 압박을 만들지만 hidden trap으로 동작하지 않는다.

### DLMOD-007 - Solver And Enumerator Integration

- Outputs:
  - item action enumeration
  - modifier-aware state hash
- Work:
  - `DynamicActionEnumerator`가 usable item을 열거한다.
  - `DynamicRoundStateHasher`가 modifier states를 포함한다.
  - solver limit이 modifier state space 증가를 견딜 수 있게 sample size를 제한한다.
- Verification:
  - known modifier round min move tests
  - unsolvable locked slot tests
  - solver limit tests 유지
- Done criteria:
  - modifier 포함 라운드가 solver/replay에서 안정적으로 검증된다.

### DLMOD-008 - Generator And Metrics Integration

- Outputs:
  - recipe allowed modifiers
  - modifier metrics
  - report fields
- Work:
  - `DynamicStageRecipe`에 allowed items/obstacles를 추가한다.
  - generator가 candidate에 modifier를 포함할 수 있게 한다.
  - metrics에 `ItemUseCount`, `ModifierTriggeredCount`, `ObstacleBlockedActionCount`,
    `MinMovesWithoutItems`, `MinMovesWithItems`를 추가한다.
  - `MinMovesWithoutItems`는 item-required 비교가 필요한 sample에서만 계산하고, solver cap을 넘으면
    `Unavailable`로 기록한다.
  - `ObstacleBlockedActionCount`는 solver가 실제로 실행한 막힌 action 수가 아니라 enumerator나 debug
    attempt에서 차단된 candidate action 수로 정의한다.
- Verification:
  - generator determinism tests
  - item optional vs required tests
  - report includes modifier effect
  - comparison solve가 solver cap을 넘을 때 report가 실패하지 않고 `Unavailable`을 기록하는지 확인
- Done criteria:
  - generator가 modifier 때문에 깨진 라운드를 reject할 수 있다.

### DLMOD-009 - Debug Play Surface

- Outputs:
  - `DynamicLabDebugGame` modifier panel
  - Play Mode smoke test
- Work:
  - modifier 목록과 charge를 표시한다.
  - `Preview Swap` 버튼을 노출한다.
  - locked slot을 UI text/color로 구분한다.
- Verification:
  - Play Mode test for item button path where feasible
  - manual debug scene check
- Done criteria:
  - 사람이 accepted modifier round를 직접 조작해 볼 수 있다.

### DLMOD-010 - Documentation And Samples

- Outputs:
  - sample modifier round definitions
  - plan progress update
- Work:
  - item/obstacle sample round를 각각 1개 이상 추가한다.
  - design intent와 risk note를 적는다.
- Verification:
  - sample rounds initialize
  - sample rounds solve/replay
- Done criteria:
  - 이후 modifier 추가자가 첫 예제를 보고 확장할 수 있다.

## PR Plan

추천 PR split:

### PR 1 - Modifier Core And Hooks

- `DLMOD-001`부터 `DLMOD-004`
- pure model, state, action, hook pipeline
- Edit Mode tests 중심

### PR 2 - First Item And Obstacle

- `DLMOD-005`부터 `DLMOD-007`
- `Preview Swap`, `Locked Slot`, solver/enumerator/hash integration
- known round tests와 replay tests

### PR 3 - Generator, Metrics, Debug Surface

- `DLMOD-008`부터 `DLMOD-010`
- generator recipe/report/metrics, debug UI, docs
- Play Mode smoke test와 CLI batch report

PR 하나로 합칠 수도 있지만 추천하지 않는다. modifier hook과 solver state space 변경은 회귀 위험이 있으므로 core hook과 content sample을 분리하는 편이 리뷰하기 쉽다.

## Verification And Test Plan

공통 검증:

- `git status --short --branch`
- `git diff --check`
- `.meta` 누락 확인
- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
- `.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900`
- Android batchmode import/compile:

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"
& $Unity -batchmode -quit -projectPath ".\DreamLaundromat" -buildTarget Android -logFile ".\DreamLaundromat\Logs\dreamlaundromat-android-batchmode.log"
```

Edit Mode tests:

- modifier definition validation
- modifier state clone/hash
- `UseItem` valid/invalid action
- blocked action does not spend move
- `Preview Swap` changes preview order
- `Locked Slot` blocks operation/submit/store/recall target
- solver can clear item sample round
- solver rejects impossible locked slot round
- replay verifier passes modifier solution
- generator deterministic with modifier recipe
- generator report includes modifier data

Play Mode tests:

- debug surface loads modifier round
- item action can be applied through test hook
- locked slot text/color appears where feasible

Manual checks:

- `DynamicLabDebug.unity`에서 modifier list가 읽히는지
- `Preview Swap`이 계획 선택처럼 느껴지는지
- `Locked Slot`이 불공정한 함정이 아니라 명확한 제약처럼 보이는지
- 모바일 세로 화면에서 modifier panel이 과밀하지 않은지

## CLI And Manual Boundary

CLI에서 가능한 것:

- pure model 구현
- Edit Mode tests
- Play Mode smoke tests
- solver/replay/generator batch 검증
- Android batchmode import/compile
- report 생성

수동 확인이 필요한 것:

- 실제 Game view에서 modifier UI 가독성
- item 사용이 재미있는 선택인지
- obstacle이 짜증나는 제약인지 의미 있는 제약인지
- Android 기기/에뮬레이터 터치 체감
- GitHub PR merge

## Risks

- hook을 너무 많이 만들면 core rules보다 framework가 커진다.
- modifier별 분기가 `DynamicRulesEngine`에 흩어지면 유지보수가 어려워진다.
- solver state space가 빠르게 커질 수 있다.
- item이 정답 버튼이 되면 puzzle decision이 약해진다.
- obstacle이 hidden trap처럼 느껴지면 공정성이 깨진다.
- modifier UI가 4축 상태 UI와 합쳐져 모바일 화면에서 과밀해질 수 있다.
- item 필수 라운드와 item optional 라운드를 구분하지 않으면 generator metric 해석이 어려워진다.
- debug surface를 production UI처럼 키우면 rules 검증보다 scene 작업이 커진다.

대응:

- first pass는 `Preview Swap`과 `Locked Slot`만 구현한다.
- hook은 네 종류로 제한한다.
- generator sample size와 solver limit을 작게 유지한다.
- design validator는 처음엔 reject보다 warning 중심으로 시작한다.
- item 필수 여부는 recipe에 명시한다.

## Deferred Or Out Of Scope

게임 로컬 후속 backlog:

- 소모품 inventory
- booster economy
- chapter별 modifier unlock
- tutorial overlay
- modifier icon art/audio/haptics
- live event modifier recipe
- analytics-based modifier tuning

공통 환경이나 repo 정책에 영향을 주는 항목은 아니므로 `docs/TODO.md`에는 기록하지 않는다.

## Implementation Defaults

사용자가 별도 변경을 요청하지 않으면 다음 기본값으로 진행한다. 현재 계획에는 구현 시작을 막는
미해결 사용자 결정은 없다.

1. PR split
   - 기본값: 3개 PR로 분리.
   - 영향: 리뷰가 쉬워지고 solver/hook 회귀를 좁게 확인할 수 있다.

2. `UseItem` move cost
   - 기본값: 첫 구현은 1 move 소비.
   - 영향: item이 공짜 정답 버튼이 되는 위험을 줄인다. 단, `Preview Swap`이 너무 약하면 이후 `ConsumesMove = false` 실험을 별도 recipe에서 한다.

3. 첫 item과 obstacle
   - 기본값: `Preview Swap`, `Locked Slot`.
   - 영향: 둘 다 deterministic이고 solver/generator 검증이 쉽다.

4. Item required round 허용 여부
   - 기본값: 첫 modifier content PR에서는 optional 중심, sample 1개만 `RequiresItem = true`로 명시.
   - 영향: item이 선택지인지 필수 열쇠인지 metric으로 비교할 수 있다.

## Self-Review

자체 점검 결과:

- Core Fun과의 연결: modifier는 상태 조율과 주문 배정을 돕거나 방해하는 비용 변화로 정의했다. 별도 미니게임이나 운 요소로 빠지지 않는다.
- Game Pillars와의 연결: deterministic action, preview fairness, solver-first 원칙을 유지한다.
- Core Rules: `UseItem`을 명시 action으로 넣고, obstacle은 hook으로 제한해 core action model이 유지된다.
- Puzzle Grammar: item/obstacle이 round grammar의 새 constraint와 option으로 들어간다.
- Content Production: recipe allowed modifiers, metrics, report를 포함해 generator 기반 생산 경로가 유지된다.
- UX: 첫 UI는 debug surface에만 묶어 production UI 리스크를 분리했다.
- Verification: pure tests, replay, solver, generator, debug Play Mode, Android batchmode를 포함했다.
- PR Boundaries: core hooks, first content, generator/debug UI를 분리했다.
- 범위 보정: cooldown/timer는 제외했고, metric 비교 solve는 solver cap 안에서만 수행하게 제한했다.
- 남은 불확실성: 실제 재미와 가독성은 debug scene과 Android 수동 확인으로 검증해야 한다.

## Implementation Progress

현재 branch에서 계획의 첫 구현 범위를 완료했다.

- `DLMOD-001`부터 `DLMOD-004`: modifier definition/state/enums, `UseItem`, hook pipeline을 추가했다.
- `DLMOD-005`부터 `DLMOD-007`: `Preview Swap`, `Locked Slot`, solver action enumeration, replay, state hash를 연동했다.
- `DLMOD-008`: recipe allowed modifiers, modifier metrics, report 출력을 추가했다.
- `DLMOD-009`: debug surface에 modifier 목록, item action, locked slot 표시를 추가했다.
- `DLMOD-010`: item/obstacle sample round와 modifier recipe, focused Edit Mode/Play Mode tests를 추가했다.

구현 중 조정한 점:

- `DreamPreviewCount > ActiveDreamSlots`는 `Preview Swap` 실험에 필요한 구성이므로 hard error가 아니라 warning으로 낮췄다.
- 첫 pass에서 `cooldown`, timer, real-time 발동은 구현하지 않았다.
- `MinMovesWithoutItems` 비교는 `CompareWithoutItems`가 켜진 recipe에서만 수행한다.

검증 상태:

- `git diff --check`: 통과
- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`: 41개 통과
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`: 7개 통과
- `.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900`: 통과
- Android batchmode import/compile: 통과
- `Assets/_Project` 아래 `.meta` 누락 확인: 누락 없음

## First Implementation Step

사용자가 이 계획을 승인하면 다음 순서로 시작한다.

1. `Dynamic Puzzle Lab` 기준선이 `master`에 merge되었는지 확인한다. 아직이면 stacked branch/PR로
   진행할지 먼저 표시한다.
2. `DLMOD-001`과 `DLMOD-002`로 modifier definition/state를 추가한다.
3. 기존 modifier 없는 sample rounds가 동일하게 solve/replay되는지 먼저 고정한다.
4. `UseItem` action과 hook pipeline을 추가한다.
5. `Preview Swap` item을 가장 먼저 붙여 solver와 replay를 검증한다.
