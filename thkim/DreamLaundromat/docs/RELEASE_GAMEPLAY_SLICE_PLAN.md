# Release Gameplay Slice Plan

## Summary

이 문서는 `DreamLaundromat`의 `FULL_GAME_ROADMAP.md` Phase 1-5를 하나의
구현 흐름으로 묶는 실행 계획이다.

목표는 `DynamicLabDebugGame` 수준의 실험 화면을 넘어서, 실제 플레이어가 세로
모바일 화면에서 레벨을 시작하고, 꿈을 읽고, operation과 storage를 사용하고, 주문을
완료하고, 실패하거나 클리어한 뒤 재시도 또는 다음 레벨로 넘어갈 수 있는
`Release Gameplay Slice`를 만드는 것이다.

이번 slice는 최종 출시 버전이 아니다. 하지만 제품형 게임 구조, 레벨 데이터 경로,
검증 스크립트, 초반 레벨 팩이 서로 연결되어야 한다.

## Planning References

- [Full Game Roadmap](FULL_GAME_ROADMAP.md)
- [DreamLaundromat implementation plan](PLAN.md)
- [Dynamic Puzzle Lab implementation plan](DYNAMIC_PUZZLE_LAB_PLAN.md)
- [Modifier Engine implementation plan](MODIFIER_ENGINE_PLAN.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)
- [Unity project conventions](../../docs/CONVENTIONS.md)
- [Mobile Android guidance](../../docs/MOBILE_ANDROID.md)
- [Dream Laundromat concept](../../../concepts/puzzle/dream-laundromat.md)
- [Dream Laundromat planning index](../../../concepts/puzzle/dream-laundromat-planning/README.md)

## Prototype Goal

핵심 가설:

`DreamLaundromat`은 랜덤 성공/실패가 없어도, preview로 공개된 꿈과 주문을 읽고
제한된 공간에서 어떤 꿈을 어떤 주문에 배정할지 판단하게 만들면 퍼즐로서 재미를
줄 수 있다.

이번 slice가 증명해야 하는 것:

- `DynamicRulesEngine` 기반 규칙이 실제 유저용 flow로 플레이 가능하다.
- 4개 상태 축, 주문 조건, preview, storage, item/obstacle 정보를 모바일 화면에서
  읽을 수 있다.
- fixed level pack을 만들고 전체 pack을 자동 검증할 수 있다.
- 초반 10개 레벨이 규칙을 조금씩 가르치며, 단순 반복 조작만 요구하지 않는다.

성공 판정은 자동 테스트만으로 끝나지 않는다. 자동 검증은 solvable, state transition,
scene flow, level pack regression을 확인하고, 실제 재미와 조작감은 manual gate로
분리한다.

## Scope

포함 범위:

- Phase 1: 이 구현 계획 작성과 자체 검토
- Phase 2: 제품형 gameplay scene과 game flow 구현
- Phase 3: 한손 터치 기준 core mobile interaction 구현
- Phase 4: fixed level data path와 level pack validation V1 구현
- Phase 5: 10개 수준의 `Level Pack V1` 작성과 검증

구체 산출물:

- `DreamLaundromat/docs/RELEASE_GAMEPLAY_SLICE_PLAN.md`
- `ReleaseGameplaySlice` scene
- release slice 전용 runtime controller와 presenter
- tap 기반 dream/order/action/storage/item interaction
- clear, fail, restart, next level flow
- fixed dynamic level pack provider
- level pack validation command
- EditMode/PlayMode 테스트
- Android target batchmode check
- manual QA checklist

## Non-Goals

이번 slice에서 제외한다.

- 전체 world map 또는 level select map
- local save/progression persistence
- monetization, ads, IAP
- analytics, crash reporting SDK
- Google Play store 등록, signing secret, release AAB
- 최종 art/audio/haptic polish
- drag-and-drop interaction
- liveops, daily puzzle, event content
- full hint system
- cloud save
- 대량 30개 이상 레벨 제작

## Key Decisions

- 작업 브랜치: `game/dream-laundromat-release-slice`
- PR 정책: Phase 1-5 gate가 통과하거나 사용자가 명시적으로 요청하기 전까지 PR을
  만들지 않는다.
