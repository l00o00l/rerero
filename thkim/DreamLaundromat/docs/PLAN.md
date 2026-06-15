# DreamLaundromat Implementation Plan

## Summary

`DreamLaundromat`은 `Dream Laundromat` 컨셉을 Unity 모바일 세로 화면 퍼즐로 검증하는 프로토타입이다.

이번 구현은 바로 완성형 게임을 만드는 것이 아니라, 아래 순서로 핵심 리스크를 줄인다.

1. `Rules Prototype`: Unity UI와 분리된 순수 퍼즐 규칙 모델, 10개 레벨 데이터, 검증 테스트를 먼저 만든다.
2. `Playable Prototype`: 검증된 규칙 모델을 Unity scene/UI에 연결해 한 손 조작으로 실제 플레이할 수 있게 만든다.

사용자는 PR을 1개로 원하므로, 구현은 하나의 PR 안에서 진행한다. 대신 PR 내부 milestone과 verification gate를 분리해 리뷰 가능성을 유지한다.

## Planning References

- [Dream Laundromat concept](../../../concepts/puzzle/dream-laundromat.md)
- [Dream Laundromat planning index](../../../concepts/puzzle/dream-laundromat-planning/README.md)
- [Core Rules](../../../concepts/puzzle/dream-laundromat-planning/03-core-rules.md)
- [Puzzle Grammar](../../../concepts/puzzle/dream-laundromat-planning/04-puzzle-grammar.md)
- [Level Progression](../../../concepts/puzzle/dream-laundromat-planning/05-level-progression.md)
- [Prototype Success Criteria](../../../concepts/puzzle/dream-laundromat-planning/10-prototype-success-criteria.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)

## Prototype Goal

검증할 핵심 가설:

> 세탁기와 건조기, 제한된 바구니만으로도 꿈 조각을 변환하고 정리하는 퍼즐 재미가 생긴다.

구체적으로는 아래를 확인한다.

- `stain=Nightmare -> Washer -> stain=None, moisture=Wet -> Dryer -> moisture=Dry -> Submit` 체인이 이해 가능한가
- 바구니 capacity 제약이 단순 방해가 아니라 순서 고민을 만드는가
- 주문이 일부 속성만 요구하는 구조가 레벨 다양성을 만드는가
- 10개 수제 레벨이 코드 수정 없이 데이터로 표현되는가
- 탭 선택 -> 탭 목적지 조작으로 모바일 세로 화면에서 플레이 가능한가

## Scope

이번 프로토타입에 포함한다.

- Unity project shell: `DreamLaundromat/`
- Android 우선 설정
- Portrait orientation
- `Assets/_Project/` 기반 구조
- 순수 C# rules model
- 속성: `stain=None/Nightmare`, `moisture=Dry/Wet`
- 기계: `Washer`, `Dryer`
- 공간: `Queue`, `Basket`, `Machine`, `Order`
- 바구니 2개, 정수 `capacity`
- 턴 제한
- 무제한 Undo
- Restart
- 10개 수제 레벨 데이터
- 최소 level validator
- Edit Mode tests
- 최소 playable scene
- 탭 선택 -> 탭 목적지 입력
- 주문/꿈/기계/바구니 기본 UI
- 세탁/건조/제출의 간단한 visual feedback
- 게임 로컬 커스텀 UI 아이콘: 꿈 상태, 기계, 주문, 보관 목적지를 구분하는 최소 PNG sprite
- game-local `run.cmd`와 `test.cmd`
- Android batchmode import/build check
- 가능하면 Android emulator smoke test

## Non-Goals

이번 프로토타입에서 제외한다.

- 염색
- 접기
- 수선
- 감정 상태
- 메타 성장
- 이벤트
- 광고/수익화
- 자동 레벨 생성기
- 고급 아트
- 고급 사운드
- localization
- cloud save
- analytics
- store release 설정
- 실제 Google Play 배포

## Key Decisions

