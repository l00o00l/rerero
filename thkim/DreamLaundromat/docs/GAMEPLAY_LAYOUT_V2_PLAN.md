# Gameplay Layout V2 Plan

## Summary

이 문서는 `DreamLaundromat` gameplay screen이 너무 많은 정보를 한 화면에 우겨 넣어
작고 읽기 어렵게 보이는 문제를 해결하기 위한 구현 계획이다.

직전 `AR-003` pass는 긴 문구를 줄였지만, 화면 구조는 여전히 `Dreams`, `Requests`,
`Workbench`, `Shelf`, `Tools/Faults`, operations, submit/store, footer navigation을 모두
상시 노출하는 방식이었다. 이번 pass는 텍스트 polish가 아니라 정보 우선순위와 노출
조건을 바꾸는 layout pass다.

## Planning References

- [Alpha Readiness Plan](ALPHA_READINESS_PLAN.md)
- [Alpha Visual Issue Inventory](ALPHA_VISUAL_ISSUE_INVENTORY.md)
- [Release UI Design Plan](RELEASE_UI_DESIGN_PLAN.md)
- [Direct Manipulation Game Feel V2 Plan](DIRECT_MANIPULATION_GAME_FEEL_V2_PLAN.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)

## Prototype Goal

핵심 가설:

기본 gameplay 화면에서 `Dreams`, `Requests`, 핵심 `Operation/Submit`만 크게 보이게
하고, `Workbench`, `Shelf`, `Tools/Faults`, navigation, status message를 조건부 또는
compact strip으로 줄이면 모바일 세로 화면에서 퍼즐의 핵심 상태를 더 크게 읽을 수 있다.

## Scope

포함 범위:

- gameplay screen layout 재배치
- header navigation compact화
- footer navigation 제거 또는 header로 이동
- `Workbench` 기본 숨김
- `Shelf`, `Tools/Faults`를 짧은 utility strip으로 축소
- dream/order/action card가 차지하는 공간 확대
- PlayMode UI 계약 갱신
- Android representative screenshot 확인

## Non-Goals

- final art pack 제작
- UI Toolkit 또는 prefab migration
- level data 수정
- rules/model 수정
- 새로운 item/obstacle 규칙 추가
- store screenshot 제작
- full animation timeline

## Key Decisions

- 기본 화면에 항상 보여줄 것:
  - level title/objective header
  - active dream cards
  - active request cards
  - operation buttons
  - submit/store row
- 조건부로 보여줄 것:
  - `Workbench`: 선택된 dream/order/storage가 있거나 submit 가능 상태가 있을 때만 표시한다.
  - `Tools/Faults`: level에 modifier가 있을 때만 짧은 strip으로 표시한다.
  - `Shelf`: storage slot이 있는 level에서만 짧은 strip으로 표시한다.
  - status message: 유효한 선택/행동 결과가 있을 때만 header 아래에 표시한다.
- navigation:
  - `Restart`, `Levels`, `Pause`, `Next`는 하단 footer를 차지하지 않고 header의 compact
    controls로 이동한다.
- 이번 pass는 “더 예쁜 최종 UI”가 아니라 “읽을 수 있는 플레이 화면”을 목표로 한다.

## Target Platforms

Primary:

- Android portrait
- one-hand touch
- Unity Android build target

Secondary:

- Unity Editor PlayMode
- Windows batchmode validation

Manual checks:

- 작은 세로 화면에서 dream/order 카드가 이전보다 커졌는지 확인
- footer가 사라진 뒤 navigation을 찾을 수 있는지 확인
- Workbench가 숨겨져도 첫 조작 이해가 과하게 나빠지지 않는지 확인

## Architecture

변경 대상:

- `ReleaseGameController`
  - gameplay screen layout 생성
  - section active/hidden 조건
  - header compact controls
- `ReleaseGameplayViewModel`
  - 기존 focus text를 유지하되 controller가 표시 조건을 판단한다.
- `ReleaseGameplaySlicePlayModeTests`
  - 상시 노출에서 조건부 노출로 바뀐 UI 계약을 반영한다.

변경하지 않는 것:

- `DynamicLab` rules/model
- solver
- level pack
- progress save
- Android scripts

## Data Model

새 gameplay data model은 추가하지 않는다.

