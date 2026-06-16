# Dynamic Puzzle Lab Implementation Plan

## Summary

`Dynamic Puzzle Lab`은 `DreamLaundromat`을 정적 프로토타입에서 출시 후보 구조로
발전시키기 위한 순수 규칙, seeded stream, solver, validator, generator 실험
환경이다.

이번 계획의 목적은 바로 완성형 UI나 출시용 레벨을 만드는 것이 아니다. 먼저
`상태조율 퍼즐`이 반복 가능한 재미를 만들 수 있는지 검증할 수 있는 실험실을
만든다.

핵심 방향은 아래와 같다.

```text
상태변화 = 게임의 정체성
주문 배정 = 퍼즐 판단의 중심
공간 압박 = 상태조율을 어렵게 만드는 비용
seeded stream = 매번 같은 풀이로 굳지 않게 만드는 입력 변화
solver/validator = 풀 수 있고 지루하지 않은 라운드를 걸러내는 안전망
```

## Planning References

- [DreamLaundromat prototype plan](PLAN.md)
- [Dream Laundromat concept](../../../concepts/puzzle/dream-laundromat.md)
- [Core Fun](../../../concepts/puzzle/dream-laundromat-planning/01-core-fun.md)
- [Core Rules](../../../concepts/puzzle/dream-laundromat-planning/03-core-rules.md)
- [Puzzle Grammar](../../../concepts/puzzle/dream-laundromat-planning/04-puzzle-grammar.md)
- [Content Production](../../../concepts/puzzle/dream-laundromat-planning/06-content-production.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)

## Prototype Goal

검증할 핵심 가설:

> `DreamLaundromat`은 고정 레벨 절차 퍼즐이 아니라, 계속 들어오는 꿈과 주문을
> 보고 꿈의 상태를 가장 쓸모 있는 형태로 조율하는 seeded stream 기반 퍼즐로
> 발전할 수 있다.

구체적으로는 아래를 확인한다.

- 플레이어가 매 턴 꿈의 가치를 다시 판단해야 하는가
- 항상 같은 처리 순서로 굳지 않는가
- `Wash`, `Soothe`, `Clarify`, `Settle`이 각각 퍼즐 딜레마를 만드는가
- 랜덤이 결과 운빨이 아니라 입력 다양성으로 작동하는가
- solver가 라운드의 클리어 가능성과 최소 턴을 검증할 수 있는가
- design validator가 풀리지만 지루한 라운드를 걸러낼 수 있는가
- generator가 후보 라운드를 만들고 검증 루프로 반복 개선할 수 있는가

## Core Fun Thesis

`DreamLaundromat`의 출시형 재미는 꿈을 무조건 좋은 상태로 만드는 데 있지 않다.
재미는 제한된 처리 공간 안에서 꿈의 `taint`, `mood`, `clarity`,
`stability`를 손님 주문에 맞게 조율하고, 어떤 꿈을 어떤 주문에 쓸지 판단하는
데서 나온다.

플레이어가 자주 해야 하는 판단:

- 이 꿈을 지금 처리할지, 다음 주문을 위해 남길지
- 이 꿈을 어떤 손님에게 배정하는 것이 가장 이득인지
- `Clarify`를 먼저 써서 정보를 드러낼지, `Soothe`로 위험을 낮출지
- `Settle`로 마무리해도 되는지, 아직 변환 여지를 남겨야 하는지
- 공간을 비우기 위해 낮은 가치 주문을 먼저 처리할지
- 다음 꿈/주문 preview를 보고 현재 선택을 바꿔야 하는지

피해야 할 상태:

- 꿈 상태마다 정답 액션이 고정되어 처리 작업처럼 느껴짐
- 공간 부족만이 난이도의 대부분을 차지함
- 기계 결과가 랜덤이라 계획이 무효화됨
- solver는 풀 수 있다고 판단하지만 실제 플레이는 반복적이고 지루함

## Design Pillars

### 1. Deterministic Actions, Variable Inputs

행동 결과는 예측 가능해야 하고, 새로움은 꿈과 주문의 입력 흐름에서 나와야 한다.

영향:

- operation은 랜덤 성공/실패를 쓰지 않는다.
- solver와 replay가 가능하다.
- 플레이어는 실패를 운보다 판단 문제로 받아들인다.

### 2. State Assignment Before Space Management

주요 고민은 "어디에 둘까"가 아니라 "이 꿈을 어떤 상태로 만들어 누구에게 줄까"여야
한다.

영향:

- design validator는 `storage-only difficulty`를 경고한다.
- 주문 경쟁, 상태 변환, preview relevance를 난이도 핵심 지표로 둔다.
- 공간 압박은 상태조율을 보조하는 비용으로만 사용한다.

### 3. Preview Makes Randomness Fair

랜덤 입력은 preview 없이는 공정하게 느껴지기 어렵다.

영향:

- 다음 꿈과 다음 주문을 일부 보여준다.
- preview count는 generator와 UI 양쪽에서 조정 가능한 parameter로 둔다.
- preview가 판단에 영향을 주지 않는 라운드는 design validator가 경고한다.

### 4. Solver First, Generator Later

콘텐츠를 많이 만들기 전에 풀 수 있는지, 지루하지 않은지 판단하는 안전망이 먼저다.

영향:

- handwritten sample rounds를 먼저 만든다.
- solver와 hard validator를 generator보다 먼저 구현한다.
- generator는 accepted/rejected reason을 남기는 후보 제안기로 시작한다.

### 5. Small Rules, Many Situations

규칙 수를 늘려 깊이를 만드는 대신, 적은 규칙이 stream과 주문 조합 속에서 많은
상황을 만들게 한다.

영향:

- 첫 lab에서는 4개 상태 축과 4개 operation만 사용한다.
- hidden reveal, chain reaction, special machine은 후속 실험으로 남긴다.
- stage recipe는 한 번에 새 개념을 많이 섞지 않는다.

## Scope

이번 `Dynamic Puzzle Lab`에 포함한다.

- 기존 정적 prototype rules와 분리 가능한 순수 C# lab model
- 4개 핵심 상태 축:
  - `taint`: `Clean`, `Nightmare`
  - `mood`: `Anxious`, `Calm`
  - `clarity`: `Blurry`, `Vivid`
  - `stability`: `Unsettled`, `Stable`