- PR은 1개로 만든다.
- PR 안의 구현 순서는 `Rules Prototype -> Playable Prototype`이다.
- Undo는 프로토타입에서 무제한으로 제공한다.
- Hint는 이번 범위에서 제외한다. 대신 변환 preview와 실패 이유 표시를 우선한다.
- 규칙 모델은 `MonoBehaviour`와 분리한다.
- 레벨 데이터는 초기에는 `ScriptableObject`로 만든다. 단, 구조는 JSON 직렬화가 가능하도록 단순한 필드로 유지한다.
- 주문은 필요한 속성만 지정하고, 지정하지 않은 속성은 비교하지 않는다.
- 모든 꿈 조각의 `capacityCost`는 프로토타입에서 `1`이다.
- 기계는 즉시 처리한다. 실시간 대기형 기계는 제외한다.
- 한 턴은 `Move`, `Wash`, `DryInDryer`, `Submit` 같은 player action 1회로 계산한다.
- Android graphics API는 PocketDodger와 동일하게 OpenGLES3-only를 우선한다.
- 프로토타입 평가를 위해 초반 4레벨은 `바로 제출 -> 세탁 -> 세탁+건조 -> 보관 압박` 순서로 핵심 규칙을 빠르게 노출한다.
- UI는 출시 품질 polish보다 `Submit`, `Machine`, `Storage`의 목적을 명확히 드러내는 것을 우선한다.
- 이번 아트 패스는 유료/외부 에셋을 쓰지 않고 `Assets/_Project/Art/UI/` 아래에 직접 생성한 아이콘만 사용한다.
- 유료 에셋은 프로토타입 기본값이 아니며, 별도 승인과 라이선스/저장소 공개 범위 확인 후에만 도입한다.

## Target Platforms

- Primary target: Android mobile
- Orientation: Portrait only
- Input: 한 손 tap 선택 -> tap 목적지 입력
- Build target: Unity Android
- Run script: `DreamLaundromat/run.cmd`, 내부적으로 `scripts/run-emulator.ps1` 사용
- Test script: `DreamLaundromat/test.cmd`, 내부적으로 `scripts/run-tests.ps1` 사용
- Smoke target: Android Studio AVD `PocketDodger_API36` 또는 연결된 Android 기기
- Manual boundary: 실제 기기 승인, 손맛, 화면 밀도별 가독성, 장시간 플레이 감각은 사람이 확인한다.
- Secondary platforms: 이번 PR에서는 없음. 단, rules model과 level data는 `MonoBehaviour`와 분리해 둬서 desktop/다른 플랫폼으로 옮길 때 재사용할 수 있게 유지한다.

## Architecture

Unity project path:

```text
DreamLaundromat/
  Assets/
    _Project/
      Art/
        UI/
      Audio/
      Editor/
      Materials/
      Prefabs/
      Scenes/
      ScriptableObjects/
      Scripts/
      Settings/
      Tests/
      UI/
  Packages/
  ProjectSettings/
  docs/
    PLAN.md
  scripts/
    run-emulator.ps1
    run-tests.ps1
  run.cmd
  test.cmd
```

Namespace root:

```text
Thkim.DreamLaundromat
```

Runtime script groups:

```text
Assets/_Project/Scripts/
  Rules/
  Levels/
  Gameplay/
  UI/
  Input/
  Infrastructure/
```

Rules model은 Unity scene 없이 테스트 가능해야 한다.

```text
LevelDefinition
  -> LevelState
  -> PlayerAction
  -> RulesEngine.Apply(action)
  -> LevelState
  -> LevelResult / ValidationResult
```

Unity scene은 rules model을 표시하고 action을 전달하는 역할만 맡는다.

## Data Model

### Core Types

```text
DreamAttributes
  stain: DreamStain
  moisture: DreamMoisture

DreamFragment
  id
  displayName
  attributes
  capacityCost

OrderRequirement
  requiredStain: optional DreamStain
  requiredMoisture: optional DreamMoisture
  count

MachineDefinition
  id
  type: Washer | Dryer
  capacity

BasketDefinition
  id
  capacity

LevelDefinition
  levelId
  moveLimit
  dreams
  machines
  baskets
  orders
  tutorialHint
```