이번 pass는 이미 존재하는 `ReleaseGameplayViewModel`과 selection state를 사용한다.
조건부 표시 판단은 우선 controller helper로 둔다. 이후 `AR-004`에서 controller
responsibility audit을 수행할 때 presenter/view model 책임으로 분리할지 판단한다.

## Scene And UI Plan

화면 구조:

1. Compact Header
   - level name/id
   - compact objective
   - `Restart`, `Levels`, `Pause`, `Next`
   - action/selection message는 있을 때만 표시
2. Main Board
   - active dreams
   - active requests
   - conditional workbench
   - optional shelf strip
   - optional tools/faults strip
3. Action Dock
   - 4개 operation
   - submit/store/recall action row

레이아웃 원칙:

- dream/request card를 가장 크게 둔다.
- section title은 가능한 제거하고, card visual과 icon으로 영역을 읽게 한다.
- utility strip은 게임판을 밀어내지 않도록 낮은 height로 제한한다.
- disabled action도 touch target은 유지하되, 화면에서 과하게 강조하지 않는다.

## Milestones

### M1 - Layout Contract

- 상시 노출과 조건부 노출 기준을 문서화한다.
- 기존 테스트 계약을 새 기준으로 바꿀 지점을 확인한다.

### M2 - Runtime Layout

- header compact controls를 추가한다.
- footer navigation을 제거한다.
- Workbench/Shelf/Tools strip 표시 조건과 height를 조정한다.

### M3 - Verification

- PlayMode/EditMode tests
- release validation
- QA balance report
- Android screenshot batch
- visual review note 갱신

## Task Breakdown

### GL2-001 - Plan Lock

- Outputs:
  - `GAMEPLAY_LAYOUT_V2_PLAN.md`
- Work:
  - 상시 노출/조건부 노출 기준을 확정한다.
  - 자동 검증과 manual visual gate를 분리한다.
- Verification:
  - 문서 자체 self-review
- Done criteria:
  - 구현 중 다시 정보 우선순위를 재논의하지 않아도 된다.

### GL2-002 - Compact Header Navigation

- Outputs:
  - gameplay header compact controls
- Work:
  - footer의 `Restart`, `Levels`, `Pause`, `Next`를 header control로 이동한다.
  - status message를 header 아래 조건부 메시지로 바꾼다.
- Verification:
  - PlayMode에서 navigation text와 screen transition 유지
- Done criteria:
  - 하단 footer가 gameplay board 공간을 차지하지 않는다.

### GL2-003 - Main Board Expansion

- Outputs:
  - larger dream/request rows
  - reduced section titles
- Work:
  - dream/order section title을 제거하거나 최소화한다.
  - dream/order row height를 늘린다.
  - Workbench는 기본 숨김으로 전환한다.
- Verification:
  - PlayMode UI presence tests
  - Android screenshot review
- Done criteria:
  - 기본 level screenshot에서 dream/order card가 이전보다 크게 보인다.

### GL2-004 - Utility Strip Compression

- Outputs:
  - compact shelf strip
  - compact tools/faults strip
- Work:
  - storage/modifier section title을 제거한다.
  - storage/modifier height를 줄인다.
  - 없는 기능은 화면에 보이지 않게 한다.
- Verification:
  - modifier level PlayMode test
  - selected level screenshot review
- Done criteria:
  - utility UI가 puzzle board보다 시각적으로 크지 않다.

### GL2-005 - Verification And Notes

- Outputs:
  - updated plan status
  - updated visual issue inventory
- Work:
  - 테스트와 screenshot 결과를 문서에 기록한다.
  - 남은 UI 문제를 다음 pass 입력으로 남긴다.
- Verification:
  - `git diff --check`
  - EditMode/PlayMode tests
  - release/balance checks
  - Android screenshot batch
- Done criteria:
  - 자동 검증 결과와 manual visual gap이 분리되어 있다.

## PR Plan

현재 branch의 Alpha readiness PR 안에 포함한다.

이유:

- 이번 변경은 `AR-003`의 후속 layout correction이다.
- 별도 PR로 쪼개면 visual issue inventory와 구현 결과가 분리되어 추적이 어려워진다.

## Verification And Test Plan

필수 자동 검증:

```powershell
git diff --check
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
```

Android/screenshot 검증:

```powershell
.\DreamLaundromat\level-screenshots.cmd -Build -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 1200 -BuildTimeoutSeconds 1200
```