- 4개 핵심 operation:
  - `Wash`
  - `Soothe`
  - `Clarify`
  - `Settle`
- `DreamBag`과 `OrderDeck` 기반 seeded random input
- active dream slots, active order slots, preview slots
- deterministic round simulation
- player action model과 undo 가능한 state snapshot
- hard validator
- breadth-first 또는 Dijkstra 계열 solver
- difficulty/fun metric 계산
- design validator
- generator 후보 생성과 batch simulation
- focused Edit Mode tests
- CLI 또는 Unity Edit Mode에서 반복 실행 가능한 lab test entry

## Non-Goals

이번 계획에서 제외한다.

- 출시용 UI polish
- 새 아트/사운드/햅틱
- Android 빌드 품질 개선
- liveops, monetization, analytics
- 자동 레벨 에디터 UI
- 완성형 힌트 시스템
- 물리 기반 이동
- 실시간 타이머
- 랜덤 성공/실패 기계
- 무한한 감정 종류
- 기존 playable scene의 전면 개편

단, 순수 model이 안정된 뒤 최소한의 debug UI 연결은 후속 milestone으로 포함할
수 있다.

## Key Decisions

- 랜덤은 결과가 아니라 입력에만 적용한다.
- 같은 `seed`와 같은 action sequence는 항상 같은 결과를 만든다.
- 기계/operation 결과는 deterministic하다.
- `DreamBag`과 `OrderDeck`은 완전 무작위가 아니라 stage parameter로 제한된
  bag/deck을 shuffle한다.
- `RoundDefinition`에 들어가는 `DreamBag`과 `OrderDeck`은 count-only로 확정된
  draw pool이다. weight는 `StageRecipe`에서만 사용한다.
- 다음 꿈 2개와 다음 주문 1개 preview를 기본값으로 한다.
- 첫 구현의 `Soothe`는 `Anxious -> Calm`만 처리하고 clarity side effect는 넣지
  않는다.
- 첫 구현의 `Clarify`는 `Blurry -> Vivid`만 처리하고 hidden reveal은 넣지 않는다.
- `Submit`은 기본적으로 `stability=Stable`인 꿈만 허용한다. 반전 주문은
  `taint`, `mood`, `clarity`에서 만들고, `Unsettled` 제출은 첫 구현에서 제외한다.
- solver는 최초 구현에서 optimal fun을 판단하지 않고 clear 가능성과 최소 턴을
  확인한다.
- design validator는 solver 결과 위에 얹어 지루한 패턴을 걸러낸다.
- generator는 solver/validator 이후에 만든다. generator를 먼저 만들지 않는다.
- lab model은 `MonoBehaviour`나 scene object에 의존하지 않는다.
- 권장 PR split은 `pure model`, `solver/validation`, `generator/batch`, 선택적
  `debug play surface`로 고정한다.
- 기존 `stain/moisture` prototype과 새 `taint/mood/clarity/stability` lab은
  한 번에 강제 통합하지 않는다. adapter 또는 별도 namespace로 실험 안정성을
  확보한다.

## Target Platforms

- Primary target: Unity Edit Mode / pure C# rules tests
- Runtime target: Android mobile을 계속 염두에 둔다.
- Orientation: Portrait 유지
- Input assumption: 한 손 tap 기반 선택
- Build target: 이번 계획의 초기 milestone에서는 Android build가 필수가 아니다.
- Manual boundary:
  - solver/validator가 계산한 난이도와 실제 재미의 일치 여부
  - stream preview가 모바일 화면에서 읽히는지
  - 랜덤 흐름이 공정하게 느껴지는지
  - 반복 플레이 피로감

## Architecture

새 model은 기존 namespace 아래에 lab 전용 영역으로 둔다.

```text
Assets/_Project/Scripts/
  DynamicLab/
    Model/
    Rules/
    Generation/
    Solving/
    Validation/
    Metrics/
```

예상 namespace:

```text
Thkim.DreamLaundromat.DynamicLab
```

데이터 흐름:

```text
RoundDefinition
  -> RoundInitializer(seed)
  -> RoundState
  -> PlayerAction
  -> DynamicRulesEngine.Apply(action)
  -> RoundState
  -> Solver / Validator / Metrics
```

generator 흐름:

```text
StageRecipe
  -> Candidate RoundDefinition
  -> HardValidator
  -> Solver
  -> Metrics
  -> DesignValidator
  -> AcceptedRound or RejectedRound
```

Unity scene 연결은 lab의 마지막 단계에서만 다룬다.

```text
RoundState
  -> DebugPresenter
  -> Existing or new lab scene
  -> player action
  -> RoundState
```

## Data Model

### Dream Attributes

```text
DreamAttributes
  taint: Clean | Nightmare
  mood: Anxious | Calm
  clarity: Blurry | Vivid
  stability: Unsettled | Stable
```

설계 의도:

- `taint`는 정화 필요성과 반전 주문을 만든다.
- `mood`는 `Soothe` 판단을 만든다.
- `clarity`는 정보/목표 선명도 판단을 만든다.
- `stability`는 제출 가능성과 마무리 비용을 만든다.

### Operations

```text
Wash
  input: taint=Nightmare
  output: taint=Clean, stability=Unsettled
  cost: 1 action

Soothe
  input: mood=Anxious
  output: mood=Calm
  side effect: none in first implementation
  cost: 1 action

Clarify
  input: clarity=Blurry
  output: clarity=Vivid
  side effect: none in first implementation
  cost: 1 action

Settle
  input: stability=Unsettled
  output: stability=Stable
  cost: 1 action
```

주의:

- `Soothe`의 `Vivid -> Blurry` side effect와 `Clarify`의 hidden reveal은 후속
  design experiment flag로 남긴다.
- 첫 구현에서 side effect를 넣지 않는 이유는 solver state space와 UX 설명 부담을
  낮추기 위해서다.
- 첫 lab에서는 hidden random reveal보다 deterministic visible transformation을
  우선한다.

### Player Action Model

solver가 열거하는 action은 명시적으로 제한한다.