- merge 정책: Codex는 PR merge, `git merge`, protected branch push를 하지 않는다.
- scene 이름: `ReleaseGameplaySlice.unity`
- scene 위치: `DreamLaundromat/Assets/_Project/Scenes/`
- runtime code 위치: `DreamLaundromat/Assets/_Project/Scripts/Gameplay/ReleaseSlice/`
- debug scene: `DynamicLabDebug.unity`는 유지하되 제품 flow의 기준으로 쓰지 않는다.
- rules source of truth: `DynamicRulesEngine`
- old static prototype: `MainGame.unity`, `Rules/`, `Levels/`는 참고 자산으로 유지하되
  release slice의 규칙 기준으로 확장하지 않는다.
- level data V1: C# fixed pack provider로 시작한다. ScriptableObject/JSON export는
  level production이 안정된 뒤 확장한다.
- level count: 처음 gate는 10개 fixed levels로 잡는다.
- input model: tap/select 기반 interaction을 기본값으로 한다.
- undo policy: 이번 slice에서는 restart를 필수로 하고, undo는 후속 phase로 둔다.
- tutorial policy: guided lock보다 level-local short guidance와 disabled reason을 먼저
  구현한다.
- item/obstacle policy: 기존 `PreviewSwap`, `LockedActiveDreamSlot`을 표시하고 검증한다.
  새 modifier 추가는 Phase 6 이후로 둔다.

## Target Platforms

Primary:

- Android mobile
- Portrait orientation
- One-hand touch
- Unity build target: Android

Secondary:

- Unity Editor Play Mode
- Windows 개발 머신에서 batchmode 검증

Run and test scripts:

- `DreamLaundromat/run.cmd`: 사람이 에디터나 에뮬레이터에서 실행할 때 사용하는 기본
  wrapper
- `DreamLaundromat/test.cmd`: Unity Test Runner wrapper
- 새 level pack 검증 wrapper는 `DreamLaundromat/release-slice.cmd`로 둔다.

Manual platform gate:

- Android emulator 또는 실제 기기에서 터치 흐름을 사람이 확인해야 한다.
- 안전 영역, 텍스트 판독성, 버튼 크기, 손가락 가림은 CLI만으로 완료 판정하지 않는다.

## Architecture

기본 방향:

- pure rules/model은 `DynamicLab`에 남긴다.
- release slice는 `DynamicLab`을 scene/UI에 연결하는 얇은 product layer를 가진다.
- scene-owned object는 serialized reference로 연결하고, broad scene lookup은 피한다.
- level pack과 validator는 Unity scene에 의존하지 않게 만든다.

예상 namespace와 폴더:

- `DreamLaundromat.DynamicLab.*`: 기존 pure rules, model, generation, solving
- `DreamLaundromat.Gameplay.ReleaseSlice`: 제품형 gameplay session과 UI bridge
- `DreamLaundromat.Editor.ReleaseSlice`: scene setup, batch validation entry point
- `DreamLaundromat.Tests.*`: EditMode/PlayMode 검증

주요 구성:

- `ReleaseLevelDefinition`
  - 화면에 노출할 level id, title, guidance, intent, risk note를 보관한다.
  - 내부 gameplay는 `DynamicRoundDefinition`을 사용한다.
- `ReleaseLevelPack`
  - fixed levels를 순서대로 제공한다.
  - level id 중복과 빈 pack을 방지한다.
- `ReleaseLevelValidator`
  - hard validation, solver, replay, design warning을 한 번에 실행한다.
- `ReleaseGameSession`
  - 현재 level index, round state, clear/fail 상태, last action result를 관리한다.
  - MonoBehaviour가 아닌 testable class로 둔다.
- `ReleaseGameController`
  - scene lifecycle과 presenter refresh를 담당한다.
- `ReleaseGamePresenter`
  - Unity UI update, card rendering, button state, guidance text를 담당한다.

## Data Model

Level data V1:

- `ReleaseLevelDefinition`
  - `LevelId`
  - `DisplayName`
  - `Guidance`
  - `DesignIntent`
  - `PlayerQuestion`
  - `RiskNote`
  - `DynamicRoundDefinition`

Fixed pack 생성:

- `DynamicSampleRecipes`와 `DynamicSampleRounds`에서 검증된 구조를 재사용한다.
- seed와 recipe 이름을 코드에 명시해서 재현 가능하게 한다.
- 사람이 승격한 level이라는 의미를 문서 metadata에 남긴다.

Validation output:

- hard validation result
- solver result
- replay verification result
- min moves
- explored state count
- design warnings

