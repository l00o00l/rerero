# Action Dock Readability Plan

## Summary

`DreamLaundromat`의 gameplay 화면은 `Gameplay Layout V2` 이후 큰 영역 배치는 나아졌지만,
하단 action dock과 `Tools/Faults` strip에는 아직 긴 설명형 텍스트가 남아 있다.
이번 pass는 puzzle rule, level data, art asset을 바꾸지 않고 모바일 세로 화면에서
조작 버튼을 더 빠르게 읽히는 짧은 게임 UI 표현으로 정리한다.

## Planning References

- [Gameplay Layout V2 Plan](GAMEPLAY_LAYOUT_V2_PLAN.md)
- [Alpha Visual Issue Inventory](ALPHA_VISUAL_ISSUE_INVENTORY.md)
- [Release UI Design Plan](RELEASE_UI_DESIGN_PLAN.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)

## Prototype Goal

기본 gameplay 화면에서 플레이어가 `Dreams`, `Requests`, operation, submit/store/recall,
tool/fault 상태를 긴 문장형 버튼 없이 즉시 구분할 수 있는지 확인한다.

검증 기준:

- `Submit Order`, `Locked Slot 0`처럼 모바일 버튼에 긴 내부 표현이 노출되지 않는다.
- modifier는 `Tool/Fault` 구분, 대상, 남은 횟수를 짧은 칩 형태의 텍스트로 표현한다.
- 기존 PlayMode/EditMode 테스트와 release validation이 깨지지 않는다.

## Scope

- `Submit`, `Store`, `Recall` 버튼 라벨 축약
- `Preview Swap`, `Dream Refresh`, `Locked Slot`, `Pinned Order`, `Soft Block`의 화면용 짧은 라벨 정의
- modifier target을 1-based display slot으로 표시
- PlayMode/EditMode 테스트의 UI 계약 갱신
- 대표 Android screenshot 재생성 및 수동 시각 점검

## Non-Goals

- icon-only UI 전환
- 신규 art asset 생성
- modifier rule, solver, level pack 변경
- UI Toolkit 또는 prefab 전환
- final store screenshot 제작

## Key Decisions

- 화면 라벨은 규칙 데이터의 `DisplayName`을 그대로 쓰지 않고 release UI용 compact label을 별도로 만든다.
- slot 표기는 기존 UI와 맞춰 `D1`, `O1`, `S1`처럼 1-based로 통일한다.
- item/fault의 남은 횟수는 `x1`처럼 짧게 표시한다.
- operation 버튼의 `Wash`, `Soothe`, `Clarify`, `Settle`은 한 단어이고 core verb이므로 이번 pass에서는 유지한다.
- `Submit Order`는 command clarity를 유지하기 위해 `Submit`으로 줄인다.

## Target Platforms

Primary:

- Android portrait
- one-hand touch
- Unity Android build target

Secondary:

- Unity Editor PlayMode
- Windows batchmode validation

Manual checks:

- 작은 화면에서 action dock의 버튼 텍스트가 겹치거나 잘리지 않는지 확인한다.
- tool/fault label이 짧아져도 의미를 추측할 수 있는지 확인한다.
- store/recall 버튼이 선택 상황에서만 나타나는 현재 조건부 노출이 유지되는지 확인한다.

## Architecture

변경 대상:

- `ReleaseGameplayCardRenderer`
  - modifier 표시 문자열을 compact label helper로 분리한다.
- `ReleaseGameController`
  - submit/store/recall 버튼 라벨을 짧은 command label로 바꾼다.
- `ReleaseGameplaySlicePlayModeTests`
  - 화면에 노출되는 UI 문자열 계약을 갱신한다.

변경하지 않는 대상:

- `DynamicLab` rules/model
- solver
- level data
- save/progress
- Android scripts

## Data Model

새 gameplay data model은 추가하지 않는다.

이번 pass의 핵심은 동일한 `DynamicModifierDefinition`과 `DynamicModifierState`를 화면용으로
해석하는 presentation helper다. 내부 `DisplayName`은 validation/debug 용도로 유지하고,
release gameplay UI에서는 compact mapping을 사용한다.