```text
DynamicPlayerAction
  ApplyOperation(activeDreamSlotId, operation)
  SubmitDream(activeDreamSlotId, activeOrderSlotId)
  StoreDream(activeDreamSlotId, storageSlotId)
  RecallDream(storageSlotId, activeDreamSlotId)
```

규칙:

- 모든 action cost는 1이다.
- `ApplyOperation`은 active dream slot에 있는 꿈에만 적용한다.
- `SubmitDream`은 active dream slot에 있는 꿈에만 적용한다.
- `SubmitDream`은 꿈이 `stability=Stable`이고 주문 요구 속성을 만족할 때만
  성공한다.
- `StoreDream`은 active dream을 비어 있는 storage slot으로 옮긴다.
- `RecallDream`은 storage dream을 비어 있는 active dream slot으로 옮긴다.
- `Undo`와 `Restart`는 player-facing command지만 solver action에는 포함하지
  않는다.

이 action model은 공간관리의 역할을 제한하기 위한 선택이다. storage는 처리 순서를
압박하는 보조 공간이고, operation과 submit은 active dream slot에서만 일어난다.

### Order Requirement

```text
OrderRequirement
  requiredTaint: optional
  requiredMood: optional
  requiredClarity: optional
  requiredStability: optional
  count
  priority
```

주문은 필요한 속성만 비교한다. 단, 첫 구현에서 `SubmitDream`은 system rule로
`stability=Stable`을 요구한다. `requiredStability=Unsettled` 주문은 첫 lab에서는
invalid order로 처리한다.

### Stream Data

```text
DreamBag
  entries:
    attributes
    count

OrderDeck
  entries:
    requirements
    count

StreamConfig
  activeDreamSlots
  activeOrderSlots
  dreamPreviewCount
  orderPreviewCount
  maxDreamDraws
  maxOrderDraws
```

`RoundDefinition`의 bag/deck은 count-only다. generator 입력인 `StageRecipe`는
weight를 사용할 수 있지만, candidate round를 만들 때 확정된 count 목록으로
materialize해야 한다.

### Storage Model

```text
StorageConfig
  storageSlotCount

StorageSlot
  slotId
  dreamId: optional
```

초기 lab storage 규칙:

- 모든 storage slot capacity는 1이다.
- `capacityCost`는 첫 lab에서 사용하지 않는다.
- storage dream은 operation이나 submit 대상이 아니다.
- storage dream을 처리하려면 `RecallDream`으로 active slot에 다시 올려야 한다.
- active dream slot이 비어 있으면 dream preview에서 즉시 refill한다.
- storage slot은 자동 refill되지 않는다.

이 모델은 공간 압박을 유지하되, 퍼즐 중심이 storage packing으로 이동하지 않게
하기 위한 최소 구조다.

### Round Definition

```text
RoundDefinition
  roundId
  seed
  moveLimit
  targetCompletedOrders
  streamConfig
  dreamBag
  orderDeck
  actionSet
  storageConfig
  tutorialTags
  difficultyTarget
```

### Round State

```text
RoundState
  activeDreams
  dreamPreview
  remainingDreamBag
  activeOrders
  orderPreview
  remainingOrderDeck
  storageSlots
  remainingMoves
  completedOrders
  randomCursor
  status
  failureReason
```

### Clear And Failure Conditions

```text
RoundStatus
  Ready
  Playing
  Cleared
  Failed

FailureReason
  None
  NoMovesRemaining
  NoValidActions
  NoDreamsAvailable
  ImpossibleOrderState
```

Clear:

- `completedOrders >= targetCompletedOrders`이면 즉시 `Cleared`다.
- clear는 move가 0이 된 직후에도 우선한다. 마지막 action으로 목표 주문 수를
  달성하면 실패가 아니다.

Failure:

- `remainingMoves == 0`이고 clear가 아니면 `NoMovesRemaining`이다.
- active dream, storage dream, remaining dream stream이 모두 비었고 clear가 아니면
  `NoDreamsAvailable`이다.
- valid solver action이 하나도 없고 clear가 아니면 `NoValidActions`다.
- hard validator나 solver가 남은 주문을 action set으로 절대 충족할 수 없다고
  판단하면 `ImpossibleOrderState`다.

runtime rules engine은 명백한 runtime failure만 처리하고, 깊은 미래 가능성 판단은
solver/validator가 맡는다.

## Randomness Model

### Principles

- 플레이어 행동의 결과는 예측 가능해야 한다.
- 랜덤은 꿈과 주문의 입력 순서를 바꿔 매 턴 새 판단을 만든다.
- preview는 랜덤의 불공정성을 줄이는 핵심 UX다.
- 라운드는 seed로 재현 가능해야 한다.
- 같은 stage recipe라도 여러 seed 후보를 돌려 좋은 라운드만 채택한다.

### Dream Bag

`DreamBag`은 특정 stage에서 등장 가능한 꿈 상태의 구성이다.

예:

```text
DLAB-012 DreamBag
  Clean + Anxious + Blurry + Stable: 3
  Nightmare + Calm + Blurry + Stable: 2
  Clean + Calm + Blurry + Unsettled: 2
  Nightmare + Anxious + Vivid + Stable: 1
```

검증 기준:

- 주문 요구를 충족할 수 있는 충분한 후보가 있어야 한다.
- 모든 꿈이 동일한 가치가 되면 안 된다.
- 너무 많은 상태 축이 동시에 섞이면 안 된다.

### Order Deck

`OrderDeck`은 목표 상태의 흐름을 만든다.

예:

```text
DLAB-012 OrderDeck
  Calm + Stable: 2
  Clean + Vivid + Stable: 2
  Anxious + Vivid + Stable: 1
  Nightmare + Blurry + Stable: 1
```

검증 기준:

- 모든 주문이 같은 최종 상태만 요구하면 안 된다.
- 일부 주문은 낮은 처리 비용의 즉시 제출 후보를 제공해야 한다.
- 일부 주문은 다음 주문 preview 때문에 보류 판단을 만들 수 있어야 한다.

### Preview

기본값:

```text
dreamPreviewCount = 2
orderPreviewCount = 1
```

preview는 UI 부담과 공정성 사이의 균형이다. preview가 없으면 운빨이 되고,
preview가 너무 많으면 계산량과 화면 부담이 커진다.

### Seed Policy

`seed`는 round replay, solver reproduction, bug report에 필수다.