### Runtime State

```text
LevelState
  remainingMoves
  locations
  dreamStates
  orderProgress
  status: Ready | Playing | Cleared | Failed
  failureReason
```

### Actions

```text
PlayerAction
  MoveToBasket(dreamId, basketId)
  MoveToMachine(dreamId, machineId)
  TakeFromMachine(dreamId, destinationId)
  Submit(dreamId, orderId)
```

세탁/건조는 `MoveToMachine` action 처리 중 즉시 변환된다. 이후 실제 연출은 UI에서 짧은 animation으로 표현한다.

### Validation

최소 validator는 아래를 확인한다.

- level id가 비어 있지 않음
- dream id 중복 없음
- machine/basket/order id 중복 없음
- 모든 order가 만들 수 있는 속성 조건을 요구함
- 총 dream capacity가 시작 공간에 들어갈 수 있음
- move limit이 1 이상임
- 초기 10레벨이 순서대로 요구 규칙을 노출함

완전 solver는 이번 범위에서 제외한다.

## Scene And UI Plan

Main scene:

```text
MainGame
  Main Camera
  GameRoot
    GameController
    RulesPresenter
  Canvas
    SafeArea
      Top
        LevelText
        MoveCounter
        OrderRow
      Middle
        DreamQueue
        MachineRow
          WasherSlot
          DryerSlot
        BasketRow
          BasketA
          BasketB
      Bottom
        UndoButton
        RestartButton
        NextButton
      Overlay
        ResultPanel
        FailureToast
```

Interaction:

- 꿈 조각 탭: 선택
- 이동 가능한 목적지 강조
- 목적지 탭: action 실행
- 기계 탭 전 preview: 변환 결과 표시
- 불가능한 목적지 탭: 짧은 실패 이유 표시
- Undo: 직전 action 이전 상태로 복구
- Restart: 현재 level 초기화
- Next: clear 후 다음 level 로드

UI는 기획 검증용 placeholder로 충분하다. 단, `stain`과 `moisture`는 색상만이 아니라 아이콘/텍스트 조합으로 구분한다.

## Milestones

### M0 - Project Shell

Unity project, Android/Portrait baseline, folder layout, run/test wrapper를 만든다.

### M1 - Rules Prototype

순수 rules model, level data shape, 10개 레벨, validator, Edit Mode tests를 만든다.

Gate A:

- Unity scene 없이 10개 레벨 데이터가 로드된다.
- order matching, washer, dryer, capacity, move limit, undo가 Edit Mode tests로 검증된다.
- 10개 레벨의 기본 validation이 통과한다.

### M2 - Playable Prototype

Rules model을 scene/UI에 연결해 10개 레벨을 플레이할 수 있게 한다.

Gate B:

- mouse/touch tap으로 꿈 선택과 목적지 선택이 가능하다.
- level 1-10을 순서대로 플레이할 수 있다.
- clear/fail/restart/undo 흐름이 동작한다.
- 최소 Play Mode smoke test가 통과한다.

### M3 - Android Smoke And PR Hardening

Android import/build/run wrapper와 PR 검증을 정리한다.

Gate C:

- game-local `test.cmd`가 Unity Test Runner 결과 XML을 검증한다.
- Android batchmode import 또는 debug build가 통과한다.
- 가능하면 emulator install/run smoke를 수행한다.

## Task Breakdown

### DL-001 - Unity Project Shell

- Outputs:
  - `DreamLaundromat/` Unity project
  - `Assets/_Project/` folder layout
  - `MainGame.unity`
  - `run.cmd`, `test.cmd`
- Work:
  - Unity 6000.4.10f1 project를 생성한다.
  - Android target과 Portrait orientation을 설정한다.
  - generated folders가 Git에 들어가지 않도록 확인한다.
- Verification:
  - `git status --short --branch`
  - Unity Android batchmode import
- Done criteria:
  - project shell이 열리고 `.meta` 파일이 누락되지 않는다.

### DL-002 - Core Attribute Model

- Outputs:
  - `DreamStain`
  - `DreamMoisture`
  - `DreamAttributes`
  - `DreamFragment`