## Scene And UI Plan

Action dock:

- `Submit Order` -> `Submit`
- `Store 1` -> `Store S1`
- `Recall 1` -> `Recall D1`

Modifier strip:

- `Tool / Preview Swap / 1` -> `Tool / Swap / x1`
- `Tool / Dream Refresh / Pick dream` -> `Tool / Refresh / Pick D`
- `Fault / Locked Slot 0 / 1` -> `Fault / Lock D1 / x1`
- `Fault / Pinned Order 1 / 1` -> `Fault / Pin O2 / x1`
- `Fault / Wash Soft Block / 1` -> `Fault / Jam Wash / x1`

## Milestones

### M1 - Compact Label Contract

- 화면에 노출할 짧은 라벨 규칙을 문서화한다.
- 기존 tests가 긴 label에 의존하는 지점을 확인한다.

### M2 - Runtime Label Update

- modifier compact label helper를 구현한다.
- submit/store/recall button label을 줄인다.

### M3 - Verification

- PlayMode/EditMode tests를 갱신하고 실행한다.
- release validation과 QA balance를 실행한다.
- representative screenshot을 다시 생성해 시각적으로 확인한다.

## Task Breakdown

### ADR-001 - Plan Lock

- Outputs:
  - `ACTION_DOCK_READABILITY_PLAN.md`
- Work:
  - action/modifier label 축약 기준을 확정한다.
  - verification과 manual gate를 명시한다.
- Verification:
  - 문서 self-review
- Done criteria:
  - 구현 중 다시 라벨 정책을 해석하지 않아도 된다.

### ADR-002 - Compact Modifier Labels

- Outputs:
  - `ReleaseGameplayCardRenderer` compact modifier label helper
- Work:
  - modifier effect별 display label을 정의한다.
  - target slot을 `D1/O1/S1` 1-based로 표시한다.
  - remaining charges를 `xN`으로 표시한다.
- Verification:
  - EditMode 또는 PlayMode UI 문자열 검사
- Done criteria:
  - 내부 `DisplayName`의 긴 문자열과 0-based target이 gameplay 화면에 그대로 노출되지 않는다.

### ADR-003 - Compact Action Labels

- Outputs:
  - 짧아진 submit/store/recall button label
- Work:
  - `Submit Order`를 `Submit`으로 줄인다.
  - store/recall 대상은 `S1`, `D1` 형식으로 표시한다.
- Verification:
  - PlayMode UI presence/absence tests
- Done criteria:
  - 선택 전 불필요한 store/recall 버튼은 여전히 숨겨지고, 선택 후 버튼 label은 짧다.

### ADR-004 - Verification And Visual Check

- Outputs:
  - updated tests
  - updated screenshot batch
- Work:
  - PlayMode/EditMode tests를 실행한다.
  - release/QA validation을 실행한다.
  - Android representative screenshots를 다시 생성한다.