```text
same roundId + same seed + same action list = same final state
```

## Solver Strategy

### First Solver

초기 solver는 완전한 AI가 아니라 deterministic state search다.

추천:

- BFS: action cost가 모두 1이면 최소 턴 계산이 단순하다.
- state hash: active dreams, orders, previews, bag/deck cursor, storage, remaining moves.
- action enumeration: `ApplyOperation`, `SubmitDream`, `StoreDream`, `RecallDream`만
  solver 후보로 만든다.
- pruning:
  - 남은 move가 현재 best solution보다 크거나 같으면 중단
  - 이미 방문한 state는 재방문하지 않음
  - 완료 불가능한 주문 상태는 조기 reject

### Solver Outputs

```text
SolveResult
  solvable
  minMoves
  solutionCountEstimate
  firstSolutionActions
  visitedStates
  deadEndCount
  maxBranchingFactor
  averageBranchingFactor
  failureReason
```

### Solver Limits

- 첫 버전에서는 너무 큰 stream을 풀지 않는다.
- solver timeout과 visited state limit을 둔다.
- generator는 solver limit을 초과하는 후보를 reject한다.

## Validation Strategy

### Hard Validator

라운드가 규칙적으로 유효한지 확인한다.

- id와 필수 필드가 유효함
- `moveLimit`과 `targetCompletedOrders`가 1 이상
- `DreamBag`과 `OrderDeck`이 비어 있지 않음
- preview count가 active slot보다 과하지 않음
- 모든 order requirement가 현재 action set으로 만들 수 있는 상태를 요구함
- solver가 clear 가능하다고 판단함
- `minMoves <= moveLimit`
- solver가 timeout 없이 종료함

### Design Validator

라운드가 풀리지만 지루하지 않은지 확인한다.

- `moveLimit - minMoves`가 너무 크거나 작지 않음
- first solution이 동일 액션 반복만으로 구성되지 않음
- 사용된 operation 종류가 최소 기준을 만족함
- 첫 3턴 안에 선택지가 1개로 고정되지 않음
- active order 중 둘 이상의 주문 후보가 같은 꿈을 두고 경쟁하는 순간이 있음
- preview가 실제 판단에 영향을 줄 수 있는 상황이 있음
- storage pressure가 존재하되, 공간 퍼즐만으로 난이도가 결정되지 않음
- storage move ratio가 높아 상태조율보다 이동 피로가 커지지 않음
- dead end가 너무 많지 않음
- 유사한 꿈이 너무 많아 UI 판독이 어려운 상태가 아님

### Metrics

```text
DifficultyScore
  minMoves
  moveSlack
  conversionCount
  operationDiversity
  orderAmbiguity
  dreamOrderCompetition
  storagePressure
  storageMoveRatio
  previewDependency
  branchingFactor
  deadEndRate
```

점수는 절대 기준이 아니라 후보 라운드 정렬과 리뷰 우선순위를 위한 것이다.

## Generator Strategy

generator는 마지막에 만든다. 첫 generator는 자동 콘텐츠 생산기가 아니라 후보
라운드 제안기다.

### Stage Recipe

```text
StageRecipe
  recipeId
  allowedAttributes
  allowedOperations
  dreamBagTemplate
  orderDeckTemplate
  moveLimitRange
  targetCompletedOrdersRange
  desiredDifficultyRange
  requiredDesignTags
  forbiddenPatterns
```

`StageRecipe`의 template은 weight를 사용할 수 있다. 단, generator는 candidate
`RoundDefinition`을 만들 때 weight를 확정 count로 materialize해야 한다.

### Candidate Generation Flow

```text
1. recipe에서 dream bag 후보 생성
2. recipe에서 order deck 후보 생성
3. seed N개로 stream 순서 생성
4. hard validator 실행
5. solver 실행
6. metrics 계산
7. design validator 실행
8. accepted/rejected 결과와 이유 저장
```

### Rejection Reasons

generator는 reject 이유를 기록해야 한다.

- impossible order
- solver timeout
- min moves too high
- move slack too high
- boring repeated action pattern
- no order competition
- no preview relevance
- storage-only difficulty
- too many similar dreams

이유가 있어야 recipe를 조정할 수 있다.

## Round Types

초기 lab에서 다룰 라운드 타입:

### Type A - State Assignment

여러 주문이 같은 꿈 후보를 두고 경쟁한다.

목표:

- 주문 배정 고민 검증
- 상태 축이 가치 차이를 만드는지 확인

### Type B - Operation Ordering

`Soothe`와 `Clarify`의 순서가 결과 또는 비용에 영향을 준다.

목표:

- 정답 공식 고착화 방지
- 액션 side effect의 의미 검증

### Type C - Stream Timing

현재 주문보다 다음 주문 preview가 더 좋은 배정 후보를 만든다.

목표:

- preview가 판단에 영향을 주는지 확인
- 즉시 제출과 보류의 tension 검증

### Type D - Storage Pressure

상태조율을 하고 싶지만 보관 공간이 부족하다.

목표:

- 공간 압박이 핵심 상태 퍼즐을 보조하는지 확인
- 순수 주차 퍼즐로 변질되는지 감시

### Type E - Reversal Order

일부 주문은 `Nightmare`, `Anxious`, `Blurry` 같은 일반적으로 나빠 보이는
상태를 요구한다.

목표:

- "좋게 만들기"가 아니라 "정확히 맞추기"라는 정체성 검증
- 주문 판독 UX 리스크 확인

## Additional Considerations

### Replay And Debuggability

동적 라운드는 버그가 발생했을 때 같은 상황을 재현할 수 있어야 한다.

필수 기록:

- `roundId`
- `seed`
- action list
- solver limit
- accepted/rejected reason
- final state hash

이 기록은 PR 본문이나 manual playtest note에도 쓰일 수 있다.

### State Space Budget

stream, preview, active slot이 커지면 solver 비용이 빠르게 증가한다.

초기 제한:

```text
activeDreamSlots <= 5
activeOrderSlots <= 3
dreamPreviewCount <= 2
orderPreviewCount <= 1
storageSlotCount <= 3
targetCompletedOrders <= 6
moveLimit <= 20
```

이 제한은 재미 기준이 아니라 solver와 실험 속도를 위한 안전장치다.