- Work:
  - `stain`과 `moisture`를 명시적 enum/value로 표현한다.
  - `capacityCost=1` 기본값을 둔다.
- Verification:
  - Edit Mode tests for attribute equality and copy behavior
- Done criteria:
  - scene object 없이 dream state를 비교하고 복사할 수 있다.

### DL-003 - Order Matching

- Outputs:
  - `OrderRequirement`
  - `CustomerOrder`
  - order matching tests
- Work:
  - 주문이 지정한 속성만 비교하도록 구현한다.
  - `stain=None`만 요구하는 주문과 `stain=None, moisture=Dry` 주문을 구분한다.
- Verification:
  - partial attribute matching tests
- Done criteria:
  - level 3-5에서 `moisture` 미지정 주문이 작동한다.

### DL-004 - Storage And Location Model

- Outputs:
  - `DreamLocation`
  - `BasketState`
  - `MachineState`
  - capacity tests
- Work:
  - Queue, Basket, Machine, Submitted location을 표현한다.
  - basket/machine capacity를 검사한다.
- Verification:
  - capacity limit tests
- Done criteria:
  - capacity가 없는 목적지로 이동할 수 없다.

### DL-005 - Machine Rules

- Outputs:
  - `MachineType`
  - `RulesEngine`
  - washer/dryer tests
- Work:
  - `Washer`: `stain=Nightmare -> stain=None`, `moisture=Wet`
  - `Dryer`: `moisture=Wet -> moisture=Dry`
  - 처리 불가능한 입력을 명확한 failure reason으로 반환한다.
- Verification:
  - washer and dryer transform tests
  - invalid machine input tests
- Done criteria:
  - 세탁/건조 체인이 rules model에서 재현된다.

### DL-006 - Turn, Clear, Failure, Undo

- Outputs:
  - `LevelState`
  - `PlayerAction`
  - `ActionResult`
  - `UndoStack`
- Work:
  - action 1회마다 move를 차감한다.
  - 모든 주문 완료 시 clear 처리한다.
  - move limit 초과와 불가능한 action을 구분한다.
  - 무제한 undo를 state snapshot 기반으로 구현한다.
- Verification:
  - move limit tests
  - clear/failure tests
  - undo tests
- Done criteria:
  - 플레이 가능한 한 판의 lifecycle이 scene 없이 검증된다.

### DL-007 - Level Data Shape

- Outputs:
  - `LevelDefinition`
  - `DreamDefinition`
  - `MachineDefinition`
  - `BasketDefinition`
  - `OrderDefinition`
- Work:
  - ScriptableObject 기반 level data를 정의한다.
  - JSON으로 옮기기 쉬운 단순 필드만 사용한다.
- Verification:
  - level definition construction tests
- Done criteria:
  - 코드 변경 없이 level data asset으로 레벨을 정의할 수 있다.

### DL-008 - Ten Prototype Levels

- Outputs:
  - 10 level assets
  - level list asset
- Work:
  - level 1: match and submit
  - level 2: first washing and machine output storage
  - level 3: washer + dryer conversion chain
  - level 4: first visible storage capacity pressure
  - levels 5-10: order assignment, drying, conversion chain, and stronger basket pressure
- Verification:
  - all prototype levels load
  - all prototype levels pass validator
- Done criteria:
  - 10개 수제 레벨이 data로 존재한다.

### DL-009 - Level Validator

- Outputs:
  - `LevelValidator`
  - validator tests
- Work:
  - id 중복, 빈 참조, capacity, move limit, order feasibility를 검사한다.
  - 완전 solver는 만들지 않는다.
- Verification:
  - invalid level tests
  - all 10 prototype levels validation test
- Done criteria:
  - 명백히 잘못된 level data를 PR 전에 잡을 수 있다.

### DL-010 - Rules Presenter And Game Controller

- Outputs:
  - `GameController`
  - `RulesPresenter`
  - level loading flow
- Work:
  - level asset을 runtime `LevelState`로 변환한다.
  - action result를 UI에 전달한다.
  - clear/fail/restart/next flow를 연결한다.