- Verification:
  - `git diff --check`
  - `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
  - `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
  - `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
  - `.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`
  - `.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900`
- Done criteria:
  - 자동 검증이 통과하고, screenshot에서 label 밀도가 이전보다 낮아진다.

## PR Plan

현재 Alpha readiness branch의 UI polish pass에 포함한다.

이유:

- 이번 변경은 `Gameplay Layout V2`에서 남긴 action dock text density 문제의 직접 후속 작업이다.
- 별도 PR로 분리하면 같은 화면의 visual readiness 근거가 흩어진다.

## Verification And Test Plan

필수 자동 검증:

```powershell
git diff --check
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
```

Screenshot 검증:

```powershell
.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900
```

Manual visual gate:

- Level 01에서 기본 화면이 text-heavy하지 않은지 확인한다.
- Level 10에서 `Fault` label이 `Lock D1`처럼 짧고 의미가 유지되는지 확인한다.
- Level 30에서 action dock의 버튼들이 작게 겹치지 않는지 확인한다.

## CLI And Manual Boundary

CLI로 가능한 것:

- code/test 변경
- Unity PlayMode/EditMode tests
- release validation
- QA balance validation
- Android screenshot batch 생성

사람 판단이 필요한 것:

- 실제 손가락 조작에서 label이 충분히 빠르게 읽히는지
- icon과 짧은 label이 처음 보는 플레이어에게 충분한지
- final art direction과 맞는지

## Risks

- label을 너무 줄이면 신규 플레이어가 의미를 추측하기 어려울 수 있다.
- `Store S1`, `Recall D1`은 현재 게임 문법을 모르면 약간 추상적일 수 있다.
- modifier label helper가 effect mapping을 중복으로 가지므로 새 modifier가 추가될 때 같이 갱신해야 한다.

## Deferred Or Out Of Scope

game-local backlog:

- icon-only action dock
- long-press tooltip 또는 glossary
- action dock animation
- tutorial step에서 `D/O/S` 표기 설명 보강

`docs/TODO.md` 대상은 아니다. 이번 내용은 특정 게임의 UI polish backlog이며 공유 환경/인프라 문제가 아니다.

## First Implementation Step

`ReleaseGameplayCardRenderer.BuildModifierLabel`에 effect별 compact label helper를 추가하고,
그 다음 `ReleaseGameController.RenderActions`의 submit/store/recall 라벨을 줄인다.

## Current Execution Status

2026-06-18 구현 pass를 완료했다.

반영한 내용:

- `Submit Order`를 `Submit`으로 줄였다.
- 선택 후 나타나는 storage action은 `Store S1`, recall action은 `Recall D1` 형식으로
  표시하도록 바꿨다.
- operation button은 전체 단어 대신 `W`, `So`, `Cl`, `Se` marker와 icon을 함께 쓰도록
  바꿨다.
- release gameplay modifier label은 내부 `DisplayName`을 그대로 쓰지 않고 effect별 compact
  mapping을 사용한다.
  - `Preview Swap` -> `Swap`
  - `Dream Refresh` -> `Refresh` 또는 `Refresh D1`
  - `Locked Slot 0` -> `Lock D1`
  - `Pinned Order 1` -> `Pin O2`
  - `Wash Soft Block` -> `Jam Wash`
- modifier 남은 횟수는 `x1` 형식으로 표시한다.
- PlayMode test에서 `Preview Swap`, `Locked Slot 0`, `Submit Order`가 gameplay 화면에
  남지 않는지 확인한다.
- EditMode test에서 compact modifier label mapping을 직접 검증한다.

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

.\DreamLaundromat\level-screenshots.cmd -Build -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 1200 -BuildTimeoutSeconds 1200
# Passed. 새 APK 빌드/설치 후 representative screenshots를 갱신했다.
```

시각 확인:

- Level 01에서 submit button이 `Submit`으로 줄어든 것을 확인했다.
- Level 10에서 fault strip이 `Fault / Lock D1 / x1`로 표시되는 것을 확인했다.
- Level 30에서도 action dock label이 marker 중심으로 더 짧아진 것을 확인했다.

남은 문제:

- operation label은 marker 중심으로 줄었지만, 초반 tutorial에서 `W/So/Cl/Se`의 의미를
  충분히 익히는지 확인해야 한다.
- header navigation은 여전히 text button이다. 출시 UI에서는 icon/menu 형태가 더 자연스럽다.
- `D/O/S` 축약 표기는 반복 플레이에는 효율적이지만, 초반 tutorial에서 의미를 충분히
  알려줘야 한다.

## Self-Review

검토 결과:

- Scope와 Non-Goals가 분리되어 있다.
- rules/model/level data를 건드리지 않는다는 경계가 명확하다.
- verification과 manual visual gate가 분리되어 있다.
- Android portrait와 one-hand touch 전제를 명시했다.
- unresolved user decision은 없다. 사용자가 이미 UI/디자인 밀도 개선을 요청했고, 이번 pass는 좁은 후속 구현이다.