### Report Format

batch simulation은 사람이 읽을 수 있는 요약을 남겨야 한다.

권장 report 필드:

```text
roundId
recipeId
seed
solvable
minMoves
moveLimit
moveSlack
difficultyScore
warnings
rejectReasons
firstSolutionActions
metricsSummary
```

기본 report 파일은 commit하지 않는다. 필요하면 `DreamLaundromat/Logs/` 또는
ignore된 output 경로에 쓴다.

### Migration From Prototype

기존 prototype의 `stain/moisture` 모델과 dynamic lab의
`taint/mood/clarity/stability` 모델은 의미가 다르다.

초기에는 강제 통합하지 않는다. 대신 아래 중 하나를 후속 결정으로 남긴다.

- prototype rules를 dynamic model로 교체
- dynamic lab을 별도 mode로 유지
- adapter로 `stain=Nightmare`를 `taint=Nightmare`에 매핑하고 `moisture`를
  `stability` 또는 별도 축으로 재해석

### Manual Playtest Notes

solver와 metric만으로 재미를 확정하지 않는다. 사람이 accepted round를 직접
플레이할 때 아래를 기록한다.

- 첫 판단까지 걸린 시간
- 첫 실패 이유가 명확했는지
- preview를 보고 선택을 바꾼 순간이 있었는지
- 어떤 꿈을 어떤 주문에 줄지 고민했는지
- 공간 때문에 상태조율이 더 재미있어졌는지, 단순히 답답했는지

## Scene And UI Plan

PR 1-3의 기본 방침은 scene/UI를 만들지 않는 것이다. `Dynamic Puzzle Lab`은 먼저
순수 model, solver, validator, generator를 안정화한다.

선택적 PR 4에서만 debug play surface를 만든다.

```text
DynamicLabDebug
  SeedHeader
    RoundId
    Seed
    RemainingMoves
  ActiveOrders
  OrderPreview
  ActiveDreams
  DreamPreview
  StorageSlots
  ActionPanel
    OperationButtons
    SubmitButtons
    StoreRecallButtons
  LogPanel
    LastAction
    FailureReason
    StateHash
```

Scene-owned:

- debug presenter
- button/input binding
- visual state rows

Data/model-owned:

- `RoundDefinition`
- `RoundState`
- action validation
- operation preview result
- seed replay

이 debug UI는 출시 UI가 아니다. accepted round를 사람이 직접 플레이하고 solver
metric과 체감을 비교하기 위한 최소 표면이다.

## UX / Interaction Implications

초기 lab은 UI polish를 목표로 하지 않지만, model 결정이 UI를 불가능하게 만들면
안 된다.

필수 판독 정보:

- 현재 꿈의 4개 상태 축
- 현재 주문이 요구하는 상태 축
- 제출 가능 여부
- operation preview
- 다음 꿈 2개 preview
- 다음 주문 1개 preview
- 남은 move
- 공간/슬롯 상태

UI 리스크:

- 4개 상태 축이 한 카드에 모두 들어가야 한다.
- preview가 많아지면 모바일 세로 화면에서 압박이 커진다.
- `Anxious`, `Calm`, `Blurry`, `Vivid`는 색상만으로 구분하면 안 된다.
- `Settle`이 마무리인지 변환인지 플레이어가 헷갈릴 수 있다.

초기 debug UI 원칙:

- 예쁘기보다 상태와 변화가 정확히 보여야 한다.
- operation preview가 반드시 있어야 한다.
- 랜덤 stream의 seed와 draw order를 표시할 수 있어야 한다.

## Milestones

### M0 - Lab Design Document

이 문서를 작성하고 자체 검토한다.

Gate:

- 상태 축, operation, random policy, solver/validator/generator 순서가 명확하다.
- 초기 구현 기본값이 닫혀 있고, 후속 실험 항목이 분리되어 있다.

### M1 - Pure Dynamic Round Model

scene 없이 `RoundDefinition`, `RoundState`, `DreamBag`, `OrderDeck`, action model을
구현한다.

Gate:

- 같은 seed와 action list가 같은 결과를 만든다.
- stream draw와 preview가 테스트된다.
- `Wash`, `Soothe`, `Clarify`, `Settle`의 최소 동작이 테스트된다.

### M2 - Solver And Hard Validator

라운드가 클리어 가능한지와 최소 턴을 계산한다.

Gate:

- 수제 lab round 5개 이상을 solver가 푼다.
- 불가능한 round를 validator가 reject한다.
- solver timeout과 visited state limit이 있다.

### M3 - Metrics And Design Validator

풀리지만 지루한 라운드를 정량/규칙 기반으로 걸러낸다.

Gate:

- repeated action pattern을 감지한다.
- order competition 또는 preview relevance가 없는 round를 경고한다.
- storage-only difficulty를 경고한다.

### M4 - Generator Prototype

`StageRecipe`에서 후보 round를 만들고 solver/validator loop를 돌린다.

Gate:

- recipe 3개에서 accepted round를 생성한다.
- rejected round의 reason이 기록된다.
- seed별 결과가 재현 가능하다.

### M5 - Debug Play Surface

필요한 경우 최소 debug UI 또는 console runner로 사람이 라운드를 플레이한다.

Gate:

- active dreams/orders/preview가 보인다.
- operation preview가 보인다.
- 특정 seed round를 재현해서 플레이할 수 있다.

## Task Breakdown

### DLAB-001 - Dynamic Lab Folder And Assembly

- Outputs:
  - `Assets/_Project/Scripts/DynamicLab/`
  - lab runtime asmdef 또는 기존 runtime asmdef 편입 결정
  - `Assets/_Project/Tests/EditMode/DynamicLab/`
- Work:
  - 기존 prototype rules와 분리된 폴더를 만든다.
  - public API와 internal helper 범위를 정한다.
- Verification:
  - Unity compile
  - Edit Mode test assembly compile
- Done criteria:
  - 기존 prototype scene을 건드리지 않고 lab code를 추가할 수 있다.

### DLAB-002 - Dynamic Attribute Model

- Outputs:
  - `DynamicDreamAttributes`
  - `DreamTaint`
  - `DreamMood`
  - `DreamClarity`
  - `DreamStability`