- Verification:
  - Play Mode smoke test for loading first level
- Done criteria:
  - scene에서 level 1을 표시할 수 있다.

### DL-011 - Basic Playable UI

- Outputs:
  - `MainGame.unity`
  - order row
  - queue, machine, basket, action bar UI
- Work:
  - placeholder UI로 모든 runtime location을 표시한다.
  - dream attributes를 아이콘/색/짧은 label로 표시한다.
- Verification:
  - manual Game view check
  - Play Mode smoke test
- Done criteria:
  - level state와 UI 표시가 일치한다.

### DL-012 - Tap Input And Action Preview

- Outputs:
  - `DreamSelectionController`
  - destination highlight
  - transformation preview
- Work:
  - 탭 선택 -> 탭 목적지 입력을 구현한다.
  - 가능한 목적지를 강조한다.
  - Washer/Dryer에 넣기 전 변환 결과를 표시한다.
- Verification:
  - Play Mode action smoke test
- Done criteria:
  - 마우스/touch tap으로 level 1-3을 플레이할 수 있다.

### DL-013 - Undo, Restart, Result Flow UI

- Outputs:
  - undo button
  - restart button
  - result panel
  - failure toast
- Work:
  - 무제한 undo를 버튼에 연결한다.
  - restart는 scene reload 없이 현재 level state를 초기화한다.
  - clear 후 next level로 이동한다.
- Verification:
  - Play Mode clear/restart/undo smoke tests
- Done criteria:
  - level 1-10을 scene 안에서 이어서 플레이할 수 있다.

### DL-014 - Minimal Feedback

- Outputs:
  - selection feedback
  - wash/dry/submit feedback
  - invalid action feedback
- Work:
  - 고급 아트 없이 짧은 scale/color/tween feedback을 만든다.
  - animation은 퍼즐 흐름을 느리게 만들지 않는다.
- Verification:
  - manual Game view check
- Done criteria:
  - 변환과 제출 결과가 즉시 읽힌다.

### DL-015 - Test And Run Wrappers

- Outputs:
  - `scripts/run-tests.ps1`
  - `test.cmd`
  - `scripts/run-emulator.ps1`
  - `run.cmd`
- Work:
  - Test Runner XML이 없거나 failed면 실패하도록 만든다.
  - emulator run wrapper는 PocketDodger 패턴을 따른다.
- Verification:
  - `.\DreamLaundromat\test`
  - `.\DreamLaundromat\run -BuildOnly` 또는 동등 옵션
- Done criteria:
  - 반복 가능한 CLI 검증 진입점이 있다.

### DL-016 - Android Build And Smoke

- Outputs:
  - debug APK local build output
  - Android build method
  - smoke log notes
- Work:
  - Android debug build를 만든다.
  - 가능하면 emulator에 install/run 한다.
  - critical crash log를 확인한다.
- Verification:
  - Unity Android build
  - `adb install -r`
  - `adb shell monkey -p <package> 1`
- Done criteria:
  - Android에서 앱이 실행되고 첫 화면이 뜬다.

### DL-017 - Minimal UI Icon Asset Pass

- Outputs:
  - `Assets/_Project/Art/UI/` 커스텀 PNG sprite
  - `UiIconCatalog` asset
  - 꿈/주문/기계/보관 버튼의 icon+text UI
- Work:
  - Unity Editor setup에서 아이콘 PNG와 `.meta`를 생성하고 Sprite로 import한다.
  - 런타임 UI는 `Resources`나 문자열 lookup 대신 scene에 연결된 `UiIconCatalog`를 사용한다.
  - 꿈 카드에는 `Clean/Nightmare`와 `Wet/Dry` 상태를 같이 보여준다.
  - 주문, 세탁기, 건조기, 보관 목적지에는 서로 다른 아이콘과 색상 규칙을 적용한다.
- Verification:
  - setup 후 모든 icon sprite와 `UiIconCatalog`가 존재하는지 검증한다.
  - Play Mode smoke test에서 주요 UI icon sprite가 scene에 표시되는지 확인한다.
  - Android build와 emulator smoke에서 첫 화면이 깨지지 않는지 확인한다.