이번 단계에서 하지 않는 것:

- JSON export/import
- ScriptableObject authoring UI
- remote content update
- live balancing table

## Scene And UI Plan

Scene:

- `ReleaseGameplaySlice.unity`
- `Main Camera`
- `Canvas`
- `SafeArea`
- `Header`
- `ActiveDreamPanel`
- `ActiveOrderPanel`
- `OperationPanel`
- `PreviewPanel`
- `StoragePanel`
- `ItemPanel`
- `FooterPanel`
- `ResultOverlay`

UI 흐름:

1. 첫 level을 로드한다.
2. active dream, active order, preview, storage, item charge를 표시한다.
3. player가 dream slot 또는 storage slot을 선택한다.
4. 선택 상태에 따라 가능한 operation, submit, store, recall, item action을 표시한다.
5. 불가능한 action은 비활성화하고 reason을 짧게 보여 준다.
6. action 후 state를 refresh한다.
7. clear 시 next level button을 보여 준다.
8. fail 시 failure reason과 restart button을 보여 준다.

UI 원칙:

- debug 용어보다 player-facing label을 우선한다.
- 상태 축은 text, icon-like label, color를 함께 사용한다.
- operation 결과 preview는 최소한 selected dream에 대해 보여 준다.
- 버튼은 화면 하단 접근성을 우선한다.
- 텍스트 크기는 모바일 판독성을 해치지 않는 선에서 고정 크기와 layout constraint를
  사용한다.

## Milestones

### M1 - Plan Lock

- 이 문서를 작성한다.
- Phase 1-5 범위와 default decisions를 고정한다.
- 자동 검증과 manual gate를 분리한다.

### M2 - Level Data Pipeline V1

- fixed release level pack을 만든다.
- pack validation code와 batch command를 만든다.
- 30개 level이 hard validation, solver, replay를 통과하게 한다.

### M3 - Release Gameplay Runtime

- `ReleaseGameSession`을 구현한다.
- level start, action dispatch, clear/fail, restart, next level을 pure class로 검증한다.

### M4 - Product Gameplay Scene

- `ReleaseGameplaySlice.unity`를 만들고 controller/presenter를 배치한다.
- debug scene 없이 첫 level을 플레이할 수 있게 한다.

### M5 - Core Mobile Interaction

- tap/select 기반 dream, order, operation, storage, item controls를 구현한다.
- invalid feedback과 simple guidance를 표시한다.

### M6 - Verification Gate

- EditMode/PlayMode 테스트를 통과시킨다.
- level pack validation command를 통과시킨다.
- Android target batchmode check를 통과시킨다.
- manual QA 항목을 문서화한다.

## Task Breakdown

### RGS-001 - Branch And Baseline Check

- Outputs:
  - 현재 브랜치와 worktree 상태 확인
- Work:
  - `git status --short --branch -uall` 실행
  - 로드맵과 기존 `DynamicLab` 구조 확인
- Verification:
  - branch가 `game/dream-laundromat-release-slice`인지 확인
- Done criteria:
  - 구현이 시작될 기준 상태가 명확하다.

### RGS-002 - Release Slice Plan

- Outputs:
  - `DreamLaundromat/docs/RELEASE_GAMEPLAY_SLICE_PLAN.md`
- Work:
  - Phase 1-5를 하나의 구현 계획으로 정리
  - 자체 검토와 Open Decisions 기록
- Verification:
  - `docs/IMPLEMENTATION_PLANNING.md` 필수 섹션을 모두 포함
  - verification/test plan 포함
- Done criteria:
  - 사용자가 자리를 비워도 CLI 가능한 구현을 계속 진행할 수 있다.

### RGS-003 - Release Level Data Types

- Outputs:
  - `ReleaseLevelDefinition`
  - `ReleaseLevelPack`
- Work:
  - fixed pack에 필요한 metadata와 `DynamicRoundDefinition` 연결
  - level id 중복 방지
- Verification:
  - EditMode tests
- Done criteria:
  - scene 없이 release levels를 순서대로 조회할 수 있다.

### RGS-004 - Release Level Pack V1

- Outputs:
  - 10개 fixed release levels
- Work:
  - sample recipe와 seed를 사용해 초반 learning arc 구성
  - 각 level에 intent, player question, risk note 작성
- Verification:
  - 모든 level hard validation 통과
  - solver 성공