Manual visual gate:

- 대표 screenshot에서 dream/order/action이 이전보다 크게 읽히는지 확인한다.
- header compact controls가 너무 작거나 찾기 어려운지 확인한다.
- Workbench 기본 숨김이 초반 이해를 해치지 않는지 확인한다.
- storage/tool/fault가 있는 level에서 strip이 너무 작아져 의미를 잃지 않았는지 확인한다.

## CLI And Manual Boundary

CLI로 가능한 것:

- Unity tests
- release validation
- QA balance report
- Android build/install/screenshot batch
- screenshot 파일 생성과 기본 시각 확인

사람 판단이 필요한 것:

- 실제로 더 읽기 쉬워졌는지
- 한손 조작 중 header navigation이 불편하지 않은지
- 첫 플레이어가 Workbench 없이도 시작 행동을 이해하는지
- 작은 화면 기기에서 touch target이 충분한지

## Risks

- `Workbench`를 숨기면 초반 안내가 부족해질 수 있다.
- header compact control이 너무 작으면 navigation discoverability가 나빠질 수 있다.
- utility strip을 줄이면 modifier 의미가 덜 읽힐 수 있다.
- controller에 layout 조건이 더 쌓이므로 다음 `AR-004`에서 책임 분리가 더 중요해진다.

## Deferred Or Out Of Scope

game-local backlog:

- icon-only navigation
- bottom sheet / drawer interaction
- final action dock art
- animated card focus zoom
- responsive breakpoint별 별도 layout

`docs/TODO.md` 대상은 아니다. 이번 항목은 특정 게임의 gameplay layout backlog다.

## First Implementation Step

`ReleaseGameController.BuildGameplayScreen`에서 footer navigation을 제거하고 header compact
controls를 만든다. 그 다음 `RenderPreview`, `RenderStorage`, `RenderModifiers`의 표시 조건과
height를 줄인다.

## Current Execution Status

2026-06-18 기준 첫 구현 pass를 완료했다.

반영한 내용:

- footer navigation을 제거하고 `Restart`, `Levels`, `Pause`를 header 우상단 compact
  controls로 이동했다.
- `Next`는 gameplay footer에 상시 노출하지 않고 result screen 중심 흐름으로 유지했다.
- `Workbench`는 기본 상태에서 숨기고, dream/order/storage 선택 또는 submit 가능 상태가
  생길 때만 나타나게 했다.
- `Dreams`와 `Requests`의 section title을 제거하고 card row 높이를 키웠다.
- 빈 `Shelf`는 기본 상태에서 숨기고, 저장된 꿈이 있거나 선택한 dream을 저장할 수 있을
  때만 나타나게 했다.
- 비활성 `Store 1/2` 버튼은 숨기고, 실제 저장 가능한 상태에서만 표시한다.
- `Tools/Faults`는 modifier가 있는 level에서만 compact strip으로 남겼다.
- PlayMode UI 계약을 “상시 노출”에서 “조건부 노출” 기준으로 갱신했다.

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

.\DreamLaundromat\level-screenshots.cmd -Build -LevelIndexes 0,9,29 -TimeoutSeconds 1200 -BuildTimeoutSeconds 1200
# Passed. 새 APK 빌드/설치 후 핵심 대표 화면을 확인했다.

.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900
# Passed. 전체 대표 screenshot set을 갱신했다.
```

시각 확인 결과:

- 기본 화면에서 dream/order card가 이전보다 크게 보인다.
- 빈 Shelf와 비활성 Store 버튼이 사라져 화면 하단의 불필요한 정보가 줄었다.
- header compact controls는 정상 크기로 보인다.
- 당시에는 submit, operation, modifier detail text의 텍스트 의존도가 남아 있었다.
  후속 `Action Dock Readability` pass에서 `Submit`, operation marker, compact modifier
  label로 추가 개선했다.

## Self-Review

검토 결과:

- Scope와 Non-Goals를 분리했다.
- 상시 노출/조건부 노출 기준을 명시했다.
- rules/model 변경을 제외했다.
- automated verification과 manual visual gate를 분리했다.
- PR은 현재 Alpha readiness PR 안에서 유지하는 것으로 정했다.
- 사용자에게 새 결정을 요구하는 항목은 없다. 현재 문제는 “정보가 너무 많다”는 방향이
  명확하므로 recommended default로 구현한다.