- Done criteria:
  - 텍스트만 있을 때보다 `Submit`, `Washer`, `Dryer`, `Storage`, 꿈 상태가 더 빠르게 구분된다.

## PR Plan

사용자 요청에 따라 PR은 하나로 만든다.

PR title 예시:

```text
Build DreamLaundromat prototype
```

PR 하나에 포함:

- project shell
- rules prototype
- 10 level data
- playable scene/UI
- tests
- run/test wrappers
- Android smoke path

PR이 커지는 리스크는 아래 방식으로 줄인다.

- 구현 순서를 milestone별로 지킨다.
- PR description에 `Gate A`, `Gate B`, `Gate C` 검증 결과를 분리해서 적는다.
- scene/prefab 변경은 rules model이 안정된 뒤에만 추가한다.
- generated folders, build outputs, logs는 커밋하지 않는다.
- reviewer가 먼저 rules model과 tests를 볼 수 있도록 PR 요약을 구조화한다.

## Verification And Test Plan

최소 검증:

- `git status --short --branch`
- `git diff --check`
- `.meta` 누락 확인
- UI icon asset과 `UiIconCatalog` 생성 확인
- generated folder 포함 여부 확인
- Unity Android batchmode import
- Edit Mode tests
- Play Mode smoke tests
- `.\DreamLaundromat\test`

가능하면 수행:

- Android debug build
- Android emulator install/run smoke
- `adb logcat` critical error check
- 실제 기기 수동 플레이

수동 확인 필요:

- 주문/꿈 상태가 모바일 크기에서 읽히는가
- 손가락이 주요 UI를 가리지 않는가
- 세탁/건조/제출 feedback이 충분히 명확한가
- 10레벨 중 6개 이상에서 공간/순서 고민이 느껴지는가
- 실패 후 재시도 의사가 생기는가

## CLI And Manual Boundary

CLI에서 가능한 작업:

- Unity project 생성
- project settings baseline
- folder/meta 생성
- C# rules model 구현
- ScriptableObject level data 생성
- editor script로 scene/UI skeleton 생성
- Edit Mode/Play Mode tests 실행
- Android batchmode import/build
- emulator install/run smoke
- PR 생성

수동 확인이 필요한 작업:

- Unity Game view에서 UI 배치와 가독성 판단
- 실제 모바일 손 조작감 판단
- level 1-10의 재미와 난이도 체감
- Android physical device authorization
- PR merge

## Risks

- 한 PR에 project shell, rules, UI가 모두 들어가 diff가 커질 수 있다.
- Unity scene/prefab YAML 변경이 rules code review를 방해할 수 있다.
- 규칙이 UI에 먼저 묶이면 Edit Mode tests가 약해질 수 있다.
- order partial matching이 UI에서 헷갈릴 수 있다.
- `moisture=Wet`이 붙는 부작용이 유저에게 잘 보이지 않을 수 있다.
- capacity 제약이 너무 약하면 퍼즐이 단순 작업이 된다.
- capacity 제약이 너무 강하면 초반부터 답답해진다.
- 10개 레벨의 해답 가능성을 validator만으로 완전히 보장하지 못한다.
- Android build는 Unity/SDK/AVD 상태에 영향을 받는다.

대응:

- Gate A 전까지 scene/UI 변경을 최소화한다.
- rules model tests를 먼저 통과시킨다.
- level data validation과 수동 playthrough를 함께 사용한다.
- PR description에서 자동 검증과 수동 검증을 분리해서 보고한다.

## Self-Review

구현 플랜 작성 후 자체 점검한 결과는 아래와 같다.