- Done criteria:
  - slice가 runtime random generator 없이 고정 레벨로 실행된다.

### RGS-005 - Level Pack Validation Command

- Outputs:
  - `release-slice.cmd`
  - `scripts/run-release-slice.ps1`
  - editor batch validation entry point
- Work:
  - fixed pack 전체 검증
  - log/report 실패 시 non-zero exit
- Verification:
  - wrapper 실행 성공
- Done criteria:
  - pack regression을 명령 하나로 확인할 수 있다.

### RGS-006 - Release Game Session

- Outputs:
  - `ReleaseGameSession`
- Work:
  - level load
  - action dispatch
  - clear/fail/restart/next
  - last message/failure reason
- Verification:
  - EditMode tests
- Done criteria:
  - Unity scene 없이 game flow를 검증할 수 있다.

### RGS-007 - Product Scene Setup

- Outputs:
  - `ReleaseGameplaySlice.unity`
  - scene setup editor script
- Work:
  - Canvas, panels, buttons, text, controller 배치
  - build settings에 scene 추가
- Verification:
  - Unity batchmode import
  - scene asset과 `.meta` 존재 확인
- Done criteria:
  - Unity가 scene을 열고 compile/import할 수 있다.

### RGS-008 - Presenter And Card Rendering

- Outputs:
  - `ReleaseGamePresenter`
  - dream/order/storage/preview/item UI rendering
- Work:
  - selected dream과 action availability 표시
  - state axis label 구성
- Verification:
  - PlayMode tests for required UI elements
- Done criteria:
  - player가 현재 puzzle state를 debug console 없이 읽을 수 있다.

### RGS-009 - Core Interaction Actions

- Outputs:
  - tap/select action flow
- Work:
  - operation buttons
  - submit/store/recall
  - item use
  - invalid action feedback
- Verification:
  - PlayMode tests for dispatch and state refresh
- Done criteria:
  - 한 level을 scene에서 시작부터 clear/fail까지 조작할 수 있다.

### RGS-010 - Result Flow

- Outputs:
  - clear/fail overlay
  - restart/next buttons
- Work:
  - result state 표시
  - final level handling
- Verification:
  - PlayMode tests
- Done criteria:
  - 빠른 재시도와 다음 레벨 진입이 가능하다.

### RGS-011 - Mobile Layout Pass

- Outputs:
  - portrait layout constraints
  - safe area usage
- Work:
  - 360x800과 1080x1920 기준 정보 밀도 조정
  - touch target size 점검
- Verification:
  - CLI로 가능한 scene import와 PlayMode checks
  - manual screenshot/game view gate 기록
- Done criteria:
  - 모바일에서 검토할 수 있는 상태의 화면 구조가 있다.

### RGS-012 - Automated Tests

- Outputs:
  - EditMode tests
  - PlayMode tests
- Work:
  - level pack validation tests
  - session transition tests
  - scene smoke tests
- Verification:
  - `DreamLaundromat/test.cmd -Mode EditMode`
  - `DreamLaundromat/test.cmd -Mode PlayMode`
- Done criteria:
  - 핵심 flow regression이 자동으로 잡힌다.

### RGS-013 - Android Target Check

- Outputs:
  - Android batchmode check result
- Work:
  - Android target import/build check 실행
  - log 검토
- Verification:
  - Unity Android batchmode command
- Done criteria:
  - Android target compile/import 문제가 없다.

### RGS-014 - Manual QA Checklist

- Outputs:
  - manual QA notes in this plan or PR notes
- Work:
  - 실제 기기/에뮬레이터에서 확인할 항목 분리
- Verification:
  - 자동으로 확인한 항목과 사람이 확인할 항목이 섞이지 않는다.
- Done criteria:
  - Codex가 할 수 없는 검증이 명확하다.

## PR Plan

사용자 지시에 따라 이번 목표는 구현 단위마다 PR을 쪼개지 않는다.

이번 브랜치의 PR 단위:

- 하나의 PR: Phase 1-5 `Release Gameplay Slice`

PR 생성 조건:

- 사용자가 명시적으로 PR 생성을 요청한다.
- 또는 Phase 1-5 gate가 자동 검증과 문서상 manual gate 분리까지 통과한다.

PR에 반드시 포함할 내용:

- `FULL_GAME_ROADMAP.md`
- `RELEASE_GAMEPLAY_SLICE_PLAN.md`
- release slice runtime, scene, scripts
- level pack validation
- tests
- 검증 결과와 manual gap