- Work:
  - 4개 상태 축을 명시적 enum/value로 정의한다.
  - equality, copy, hash를 solver에 적합하게 만든다.
- Verification:
  - attribute equality/copy/hash tests
- Done criteria:
  - attributes가 state hash에 안정적으로 들어간다.

### DLAB-003 - Operation Rules

- Outputs:
  - `DynamicOperation`
  - `DynamicRulesEngine`
  - operation result model
- Work:
  - `Wash`, `Soothe`, `Clarify`, `Settle` 최소 규칙을 구현한다.
  - 실패 사유를 명확한 enum/string으로 반환한다.
  - `Soothe`와 `Clarify` side effect는 첫 구현에서 넣지 않는다.
- Verification:
  - operation transform tests
  - invalid operation tests
- Done criteria:
  - operation 결과가 scene 없이 예측 가능하다.

### DLAB-004 - Round Definition And State

- Outputs:
  - `DynamicRoundDefinition`
  - `DynamicRoundState`
  - `DynamicPlayerAction`
  - `DynamicStorageConfig`
  - `DynamicRoundStatus`
  - `DynamicFailureReason`
  - state snapshot
- Work:
  - active dreams/orders, preview, remaining bag/deck, remaining moves를 표현한다.
  - `ApplyOperation`, `SubmitDream`, `StoreDream`, `RecallDream` action model을
    정의한다.
  - storage slot capacity 1 모델을 구현한다.
  - clear/failure condition을 runtime status로 표현한다.
  - undo 가능한 immutable 또는 copy-safe state snapshot을 만든다.
- Verification:
  - state initialization tests
  - state copy mutation isolation tests
  - action enumeration tests
  - storage movement tests
  - clear/failure status tests
- Done criteria:
  - action sequence replay가 안정적으로 가능하다.

### DLAB-005 - Seeded Stream

- Outputs:
  - `DreamBag`
  - `OrderDeck`
  - deterministic shuffler
  - preview draw logic
- Work:
  - seed 기반 shuffle과 draw cursor를 구현한다.
  - active slot과 preview slot refill 규칙을 구현한다.
  - `RoundDefinition`의 bag/deck은 count-only로 처리하고, weight는 받지 않는다.
- Verification:
  - same seed same stream tests
  - different seed different stream smoke tests
  - preview/refill tests
  - count-only materialized bag/deck tests
- Done criteria:
  - stream random이 재현 가능하다.

### DLAB-006 - Order Matching And Submit

- Outputs:
  - `DynamicOrderRequirement`
  - submit validation
  - completed order flow
- Work:
  - optional attribute matching을 구현한다.
  - 주문 완료 시 새 주문 draw/refill을 처리한다.
  - 제출 시 새 꿈 draw/refill 정책을 확정하고 구현한다.
  - `SubmitDream`은 `stability=Stable`인 active dream만 허용한다.
  - `requiredStability=Unsettled` 주문은 invalid로 처리한다.
- Verification:
  - partial matching tests
  - submit/refill tests
  - stable-only submit tests
  - invalid unstable order tests
- Done criteria:
  - active order flow가 deterministic하게 바뀐다.

### DLAB-007 - Sample Handwritten Rounds

- Outputs:
  - 5-10개 lab round definitions
  - round intent notes
- Work:
  - Type A-E를 최소 1개씩 포함한다.
  - generator 없이 사람이 의도한 라운드를 만든다.
- Verification:
  - load/initialize tests
- Done criteria:
  - solver/validator 개발용 기준 라운드가 있다.

### DLAB-008 - Hard Validator

- Outputs:
  - `DynamicRoundHardValidator`
  - validation result report
- Work:
  - 필수 필드, empty bag/deck, impossible order, invalid counts를 검사한다.
  - solver hook은 다음 task에서 연결한다.
- Verification:
  - invalid round tests
  - valid sample round tests
- Done criteria:
  - 명백히 잘못된 라운드를 solver 전에 reject한다.

### DLAB-009 - Solver

- Outputs:
  - `DynamicRoundSolver`
  - `SolveResult`
  - state hash/pruning
- Work:
  - BFS 기반 최소 턴 solver를 구현한다.
  - timeout과 visited state limit을 둔다.
  - first solution action list를 반환한다.
- Verification:
  - known solvable round tests
  - known impossible round tests
  - min move expectation tests
  - timeout/limit tests
- Done criteria:
  - sample round의 클리어 가능성과 최소 턴을 계산한다.

### DLAB-010 - Metrics

- Outputs:
  - `DynamicRoundMetrics`
  - metric report
- Work:
  - solver output과 round definition에서 난이도/재미 후보 지표를 계산한다.
  - repeated action pattern, operation diversity, move slack을 계산한다.
- Verification:
  - metric calculation tests
- Done criteria:
  - round 후보를 비교할 수 있는 숫자와 tags가 나온다.

### DLAB-011 - Design Validator

- Outputs:
  - `DynamicRoundDesignValidator`
  - warning/reject reason list
- Work:
  - boring pattern, no order competition, no preview relevance, storage-only
    difficulty를 감지한다.
  - 첫 버전은 heuristic으로 충분하다.
- Verification:
  - intentionally boring round tests
  - intentionally interesting round smoke tests
- Done criteria:
  - 풀리지만 좋지 않은 라운드가 경고 또는 reject된다.

### DLAB-012 - Stage Recipe

- Outputs:
  - `StageRecipe`
  - recipe validation
  - 3개 sample recipe
- Work:
  - allowed attributes/operations, bag/deck template, difficulty target을 표현한다.
  - recipe 자체의 모순을 검사한다.
- Verification:
  - recipe validation tests
- Done criteria:
  - generator가 사용할 안전한 입력 구조가 있다.

### DLAB-013 - Candidate Generator

- Outputs:
  - `DynamicRoundGenerator`
  - accepted/rejected candidate report
- Work:
  - recipe와 seed range에서 round candidate를 생성한다.
  - hard validator, solver, metrics, design validator를 순서대로 호출한다.
- Verification:
  - generator determinism tests
  - accepted candidate smoke tests
  - rejection reason tests
- Done criteria:
  - seed batch에서 accepted round 후보가 생성된다.

### DLAB-014 - Batch Simulation Entry