- 기획 참조: concept, core rules, puzzle grammar, level progression, prototype success criteria를 연결했다.
- Scope/Non-Goals: 프로토타입 포함 범위와 제외 범위를 분리했다.
- 2 -> 3 구조: `Rules Prototype`을 먼저 만들고, 이후 `Playable Prototype`으로 연결하도록 milestone을 나눴다.
- PR 정책: 사용자의 요청에 따라 PR은 1개로 유지하되, `Gate A`, `Gate B`, `Gate C`로 내부 검증 단계를 분리했다.
- 퍼즐 리스크: pure rules model, level data shape, validation, Edit Mode tests를 scene/UI보다 먼저 배치했다.
- 검증: 자동 검증과 수동 확인이 필요한 항목을 분리했다.
- TODO 기준: 게임별 후속 아이디어는 이 문서에 남기고, 공통 워크플로에 영향을 주는 항목만 `docs/TODO.md`로 보내도록 정리했다.
- UI icon pass: 외부/유료 에셋 도입 없이 재생성 가능한 로컬 PNG sprite와 `UiIconCatalog`로 범위를 제한했다.
- UI icon pass verification: asset catalog 완성 여부, Play Mode icon 표시 여부, Android build/smoke 확인을 검증 항목에 포함했다.

수정 반영:

- 완성형 Unity UI부터 만들지 않고 `Rules Prototype` gate를 먼저 통과하도록 작업 순서를 조정했다.
- 한 PR로 진행하는 리스크를 `PR Plan`과 `Risks`에 명시했다.
- 사용자 결정이 필요한 항목을 별도 섹션으로 분리했다.

## User Decisions

결정 상태: 2026-06-15 사용자 확인 완료.

1. PR 1개 유지 여부
   - 결정: 1개 유지
   - 이유: 사용자의 현재 선호와 맞고, 프로토타입 전체 흐름을 한 번에 리뷰할 수 있다.
   - 영향: diff가 커지므로 PR description에서 `Gate A/B/C` 검증 결과를 분리해야 한다.

2. Undo 정책
   - 결정: 무제한 Undo
   - 이유: 퍼즐 프로토타입에서는 실험과 학습을 막지 않는 것이 중요하다.
   - 영향: 출시형 수익화/힌트 정책은 나중에 다시 판단한다.

3. 첫 레벨 데이터 형식
   - 결정: Unity `ScriptableObject`
   - 이유: Unity editor와 inspector에서 빠르게 조정하기 쉽다.
   - 영향: 나중에 JSON/export가 필요할 수 있으므로 필드는 단순하게 유지한다.

4. Android smoke test 범위
   - 결정: emulator smoke까지 시도하고, 실제 기기는 수동 확인으로 남긴다.
   - 이유: CLI에서 emulator까지는 처리 가능하지만 실제 기기 연결/승인은 사용자 조작이 필요할 수 있다.
   - 영향: 실제 손 조작감은 PR merge 전 사용자가 확인해야 한다.

5. 프로토타입 아트 에셋 정책
   - 결정: 이번 단계에서는 외부 무료/유료 에셋을 가져오지 않고, 게임 로컬 커스텀 UI 아이콘을 직접 생성한다.
   - 이유: 라이선스 리스크 없이 `Submit`, `Washer`, `Dryer`, `Storage`, 꿈 상태의 가독성만 빠르게 검증하기 위해서다.
   - 영향: 출시형 비주얼 품질이나 테마 아트는 이후 별도 아트 패스로 다시 판단한다.

## Deferred Or Out Of Scope

게임별 후속 아이디어:

- DyeVat과 color rules
- Folder와 `capacityCost=2/1`
- Mender와 damage rules
- emotion rules
- daily puzzle
- cosmetic collection
- advanced art/audio
- full solver
- level editor
- localization
- monetization/liveops

이 항목들은 `docs/TODO.md`에 넣지 않는다. 공통 워크플로, 저장소 정책, 자동 검증 인프라처럼 여러 게임에 영향을 주는 항목만 `docs/TODO.md`로 보낸다.

## First Implementation Step

다음 구현 시작 단계:

1. 브랜치 생성: `game/dream-laundromat-prototype`
2. Unity project shell 생성: `DreamLaundromat/`
3. Android/Portrait/project layout baseline 설정
4. `Rules/` pure model부터 구현
5. `DL-002`부터 Edit Mode tests를 붙여 Gate A를 먼저 통과시킨다.