Codex 금지:

- `gh pr merge`
- `git merge`
- protected branch push
- GitHub merge API 호출

## Verification And Test Plan

구현 중 반복 확인:

```powershell
git status --short --branch -uall
git diff --check
```

Unity tests:

```powershell
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
```

Dynamic lab regression:

```powershell
.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900
```

Release slice validation:

```powershell
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
```

Android target check:

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"
& $Unity -batchmode -quit -projectPath .\DreamLaundromat -buildTarget Android -logFile .\DreamLaundromat\Logs\android-import.log
```

Asset/meta checks:

- 새 Unity asset에는 `.meta`가 있어야 한다.
- `Library/`, `Temp/`, `Obj/`, `Logs/`, build output은 commit하지 않는다.
- 큰 binary asset이 추가되면 LFS 필요 여부를 확인한다.

Manual checks:

- 360x800 수준 세로 화면에서 텍스트가 겹치지 않는지 확인
- 1080x1920 세로 화면에서 정보가 지나치게 퍼지지 않는지 확인
- 실제 터치로 dream 선택, operation, storage, submit, item use가 이해되는지 확인
- clear/fail/restart/next가 빠르게 이어지는지 확인
- 초반 10레벨이 지루한 단순 반복으로 느껴지지 않는지 확인
- Android emulator 또는 실제 기기에서 첫 화면과 기본 touch flow 확인

## CLI And Manual Boundary

Codex가 CLI에서 할 수 있는 것:

- 코드 구현
- Unity batchmode import/build check
- EditMode/PlayMode 테스트
- fixed level pack validation
- scene setup script 실행
- 로그 검토
- PR 생성과 리뷰

사람이 해야 하는 것:

- PR merge
- protected branch push 승인
- 실제 기기 조작감 평가
- 스토어 계정 작업
- signing secret/keystore 입력
- 유료 asset 구매와 라이선스 동의
- 최종 재미와 난이도 체감 판단

## Risks

### R1 - 4개 상태 축이 모바일에서 읽기 어려울 수 있음

대응:

- 상태 축 label을 간결하게 유지한다.
- 색만 쓰지 않고 text와 icon-like tag를 함께 쓴다.
- manual readability gate를 별도로 둔다.

### R2 - Release slice가 debug UI의 복잡도를 그대로 가져올 수 있음

대응:

- `DynamicLabDebugGame` 코드를 직접 확장하지 않는다.
- product layer를 새로 만들고 debug scene은 회귀 확인용으로 유지한다.

### R3 - Fixed level pack이 재미보다 solvable에만 치우칠 수 있음

대응:

- 각 level에 `DesignIntent`, `PlayerQuestion`, `RiskNote`를 둔다.
- solver 성공뿐 아니라 design warning을 report에 남긴다.

### R4 - Unity scene/YAML 변경이 review를 어렵게 만들 수 있음

대응:

- scene setup은 Editor script로 생성한다.
- scene 변경 후 batchmode import와 PlayMode 테스트를 실행한다.

### R5 - Android build check가 오래 걸리거나 환경 문제로 실패할 수 있음

대응:

- 먼저 EditMode/PlayMode와 import check를 통과시킨다.
- Android 실패가 환경 문제인지 compile 문제인지 log로 분리한다.

## Deferred Or Out Of Scope

game-local backlog:

- ScriptableObject 또는 JSON 기반 level authoring
- progression save
- level select map
- full tutorial lock system
- undo
- richer item/obstacle expansion
- final art/audio/haptics
- release AAB/signing/store flow

`docs/TODO.md`에는 이 항목들을 기록하지 않는다. 이 항목들은 단일 게임의 기능
백로그이며, 공통 환경이나 장기 생산성 deferred work가 아니다.

## Open Decisions

안전한 기본값으로 진행할 결정:

- level count는 10개로 시작한다.
- fixed level data는 C# provider로 시작한다.
- scene 이름은 `ReleaseGameplaySlice.unity`로 한다.
- interaction은 tap/select로 한다.
- undo는 이번 slice에서 제외하고 restart를 우선한다.

사용자 확인이 있으면 나중에 바꿀 수 있는 결정:

- slice level 수를 15개까지 늘릴지 여부
- ScriptableObject/JSON authoring을 Phase 4 안에서 앞당길지 여부
- drag interaction을 Phase 3 안에서 포함할지 여부
- 첫 art pass를 이번 PR에 조금 더 넣을지 여부

현재는 위 결정들이 구현을 막지 않으므로 질문하지 않고 기본값으로 진행한다.

## First Implementation Step

다음 작업은 `RGS-003 - Release Level Data Types`이다.

먼저 scene과 UI를 만들기 전에, fixed level pack과 validation path를 pure code로
구성한다. 이렇게 하면 제품형 scene이 붙기 전에 레벨 데이터와 규칙 검증이 안정되고,
PlayMode 테스트가 실패하더라도 core rules 문제와 UI 문제를 분리할 수 있다.

## Implementation Status

2026-06-18 기준 Phase 1-13의 CLI 구현과 자동 검증은 완료했다.

완료된 항목:

- `ReleaseGameplaySlice.unity` 생성
- `ReleaseGameController` 기반 제품형 gameplay scene 구현
- tap/select 기반 active dream, order, storage, operation, item interaction 구현
- drag/drop 기반 dream-to-order, dream-to-storage, storage-to-dream interaction 구현
- clear, fail, restart, next level flow 구현
- `ReleaseLevelPack` 30개 level 구성
- Phase 6 item/obstacle V1과 modifier impact validation 추가
- Phase 7 guided tutorial prompt와 early guided input lock 추가
- Phase 8 local progression/settings save 추가
- Phase 9-11 Visual/UX V1, card/action surface treatment, footer/navigation surface treatment 추가
- Phase 12-13 QA/balance report, Android screenshot smoke, level screenshot batch 추가
- `ReleaseLevelPackValidator`와 `release-slice.cmd` 추가
- release slice EditMode/PlayMode 테스트 추가
- Android debug build가 `ReleaseGameplaySlice.unity`를 첫 scene으로 빌드하도록 변경

최근 검증 결과:

```powershell
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
# Valid=True, Levels=30, Errors=0, Warnings=0