- Outputs:
  - Unity Edit Mode test 또는 editor menu/CLI entry
  - generated report file은 기본적으로 commit하지 않음
- Work:
  - sample recipes를 여러 seed로 돌린다.
  - accepted/rejected 요약을 출력한다.
  - report에 `roundId`, `seed`, metric, warning, reject reason을 포함한다.
- Verification:
  - batch run test
  - generated output ignored check if files are written
- Done criteria:
  - 라운드 생성/검증 루프를 반복 실행할 수 있다.

### DLAB-015 - Debug Play Surface

- Outputs:
  - 최소 debug presenter 또는 console-like play harness
- Work:
  - 특정 `roundId`와 `seed`를 로드한다.
  - action을 입력하고 state 변화를 볼 수 있게 한다.
  - 이 task는 M1-M4가 안정된 뒤에만 진행한다.
- Verification:
  - Play Mode smoke test if Unity scene is used
  - manual debug playthrough
- Done criteria:
  - 사람이 accepted round를 직접 플레이해 metric과 체감을 비교할 수 있다.

## PR Plan

권장 PR split:

### PR 1 - Pure Dynamic Round Model

- `DLAB-001`부터 `DLAB-007`
- 상태 축, operation, seeded stream, 수제 sample rounds
- Edit Mode tests 중심

### PR 2 - Solver And Validation

- `DLAB-008`부터 `DLAB-011`
- hard validator, solver, metrics, design validator
- known round 기반 tests

### PR 3 - Generator And Batch Lab

- `DLAB-012`부터 `DLAB-014`
- stage recipe, candidate generator, batch simulation
- accepted/rejected report 검증

### PR 4 - Debug Play Surface

- `DLAB-015`
- 필요할 경우에만 진행
- UI/scene diff가 생기므로 rules/generator와 분리한다.

PR을 하나로 합칠 수도 있지만 추천하지 않는다. solver/generator는 리뷰 난이도가
높고, UI가 섞이면 핵심 규칙 검토가 흐려진다.

## Verification And Test Plan

공통 검증:

- `git status --short --branch`
- changed hand-authored files whitespace check
- generated folder/build output 포함 여부 확인
- Unity compile
- `.\DreamLaundromat\test.cmd -Mode EditMode`

PR 1 검증:

- attribute equality/copy/hash tests
- operation transform tests
- action enumeration tests
- storage movement tests
- clear/failure status tests
- seeded stream determinism tests
- count-only bag/deck tests
- stable-only submit tests
- sample round initialization tests

PR 2 검증:

- hard validator valid/invalid tests
- solver known solvable/impossible tests
- min move expectation tests
- solver timeout/visited limit tests
- metrics calculation tests
- boring round design validator tests

PR 3 검증:

- recipe validation tests
- generator determinism tests
- candidate acceptance/rejection tests
- batch simulation smoke

PR 4 검증:

- Play Mode smoke test if scene is used
- mobile portrait readability manual check
- operation preview manual check
- accepted round manual playthrough

수동 확인:

- solver 최소 턴이 사람이 보기에도 납득되는지
- design validator가 실제 재미와 어느 정도 맞는지
- preview count가 공정성과 UI 부담 사이에서 적절한지
- 라운드가 공간관리로만 느껴지지 않는지
- `Soothe`와 `Clarify`가 실제 딜레마를 만드는지

## CLI And Manual Boundary

CLI에서 가능한 작업:

- pure model 구현
- Edit Mode tests
- solver/validator/generator batch simulation
- seed replay
- round report 출력
- PR 생성/리뷰

수동 확인이 필요한 작업:

- accepted round의 실제 재미 판단
- 모바일 화면에서 4개 상태 축과 preview가 읽히는지
- 랜덤 흐름이 공정하게 느껴지는지
- tutorial이 필요한 지점 판단
- PR merge

## Risks

- 4개 상태 축이 모바일 UI에서 너무 복잡할 수 있다.
- solver state space가 stream과 preview 때문에 급격히 커질 수 있다.
- design validator가 재미를 지나치게 단순한 숫자로 오판할 수 있다.
- generator가 solver를 통과하지만 사람이 보기에는 재미없는 라운드를 많이 만들 수 있다.
- `Soothe`와 `Clarify` side effect가 불명확하면 규칙이 혼란스럽다.
- 랜덤 preview가 부족하면 운빨로 느껴지고, 너무 많으면 계산 피로가 생긴다.
- `Settle`이 모든 제출 전 필수 절차가 되면 반복 작업처럼 느껴질 수 있다.
- storage action cost가 높게 느껴지면 상태조율보다 이동 피로가 커질 수 있다.
- 기존 prototype rules와 새 lab rules가 혼재되면 코드 의미가 흐려질 수 있다.
- debug UI를 너무 일찍 만들면 rules 검증보다 scene 작업이 앞설 수 있다.

대응:

- M1-M4까지는 scene/UI를 최소화한다.
- solver limit을 명시하고 작은 sample round부터 시작한다.
- design validator는 reject보다 warning 중심으로 시작한다.
- sample handwritten rounds를 먼저 만들고 generator는 마지막에 붙인다.
- side effect는 첫 구현에서 넣지 않고, 후속 experiment flag로만 다룬다.
- sample round에 이미 `Stable`인 꿈과 `Unsettled`인 꿈을 섞어 `Settle` 반복감을
  점검한다.
- design validator가 storage-only difficulty와 excessive storage move ratio를
  경고하도록 한다.

## Deferred Or Out Of Scope

게임별 후속 항목:

- 출시용 level editor UI
- full hint system
- liveops event recipe editor
- analytics-based difficulty tuning
- monetization hooks
- localization
- store release setup
- production art/audio

이 항목들은 게임별 후속 backlog다. 공통 저장소 정책이나 여러 게임에 영향을 주는
인프라가 아니라면 `docs/TODO.md`에 넣지 않는다.

## Implementation Defaults

이번 계획에서 초기 구현 기본값은 아래처럼 닫는다. 사용자가 명시적으로 바꾸지
않는 한 이 기본값으로 구현한다.

1. PR split
   - 결정: 3개 PR + 선택적 debug UI PR
   - 영향: 리뷰 가능성과 회귀 추적이 좋아진다. 대신 PR 수가 늘어난다.

2. `Soothe` side effect
   - 결정: 첫 구현에서는 `Anxious -> Calm`만 적용하고, `Vivid -> Blurry`는
     design experiment flag로 둔다.
   - 영향: solver와 UI 복잡도를 낮추고, side effect 재미는 별도 라운드에서 검증한다.

3. `Clarify` side effect
   - 결정: 첫 구현에서는 `Blurry -> Vivid`만 적용하고 hidden reveal은 제외한다.
   - 영향: 불공정 랜덤을 피하고, 정보 공개 규칙은 후속 실험으로 남긴다.

4. `Settle` 정책
   - 결정: `Stable` 상태가 제출 기본 조건이다. `Unsettled` 제출 주문은 첫 lab에서
     invalid로 처리한다.
   - 영향: `Settle`이 마무리 액션으로 의미를 갖지만, 모든 제출 전 필수 절차가 되어
     반복 작업처럼 느껴질 위험이 있다. 일부 주문이나 초반 라운드에서는 자동
     stability를 허용할지 검토해야 한다.

5. Debug play surface timing
   - 결정: solver/validator/generator가 돌아간 뒤 별도 PR로 진행한다.
   - 영향: 순수 모델 검증이 먼저 안정된다. 대신 사람이 플레이하며 체감하는 시점은
     조금 늦어진다.

6. Bag/deck materialization
   - 결정: `RoundDefinition`은 count-only bag/deck만 받는다. weight는
     `StageRecipe`에서만 허용한다.
   - 영향: solver replay와 seed 재현성이 단순해진다.

7. Storage model
   - 결정: 첫 lab storage는 capacity 1 slot 목록이다. storage dream은 operation과
     submit 대상이 아니며, active slot으로 recall해야 한다.
   - 영향: 공간 압박은 유지하되 storage packing 게임으로 커지는 것을 막는다.

## Self-Review

자체 점검 결과:

- Core Fun: 공간관리가 아니라 상태조율과 주문 배정을 중심에 두었다.
- Game Pillars: deterministic action, seeded input, preview fairness, solver-first
  production loop가 서로 충돌하지 않게 정리했다.
- Core Rules: 4개 상태 축, 4개 operation, player action model, storage model,
  clear/failure condition을 초기 구현 기본값으로 닫았다.
- Puzzle Grammar: DreamBag, OrderDeck, stream preview, round type A-E로 라운드
  문법을 확장했다.
- Content Production: generator를 먼저 만들지 않고 solver/validator/metrics 위에
  얹도록 순서를 조정했다.
- UX: preview와 4개 상태 축이 모바일에서 읽혀야 한다는 리스크를 명시했다.
- Satisfaction: 이번 plan은 손맛보다 규칙 실험에 집중하므로 visual/audio polish를
  non-goal로 두었다.
- Prototype Criteria: solvability뿐 아니라 boring pattern, order competition,
  preview relevance를 검증 기준에 포함했다.
- Verification: 자동 tests, batch simulation, manual checks를 분리했다.
- PR Boundaries: pure model, solver/validation, generator, debug UI를 분리했다.

수정 반영:

- `generator`를 초기 milestone에서 뒤로 미루고 handwritten rounds와 solver를 먼저
  배치했다.
- 랜덤을 input stream으로 제한하고 기계 결과 랜덤을 명시적으로 제외했다.
- `storage-only difficulty`를 design validator의 경고 대상으로 추가했다.
- `Soothe`, `Clarify`, `Settle`의 초기 구현 정책을 `Implementation Defaults`로
  확정하고, 복잡한 side effect는 후속 experiment로 분리했다.
- replay/debuggability, state space budget, report format, prototype migration,
  manual playtest note를 추가 고려사항으로 보강했다.
- solver가 열거할 `DynamicPlayerAction`, storage slot 규칙, clear/failure status,
  count-only bag/deck 정책, scene/UI plan을 추가했다.

## First Implementation Step

사용자가 이 계획을 승인하면 다음 순서로 진행한다.

1. `game/dream-laundromat-dynamic-lab` 브랜치를 만든다.
2. `DLAB-001`부터 시작해 lab folder와 test folder를 만든다.
3. `DLAB-002`와 `DLAB-003`으로 4개 상태 축과 operation rules를 구현한다.
4. Edit Mode tests로 pure model을 먼저 고정한다.
5. scene/UI 작업은 solver/validator 이후로 미룬다.

## Implementation Progress

현재 PR 범위에서 `M1`부터 `M5`까지 구현했다.

- `M1`: `DynamicRoundDefinition`, `DynamicRoundState`, seeded stream, storage, sample handwritten rounds를 추가했다.
- `M2`: hard validator, BFS solver, state hash, solver limit을 추가했다.
- `M3`: metrics와 design validator를 추가해 direct-submit, 반복 operation, storage 편중, 낮은 branching, 낮은 preview relevance, `Settle` 반복 세금, 기계적인 operation-submit cadence를 경고한다.
- `M4`: `DynamicStageRecipe`, candidate generator, batch simulator, accepted/rejected report를 추가했다. sample recipe에는 design intent, player question, risk note를 함께 기록한다.
- `M5`: `DynamicLabDebugGame`과 `DynamicLabDebug.unity` debug play surface를 추가했다. 이 화면은 accepted candidate를 로드하고 active dreams, active orders, preview, storage, operations를 직접 눌러 확인하는 용도다.

추가 보강:

- solver가 찾은 첫 해답을 `DynamicRulesEngine`으로 다시 replay해 실제 clear까지 도달하는지 검증한다.
- generator acceptance는 최소 move뿐 아니라 action type diversity, repeated action type run, storage ratio, `Settle` ratio를 함께 본다.
- `.\DreamLaundromat\dynamic-lab.cmd`로 sample recipe batch report를 CLI에서 생성할 수 있다.

검증 상태:

- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`: 32개 통과.
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`: 6개 통과.
- `.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900`: report 생성 성공.

아직 남긴 범위:

- 실제 모바일 화면에서 preview, 4축 상태 표시, 조작 피드백이 읽히는지는 수동 플레이테스트로 확인해야 한다.
- debug scene은 체감 평가용 최소 화면이므로, 출시 UI/튜토리얼/연출 품질은 별도 구현 범위로 남긴다.