.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
# Total=85 Passed=85 Failed=0 Skipped=0

.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
# Total=20 Passed=20 Failed=0 Skipped=0

.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
# Passed. Report generated under DreamLaundromat/Logs.

.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
# Android debug build/install/launch completed with exit code 0

.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
# Android screenshot smoke passed

.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0 -TimeoutSeconds 300 -NoAutoStart
# Android level screenshot batch passed
```

남은 manual gate:

- Android emulator 또는 실제 기기에서 touch flow와 손가락 가림 직접 확인
- 360x800, 1080x1920 등 주요 세로 화면비에서 텍스트 겹침과 판독성 확인
- 30레벨을 사람이 직접 플레이하며 재미, 반복감, 막힘 이유를 기록
- item/obstacle 레벨에서 설명 없이도 효과가 이해되는지 확인

이 manual gate들은 CLI에서 완료 판정할 수 없는 항목이다. 다만 자동 검증 기준으로는
Phase 1-13 구현 산출물이 동작하고 빌드된다.

## Self-Review

검토 기준:

- `docs/IMPLEMENTATION_PLANNING.md`의 필수 섹션을 모두 포함했다.
- Phase 1-5가 모두 scope, milestone, task에 들어 있다.
- 자동 검증과 manual gate를 분리했다.
- PR을 하나로 유지하라는 사용자 지시를 `PR Plan`에 반영했다.
- debug scene을 제품 scene으로 그대로 확장하지 않는 결정을 명시했다.
- fixed level data V1의 기본값을 정해 구현이 멈추지 않게 했다.
- `docs/TODO.md`에 넣을 항목과 game-local backlog를 분리했다.

자체 수정한 점:

- 처음에는 scene/UI를 먼저 만들 수 있었지만, level data와 validation이 없으면
  Phase 4-5가 뒤로 밀릴 위험이 있어 `RGS-003`을 첫 구현 작업으로 조정했다.
- ScriptableObject level authoring을 바로 만들지 않고 C# fixed pack provider로 시작해
  scene YAML과 asset authoring 위험을 줄였다.
- Playable slice 판정을 자동 테스트만으로 끝내지 않고 manual readability와 touch feel
  gate를 별도로 두었다.

남은 위험:

- 실제 재미와 조작감은 사람이 플레이해야 판단할 수 있다.
- Android emulator/physical device smoke는 로컬 환경과 기기 연결 상태에 따라 수동
  gate가 될 수 있다.
