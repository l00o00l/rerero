# Release UI Design Plan

## Summary

이 문서는 `DreamLaundromat`을 현재의 기능 검증용 UI에서 실제 출시 후보로 보일 수 있는
모바일 게임 UI와 디자인으로 끌어올리기 위한 상세 구현 계획이다.

현재 release slice는 30개 레벨, 진행 저장, tutorial, item/obstacle, QA report,
Android build/run/screenshot smoke를 갖추었다. 직전 Visual/UX pass로 상태 축과 카드
문구는 훨씬 읽기 쉬워졌다. 하지만 아직 첫 인상, 화면 흐름, 카드/버튼의 재사용 가능한
디자인 시스템, 결과 화면, 레벨 선택, 아트 정체성은 출시 앱 수준이 아니다.

이번 계획의 목표는 "동작하는 개발 UI"를 "처음 보는 플레이어에게 완성된 모바일 퍼즐
게임처럼 보이는 UI V1"로 바꾸는 것이다. 최종 상용 아트 전체 제작은 아니지만, 출시
판단이 가능한 화면 구조와 디자인 언어를 만든다.

## Planning References

- `DreamLaundromat/docs/FULL_GAME_ROADMAP.md`
- `DreamLaundromat/docs/PHASE_6_13_PLAN.md`
- `DreamLaundromat/docs/RELEASE_GAMEPLAY_SLICE_PLAN.md`
- `DreamLaundromat/docs/VISUAL_UX_DIRECTION_PLAN.md`
- `DreamLaundromat/docs/MODIFIER_ENGINE_PLAN.md`
- `docs/IMPLEMENTATION_PLANNING.md`

## Design Goal

출시 후보 UI는 다음 질문에 답해야 한다.

- 이 게임이 꿈 세탁소라는 것을 첫 화면에서 바로 알 수 있는가?
- 플레이어가 튜토리얼 문서를 읽지 않아도 꿈, 주문, 기계, 도구, 제약을 구분하는가?
- 세로 모바일 화면에서 한손으로 주요 조작을 반복해도 피로하지 않은가?
- clear/fail/result가 단순 문구가 아니라 게임의 보상/복구 순간처럼 느껴지는가?
- 새로운 레벨을 시작하고, 실패하고, 다시 시도하고, 다음 레벨로 가는 흐름이 출시 앱처럼
  자연스러운가?

성공 기준은 단순히 장식이 늘어나는 것이 아니다. `DreamLaundromat`의 퍼즐 재미인
상태 읽기, 주문 배정, 순서 계획, 공간 압박, 빠른 재시도를 더 빠르고 기분 좋게 만들 때
성공이다.

## Game Planning Alignment

### Core Fun

UI가 강화해야 할 핵심 재미:

- 꿈의 네 상태 축을 빠르게 읽고 어떤 주문에 쓸지 판단한다.
- operation 결과를 미리 보고, 몇 수 뒤의 주문/preview까지 고려한다.
- storage와 item/obstacle 때문에 생기는 작은 압박을 명확하게 이해한다.
- 실패해도 왜 막혔는지 납득하고 바로 다시 시도한다.

UI가 만들면 안 되는 anti-fun:

- 상태 축을 읽느라 퍼즐 판단보다 해석 비용이 더 커지는 것.
- 예쁜 장식이 실제 조작 영역을 밀어내는 것.
- item/obstacle이 보이지 않는 함정처럼 느껴지는 것.
- 실패 후 다음 행동이 불분명해서 흐름이 끊기는 것.

### Game Pillars

- `One-Hand Clarity`: 엄지 조작과 세로 화면 가독성을 우선한다.
- `Readable Dream State`: color-only 표현을 금지하고 label, icon, color를 같이 쓴다.
- `Dream Laundromat Identity`: 세탁소, 주문서, 꿈 조각, 특수 도구 은유를 화면 구조에
  반영한다.
- `Fast Retry`: 실패, 재시작, 다음 레벨 이동을 가장 짧은 흐름으로 둔다.
- `Honest Constraints`: 장애물과 불가능한 action은 이유를 보이게 한다.
- `Scalable Content`: 30개 레벨 이후 100개 이상으로 늘어도 UI와 level review가 감당
  가능해야 한다.

### Core Rules

UI는 기존 규칙을 바꾸지 않는다.

- `Dream`은 `Taint`, `Mood`, `Clarity`, `Stability` 상태 축을 가진다.
- `Order`는 상태 조건과 완료 카운트를 가진다.
- `ApplyOperation`, `SubmitDream`, `StoreDream`, `RecallDream`, `UseItem`을 유지한다.
- clear/fail 조건은 `DynamicRulesEngine`과 `ReleaseGameSession`의 결과를 따른다.
- undo는 이번 pass의 기본 scope에 넣지 않는다.
- hint는 full solver hint가 아니라 blocked reason과 operation preview까지만 다룬다.

### Puzzle Grammar

UI가 표현해야 하는 문법:

- 상태 축: `Clean/Nightmare`, `Calm/Anxious`, `Vivid/Blurry`, `Stable/Unsettled`
- 변환: `Wash`, `Soothe`, `Clarify`, `Settle`
- 제약: locked slot, pinned order, soft-blocked operation
- 도구: preview swap, dream refresh
- 목표: 주문 조건과 목표 완료 수

금지할 화면 패턴:

- 상태 축을 한 줄 텍스트로만 압축해서 작은 화면에서 읽기 어렵게 만드는 패턴
- 장애물과 도구를 같은 색/같은 모양으로 보여주는 패턴
- preview가 아래쪽에 묻혀서 다음 입력 정보가 보상보다 덜 중요해 보이는 패턴
- result가 footer 문구 하나로만 끝나는 패턴

### Level Progression

첫 출시 후보 UI는 최소 30레벨 pack을 자연스럽게 탐색할 수 있어야 한다.

- Level 1-3: 꿈/주문/submit 읽기
- Level 4-8: operation과 preview 읽기
- Level 9-13: item/obstacle 소개
- Level 14-30: 반복 숙련과 QA/balance loop

UI는 첫 10레벨에서 새 개념을 하나씩 보이게 해야 하며, level select나 result 화면에서
다음 레벨의 성격을 짧게 예고할 수 있어야 한다.

### Content Production

출시형 UI는 콘텐츠 생산에도 영향을 준다.

- level별 screenshot review를 자동화할 수 있어야 한다.
- QA report는 visual checklist와 feedback timing을 계속 포함해야 한다.
- level metadata의 `DifficultyBand`, `TutorialTags`, `ManualGateNote`를 UI에서 일부
  활용할 수 있어야 한다.
- design token과 card renderer가 level 수 증가에 따라 흔들리지 않아야 한다.

### UX / Interaction

기본 입력 모델:

- portrait
- one-hand touch
- tap selection
- drag는 이번 pass의 필수 조건이 아니다.
- operation preview와 disabled reason을 우선한다.

핵심 흐름:

1. Title/Home에서 바로 이어하기 또는 level select로 진입한다.
2. Level Select에서 unlocked/cleared/current 상태를 읽는다.
3. Gameplay에서 꿈, 주문, preview, storage, tool/obstacle, action을 조작한다.
4. Pause/Settings에서 sound, haptic, contrast, reduced motion, restart를 관리한다.
5. Result에서 clear/fail 이유, moves, next/retry를 명확히 보여준다.

### Satisfaction Design

반복 action에 필요한 감각:

- operation 성공: 짧고 가벼운 pulse, 낮은 부담의 SFX
- submit 성공: 주문서가 채워지는 느낌
- item 사용: 특수 도구를 썼다는 분명한 변화
- obstacle block: 실패음보다 부드러운 경고
- clear: 짧은 축하, haptic optional
- fail: 원인 설명과 즉시 retry

현재 generated tone은 유지하되, timing table과 event hook을 디자인 시스템의 일부로 둔다.

### World / Character

화면 은유:

- Dream card: 세탁 태그가 붙은 꿈 조각
- Order card: 손님 요청서
- Operation controls: 꿈 처리 기계 조작부
- Storage: 보관 선반
- Item: 야간 세탁소의 특수 도구
- Obstacle: 예약, 고장, 임시 제한
- Level Select: 세탁소 작업대 또는 주문판

세계관은 규칙 기억을 도와야 한다. 장식이 규칙을 숨기면 실패다.

### Prototype Success Criteria

첫 구현 pass는 다음을 증명해야 한다.

- Android screenshot만 봐도 출시 후보 게임 화면처럼 보인다.
- 꿈/주문/action/tool/obstacle/result/settings 역할이 즉시 구분된다.
- 720x1280과 1080x1920에서 텍스트가 겹치지 않는다.
- PlayMode 테스트가 주요 화면 흐름과 core text/icon 존재를 확인한다.
- screenshot smoke와 QA report가 UI 변경 PR마다 재현 가능하다.
- 사람이 봐야 하는 visual taste, 손맛, SFX/haptic 감각은 manual gate로 남아 있다.

## Scope

포함 범위:

- 출시 후보용 화면 흐름 V1
  - Title/Home
  - Level Select
  - Gameplay
  - Pause/Settings
  - Clear Result
  - Fail Result
- Gameplay screen redesign
  - Dream card
  - Order card
  - Preview
  - Storage
  - Tool/Obstacle strip
  - Action panel
  - Settings/Pause access
- Design system V1
  - palette
  - typography scale
  - spacing
  - card styles
  - button states
  - badge/icon rules
  - result state styling
- Art direction V1
  - code-native shapes and simple generated bitmap/icon assets if useful
  - no paid asset dependency by default
- Art asset plan V1
  - title/home background
  - level select board/background
  - dream/order/card frame assets
  - state, operation, item, obstacle icon set
  - result and feedback effect sprites
  - asset storage/import/size policy
- UI architecture cleanup
  - `ReleaseGameController` rendering responsibility 분리
  - presenter/renderer/helper 단위 도입
  - scene/YAML churn 최소화
- Verification
  - EditMode/PlayMode tests
  - release validation
  - QA/balance report
  - Android build/run
  - screenshot smoke
  - manual review checklist

## Non-Goals

이번 pass에서 제외한다.

- Google Play store listing final screenshots
- app icon final
- splash screen final
- final commercial illustration pack
- paid asset purchase
- monetization, ads, IAP
- analytics SDK
- cloud save
- localization
- live event/challenge mode
- complete prefab/UI Toolkit migration
- full hint system
- final release AAB signing

## Key Decisions

- Primary platform은 Android portrait one-hand touch다.
- 현재 `ReleaseGameplaySlice` scene을 유지하되, UI renderer를 분리한다.
- 첫 pass는 uGUI/programmatic UI를 유지한다. 대규모 prefab 전환은 별도 판단으로 둔다.
- 디자인은 어두운 디버그 패널 중심에서 벗어나, 조용한 야간 세탁소 분위기의 muted base와
  선명한 semantic accent를 쓴다.
- 외부 유료 asset은 기본적으로 사용하지 않는다.
- 필요한 경우 generated bitmap asset이나 코드 기반 아이콘/shape를 만든다.
- color-only 구분은 금지한다.
- Result 화면과 Level Select는 출시 후보 인상을 위해 필수 scope에 포함한다.
- Settings는 gameplay action과 섞이지 않게 pause/settings surface로 분리한다.
- full art polish보다 readability, touch comfort, screenshot consistency를 우선한다.

## Target Platforms

Primary:

- Android
- Portrait
- One-hand touch
- baseline resolution: 1080x1920

Secondary checks:

- 720x1280
- 1440x2960 또는 긴 세로 화면
- Windows Editor PlayMode

Required scripts:

- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
- `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 240 -BuildTimeoutSeconds 1200`
- `.\DreamLaundromat\screenshot-smoke.cmd`

주의:

- 같은 Unity project의 batchmode 명령은 병렬로 실행하지 않는다.
- Android screenshot은 nonblank/focus/crash smoke까지 자동화하고, 시각적 취향은 사람이
  판단한다.

## Architecture

현재 구조:

- `Thkim.DreamLaundromat.DynamicLab`
  - pure puzzle rules, solver, generator, metrics
- `Thkim.DreamLaundromat.Gameplay.ReleaseSlice`
  - release level pack
  - session/progression/settings/feedback
  - visual descriptors/style
  - QA/balance report
- `Thkim.DreamLaundromat.Editor.ReleaseSlice`
  - scene setup
  - validation report
  - balance report

추가/정리할 runtime 구조:

- `ReleaseUiFlowController`
  - title, level select, gameplay, pause, result state 전환
- `ReleaseGameplayPresenter`
  - `ReleaseGameSession` 상태를 UI view model로 변환
- `ReleaseCardRenderer`
  - dream/order/storage/tool/obstacle card label, color, state를 생성
- `ReleaseActionPanelRenderer`
  - operation, submit, store, recall, item action button 표시
- `ReleaseResultPresenter`
  - clear/fail summary, moves, retry/next action 표시
- `ReleaseLevelSelectPresenter`
  - level unlock/clear/current 상태 표시
- `ReleaseDesignTokens`
  - style token을 `ReleaseVisualStyle`에서 확장하거나 별도 파일로 분리

구현 원칙:

- rules/model은 scene UI에 의존하지 않는다.
- UI renderer는 가능한 한 pure string/color/layout descriptor를 반환해 EditMode 테스트가
  가능해야 한다.
- scene-wide lookup을 늘리지 않는다.
- Unity scene/YAML 변경은 필요한 만큼만 한다.
- 새 asset은 `.meta`와 함께 commit 대상이 되어야 한다.

## Data Model

추가하거나 정리할 데이터:

- `ReleaseScreenState`
  - `Title`
  - `LevelSelect`
  - `Gameplay`
  - `Pause`
  - `ClearResult`
  - `FailResult`
- `ReleaseLevelTileState`
  - level index
  - level id
  - display name
  - difficulty band
  - unlocked
  - cleared
  - current
  - tutorial tags summary
- `ReleaseCardVisual`
  - title
  - subtitle
  - badge lines
  - semantic color
  - selected/locked/disabled state
- `ReleaseButtonVisual`
  - label
  - hint
  - enabled
  - disabled reason
  - semantic color
- `ReleaseResultSummary`
  - status
  - level id
  - completed orders
  - remaining moves
  - move count
  - failure reason
  - next available
- `ReleaseDesignTokenSet`
  - palette
  - typography
  - spacing
  - touch target
  - card heights
  - feedback durations

기존 save/progress:

- `ReleaseProgressState`의 `HighestUnlockedLevelIndex`, `CompletedLevelIds`, settings를
  level select와 result screen에 연결한다.

## Scene And UI Plan

### App Flow

```text
Title/Home
  -> Continue
  -> Level Select
  -> Settings

Level Select
  -> Gameplay
  -> Title/Home

Gameplay
  -> Pause
  -> Clear Result
  -> Fail Result

Pause
  -> Resume
  -> Restart
  -> Level Select
  -> Settings toggles

Clear Result
  -> Next Level
  -> Replay
  -> Level Select

Fail Result
  -> Retry
  -> Level Select
```

### Title/Home

목표:

- 첫 화면에서 `DreamLaundromat`의 브랜드와 꿈 세탁소 분위기를 보여준다.
- landing page처럼 설명하지 않고 바로 플레이 흐름으로 들어간다.

구성:

- `Dream Laundromat` title
- small subtitle: `Night shift puzzle laundry`
- Continue button
- Level Select button
- Settings button
- current progress summary

Non-goal:

- 마케팅 hero page나 긴 설명 화면은 만들지 않는다.

### Level Select

목표:

- 30개 level의 unlock/clear/current 상태를 읽고 진입한다.
- 출시 후 100개 이상으로 늘려도 확장 가능해야 한다.

구성:

- scrollable compact grid 또는 vertical chapter list
- level tile:
  - number
  - difficulty band
  - cleared marker
  - locked marker
  - introduced concept tag
- current level quick jump

기본 은유:

- 세탁소 주문판 또는 작업표.

### Gameplay

목표:

- 현재 퍼즐 판단에 필요한 정보를 한 화면에서 읽게 한다.
- 기존 release slice의 조작을 유지하되, 시각 체계를 출시 후보 수준으로 끌어올린다.

정보 우선순위:

1. 목표와 남은 move
2. Active Dreams
3. Active Orders
4. 선택된 꿈의 operation preview
5. Preview와 Storage
6. Tool/Obstacle
7. Message/result

권장 구조:

- Header:
  - level name/id
  - difficulty
  - moves/orders
  - pause button
- Board:
  - dream lane
  - order lane
  - preview/storage compact lane
- Action area:
  - operation buttons
  - submit/store/recall
  - item buttons when relevant
- Message strip:
  - blocked reason
  - tutorial prompt

### Pause/Settings

목표:

- gameplay action과 settings를 명확히 분리한다.
- 모바일 게임의 기본 기대 흐름을 제공한다.

구성:

- Resume
- Restart
- Level Select
- Sound
- Haptic
- High Contrast
- Reduced Motion
- Large Text

주의:

- settings toggle은 action button처럼 보이지 않아야 한다.

### Result Screens

Clear Result:

- `Cleared`
- completed orders
- remaining moves 또는 used moves
- next level CTA
- replay CTA
- level select CTA
- short satisfaction feedback

Fail Result:

- `Failed`
- failure reason
- retry CTA
- level select CTA
- optional small tip based on failure reason

Result는 modal/card 하나로 처리할 수 있지만, gameplay footer 문구만으로 끝내지 않는다.

## Design System V1

### Palette

방향:

- 어두운 야간 세탁소 base
- 꿈 상태와 action만 semantic accent 사용
- 한 hue family만 지배하지 않게 한다.

기본 역할:

- background
- surface
- elevated surface
- text primary
- text secondary
- dream clean/nightmare
- mood calm/anxious
- clarity vivid/blurry
- stability stable/unsettled
- action wash/soothe/clarify/settle
- item/tool
- obstacle/warning
- success
- failure
- selected/focus

### Typography

원칙:

- hero-scale text는 Title/Home에만 사용한다.
- gameplay card 내부는 작은 heading과 body text를 쓴다.
- viewport width 기반 font scaling은 쓰지 않는다.
- long word나 badge label이 버튼 밖으로 나가지 않게 한다.

### Cards

Dream card:

- title: `Dream 0`
- object metaphor: laundry tag / memory cloth
- state badges: 4축
- selected state
- locked state

Order card:

- title: `Order 0`
- object metaphor: request sheet
- requirement badges
- progress count

Tool card:

- title: item name
- charge
- target requirement
- effect preview

Obstacle card:

- title: obstacle name
- remaining block count
- blocked target
- resolved state

### Buttons

버튼 종류:

- primary CTA: Continue, Next, Retry
- secondary CTA: Level Select, Replay
- gameplay action: operation, submit, store, recall
- tool action
- settings toggle
- icon/small control: pause

상태:

- normal
- highlighted/selected
- disabled with reason
- pressed

### Icons And Symbols

필수 아이콘/심볼:

- Taint Clean/Nightmare
- Mood Calm/Anxious
- Clarity Vivid/Blurry
- Stability Stable/Unsettled
- Wash/Soothe/Clarify/Settle
- Submit
- Store/Recall
- Preview
- Tool
- Obstacle
- Lock
- Pause/Settings
- Clear/Fail

기본 구현:

- 첫 pass는 code-native text marker + simple geometric icon으로 시작한다.
- 필요하면 generated bitmap icon set을 `Assets/_Project/Art/UI/` 아래에 둔다.
- 모든 bitmap asset은 크기와 LFS 필요 여부를 검토한다.

## Art Direction V1

Tone:

- 조용한 야간 세탁소
- cozy하지만 약간 이상한 꿈의 질감
- 과한 공포나 과한 귀여움은 피한다.

Visual language:

- soft fabric edge
- paper order sheet
- muted machine panel
- small glowing memory marks
- clear semantic badges

추천 스타일:

- flat/vector-like base
- 약한 texture
- 높은 readability
- 작은 모바일 화면에서 손실되지 않는 silhouette

기본값:

- 유료 asset 없이 code-native UI와 generated/simple bitmap icons로 시작한다.
- 최종 painterly art는 core UI가 검증된 뒤 판단한다.

## Art Asset Plan

출시 후보 UI에는 실제 아트/이미지 에셋이 필요하다. 다만 첫 pass의 목적은 최종 상용
일러스트를 대량으로 만드는 것이 아니라, 게임 정체성과 규칙 판독성을 동시에 검증할 수
있는 최소 에셋 세트를 만드는 것이다.

### Asset Strategy

기본 전략:

- UI 구조와 layout은 code-native/uGUI로 유지한다.
- 게임 정체성을 만드는 핵심 지점에만 bitmap/icon asset을 넣는다.
- 모든 asset은 규칙 이해를 도와야 한다.
- 장식용 이미지가 조작 영역과 상태 판독성을 밀어내면 제외한다.
- 외부 유료 asset은 기본적으로 사용하지 않는다.
- 무료 asset은 license, redistribution 가능 여부, attribution 필요 여부를 확인하기 전에는
  repository에 넣지 않는다.
- generated bitmap asset은 prompt/source note와 용도를 문서에 남긴다.

권장 첫 구현 방식:

- `Title/Home`과 `Level Select`에는 분위기용 bitmap background를 사용한다.
- Gameplay의 Dream/Order/Tool/Obstacle은 frame, badge, icon 중심으로 적용한다.
- 상태 축과 operation은 작은 화면에서 읽히는 단순 icon set으로 시작한다.
- clear/fail은 full illustration보다 compact result visual과 effect sprite로 시작한다.

### Asset Folders

기본 저장 위치:

```text
DreamLaundromat/Assets/_Project/Art/UI/
DreamLaundromat/Assets/_Project/Art/UI/Backgrounds/
DreamLaundromat/Assets/_Project/Art/UI/Cards/
DreamLaundromat/Assets/_Project/Art/UI/Icons/
DreamLaundromat/Assets/_Project/Art/UI/Effects/
DreamLaundromat/Assets/_Project/Art/UI/SourceNotes/
```

규칙:

- Unity asset은 `.meta`와 함께 commit한다.
- 생성형 asset의 prompt, 생성일, 용도, 수정 여부는 `SourceNotes/`에 짧게 기록한다.
- 원본 대용량 source file은 첫 pass에서 repository에 넣지 않는다.
- 반복 사용되는 runtime asset만 `Assets/_Project/Art/UI/`에 둔다.
- third-party asset은 `Assets/ThirdParty/` 또는 package 경로를 사용하고, `_Project/Art/UI/`
  안에 vendor 원본을 섞지 않는다.

### Minimum Asset Set

#### Priority 1 - Identity And Readability

출시 후보 첫 인상을 위해 가장 먼저 필요하다.

- `title_night_laundromat_background`
  - 화면: Title/Home
  - 역할: 앱 시작 시 꿈 세탁소 정체성을 즉시 보여준다.
  - 권장 형식: PNG, portrait-friendly, 1440x2560 이하
  - 어두운 전체 tone이더라도 주요 title text 뒤는 읽혀야 한다.
- `level_select_order_board_background`
  - 화면: Level Select
  - 역할: 레벨 선택을 세탁소 작업표/주문판처럼 보이게 한다.
  - 권장 형식: PNG, 1440x2560 이하
- `dream_card_frame`
  - 화면: Gameplay
  - 역할: Dream이 일반 버튼이 아니라 조작 대상 카드임을 보여준다.
  - 권장 형식: transparent PNG 또는 code-native 9-slice style
- `order_sheet_frame`
  - 화면: Gameplay
  - 역할: Order가 손님 요청서임을 보여준다.
  - 권장 형식: transparent PNG 또는 code-native 9-slice style

#### Priority 2 - State And Action Icons

퍼즐 판독성을 위해 필요하다.

- State icons:
  - `state_taint_clean`
  - `state_taint_nightmare`
  - `state_mood_calm`
  - `state_mood_anxious`
  - `state_clarity_vivid`
  - `state_clarity_blurry`
  - `state_stability_stable`
  - `state_stability_unsettled`
- Operation icons:
  - `operation_wash`
  - `operation_soothe`
  - `operation_clarify`
  - `operation_settle`

권장 형식:

- transparent PNG
- 256x256 이하
- 단색 silhouette로도 구분 가능해야 한다.
- 텍스트 label 없이도 대략 의미가 떠올라야 하지만, gameplay UI에서는 label과 함께 쓴다.

#### Priority 3 - Tool And Obstacle Icons

item/obstacle이 실제 선택으로 보이게 만드는 데 필요하다.

- Tool icons:
  - `tool_preview_swap`
  - `tool_dream_refresh`
- Obstacle icons:
  - `obstacle_locked_slot`
  - `obstacle_order_pin`
  - `obstacle_operation_soft_block`

권장 형식:

- transparent PNG
- 256x256 이하
- tool은 player-owned 느낌, obstacle은 visible constraint 느낌으로 구분한다.
- tool과 obstacle을 색상만으로 구분하지 않는다.

#### Priority 4 - Result And Feedback Effects

반복 플레이 감각을 위해 필요하지만, Priority 1-3 이후에 적용한다.

- `effect_submit_success`
- `effect_item_use`
- `effect_obstacle_block`
- `effect_clear_glow`
- `effect_fail_warning`
- `result_clear_mark`
- `result_fail_mark`

권장 형식:

- transparent PNG sprite
- 작은 pulse/glow 중심
- reduced motion일 때는 animation 대신 static sprite + message로 대체 가능해야 한다.

### Generated Bitmap Policy

generated bitmap을 사용할 때의 기본값:

- 먼저 low-risk UI asset부터 생성한다.
- 사람 얼굴, 특정 작가 스타일 모방, 상표/저작권 캐릭터는 사용하지 않는다.
- prompt는 `SourceNotes/`에 기록한다.
- 생성 후 Unity 화면에서 실제 크기로 확인한다.
- asset이 너무 stock-like하거나 게임 규칙을 흐리면 폐기한다.

추천 생성 순서:

1. Title/Home background
2. Level Select order board background
3. Dream/Order frame texture
4. State and operation icon set
5. Tool/Obstacle icon set
6. Result/effect sprites

### Free And Paid Asset Policy

무료 asset:

- license가 명확해야 한다.
- commercial use 가능 여부를 확인한다.
- attribution 필요 여부를 문서에 남긴다.
- redistribution 금지 asset은 repository에 넣지 않는다.

유료 asset:

- 이번 pass 기본값은 사용하지 않음이다.
- 구매가 필요하면 사람이 직접 구매/라이선스 확인을 해야 한다.
- 구매 asset의 원본 package를 repository에 넣을 수 있는지 확인한다.
- 팀 공유가 필요한 경우 license seat와 저장 정책을 먼저 정한다.

### Import And Size Policy

권장 import 기준:

- UI icon: transparent PNG, 128x128 또는 256x256
- card frame: transparent PNG 또는 9-slice용 sprite, 512x512 이하부터 시작
- title/level select background: PNG, 1440x2560 이하부터 시작
- effect sprite: transparent PNG, 512x512 이하

용량 기준:

- 파일 하나가 1MB를 넘으면 필요성을 검토한다.
- 파일 하나가 5MB를 넘으면 Git LFS 또는 해상도 축소를 검토한다.
- 여러 bitmap asset이 한 번에 늘어나면 `.gitattributes`와 LFS 정책을 다시 확인한다.

Unity import 기준:

- UI sprite는 Sprite import로 설정한다.
- icon과 frame은 mipmap을 끄는 것을 기본으로 검토한다.
- 큰 background는 압축 품질과 Android 메모리 사용량을 확인한다.
- pixel-perfect보다 모바일 가독성과 메모리 사용량을 우선한다.

### Asset Verification

자동 검증:

- `.meta` 누락 검사
- asset path convention 검사
- changed asset size report
- PlayMode icon/text presence
- Android screenshot smoke

수동 검증:

- Title/Home 첫 인상이 게임 정체성을 전달하는지
- 작은 화면에서 icon silhouette가 유지되는지
- 상태 icon이 label 없이도 서로 구분되는지
- Dream/Order/Tool/Obstacle이 서로 다른 역할로 보이는지
- background가 gameplay readability를 해치지 않는지

### Asset Acceptance Criteria

asset을 포함하려면 다음 조건을 만족해야 한다.

- 게임 규칙 또는 화면 역할을 더 잘 읽게 만든다.
- 모바일 실제 크기에서 구분된다.
- 텍스트와 조작 영역을 가리지 않는다.
- license/source가 명확하다.
- `.meta`가 함께 있다.
- 용량이 합리적이고 LFS 필요 여부가 검토됐다.
- Android screenshot에서 깨지지 않는다.

## Milestones

### M1 - Release UI Design Lock

- release UI 목표와 화면 흐름 확정
- design token 목록 확정
- current screenshot 문제 기록
- user decision 기본값 확정

### M2 - UI Architecture Split

- `ReleaseGameController`에서 card/action/result rendering helper 분리
- screen state와 presenter 도입
- 기존 PlayMode tests 유지

### M3 - App Flow Screens

- Title/Home
- Level Select
- Pause/Settings
- Clear/Fail Result
- gameplay 진입/복귀 흐름

### M4 - Gameplay Screen Redesign

- dream/order cards
- preview/storage compact layout
- action panel
- tool/obstacle strip
- message/tutorial strip

### M5 - Design System And Art V1

- palette/typography/card/button/badge token
- simple icons/symbols
- optional generated bitmap assets
- minimum art asset set 적용
- asset source notes와 size policy 확인
- accessibility contrast/touch target audit

### M6 - Feedback And Motion V1

- action feedback mapping
- result feedback
- reduced motion
- generated SFX/haptic hook 유지

### M7 - Screenshot And Review Automation

- current screenshot smoke 유지
- optional level screenshot batch
- visual review checklist update
- PR summary artifact paths

### M8 - Full Verification Gate

- EditMode/PlayMode
- release-slice
- qa-balance
- Android build/run
- screenshot smoke
- manual review checklist

## Task Breakdown

### RUI-001 - Current UI Audit And Baseline

- Outputs:
  - baseline screenshot notes in `RELEASE_UI_DESIGN_PLAN.md` 또는 QA report
  - current biggest UI issues list
- Work:
  - 최신 `android-screenshot-smoke.png` 확인
  - first impression, hierarchy, readability, touch, result flow 문제 기록
  - current UI와 release target의 차이를 정리
- Verification:
  - `.\DreamLaundromat\screenshot-smoke.cmd`
  - screenshot path와 focused package 확인
- Done criteria:
  - 구현 전 비교 기준이 명확하다.

### RUI-002 - Design Token Set V1

- Outputs:
  - `ReleaseDesignTokens` 또는 확장된 `ReleaseVisualStyle`
  - EditMode tests for token completeness/contrast
- Work:
  - semantic colors 확장
  - typography scale 정의
  - spacing/touch/card height token 정의
  - result/settings/tool/obstacle color 추가
- Verification:
  - EditMode contrast/touch target tests
  - `ReleaseAccessibilityAudit`
- Done criteria:
  - 새 화면과 기존 gameplay가 같은 token set을 쓴다.

### RUI-003 - UI Presenter Split

- Outputs:
  - `ReleaseGameplayPresenter`
  - `ReleaseCardRenderer`
  - `ReleaseActionPanelRenderer`
- Work:
  - `ReleaseGameController`의 label/color 생성 책임 분리
  - session state를 view descriptor로 변환
  - pure helper는 EditMode에서 검증 가능하게 유지
- Verification:
  - existing PlayMode tests pass
  - new EditMode tests for card/action descriptors
- Done criteria:
  - gameplay UI 변경이 controller 대형 수정 없이 가능하다.

### RUI-004 - Screen Flow State

- Outputs:
  - `ReleaseScreenState`
  - title/level select/gameplay/pause/result 전환
- Work:
  - app flow state 추가
  - current level continuation과 level select 진입 처리
  - pause/resume/restart/result flow 구현
- Verification:
  - PlayMode screen transition tests
- Done criteria:
  - scene 하나 안에서 출시 앱 기본 화면 흐름을 재현한다.

### RUI-005 - Title/Home Screen V1

- Outputs:
  - title/home UI
- Work:
  - game title, continue, level select, settings, progress summary
  - Dream Laundromat identity first signal 추가
- Verification:
  - PlayMode title screen labels/buttons
  - Android screenshot review
- Done criteria:
  - 앱 시작 화면이 debug scene처럼 보이지 않는다.

### RUI-006 - Level Select V1

- Outputs:
  - 30 level select surface
  - level tile descriptor
- Work:
  - unlocked/locked/cleared/current 상태 표시
  - difficulty band와 concept tag 표시
  - current progress에서 continue 가능
- Verification:
  - EditMode level tile descriptor tests
  - PlayMode level select navigation tests
- Done criteria:
  - 30개 레벨을 출시 앱처럼 선택할 수 있다.

### RUI-007 - Gameplay Layout Redesign

- Outputs:
  - redesigned gameplay layout
- Work:
  - header 목표/진행 요약 정리
  - dream/order cards visual hierarchy 개선
  - preview/storage lane 압축
  - action panel과 message strip 정리
  - pause access 추가
- Verification:
  - PlayMode core UI presence
  - Android screenshot smoke
  - manual screenshot review
- Done criteria:
  - gameplay screenshot만 봐도 퍼즐 역할과 조작 흐름이 이해된다.

### RUI-008 - Card And Badge System

- Outputs:
  - dream/order/tool/obstacle card descriptors
  - state badge descriptors
- Work:
  - state icon/symbol rules 고정
  - selected/locked/disabled/resolved 상태 표현
  - item/obstacle role shape/color 구분
- Verification:
  - EditMode descriptor tests
  - PlayMode item/obstacle level UI tests
- Done criteria:
  - color 없이도 카드 역할과 상태를 읽을 수 있다.

### RUI-009 - Result And Failure UX

- Outputs:
  - clear result screen
  - fail result screen
  - result summary model
- Work:
  - clear summary, next, replay, level select
  - fail reason, retry, level select
  - failure reason별 short copy hook
- Verification:
  - PlayMode clear/fail result tests
  - manual retry flow check
- Done criteria:
  - 라운드 종료 후 다음 행동이 즉시 보인다.

### RUI-010 - Pause And Settings UX

- Outputs:
  - pause overlay
  - settings surface
- Work:
  - Resume, Restart, Level Select
  - sound/haptic/high contrast/reduced motion/large text
  - settings save와 UI 상태 연결
- Verification:
  - EditMode settings persistence tests
  - PlayMode pause/settings tests
- Done criteria:
  - settings가 gameplay action처럼 보이지 않고, 저장 상태와 동기화된다.

### RUI-011 - Feedback, Motion, Reduced Motion

- Outputs:
  - feedback visual hooks
  - reduced motion handling
- Work:
  - operation/submit/item/block/clear/fail feedback mapping
  - timing table과 UI event 연결
  - reduced motion일 때 motion 대신 color/text feedback 유지
- Verification:
  - EditMode timing table tests
  - PlayMode event hook smoke
  - manual audio/haptic/motion check
- Done criteria:
  - 반복 action이 구분되고, reduced motion 설정이 의미를 가진다.

### RUI-012 - Icon And Asset Pass V1

- Outputs:
  - `Assets/_Project/Art/UI/` folder structure
  - Priority 1 identity assets
  - Priority 2 state/action icons
  - Priority 3 tool/obstacle icons when feasible
  - asset source notes
  - asset size report
- Work:
  - `Backgrounds`, `Cards`, `Icons`, `Effects`, `SourceNotes` 폴더 생성
  - Title/Home background와 Level Select background 적용
  - Dream card frame과 Order sheet frame 적용
  - 8개 state icon과 4개 operation icon 제작/적용
  - `Preview Swap`, `Dream Refresh`, `Locked Slot`, `Order Pin`, `Operation Soft Block`
    icon 제작/적용 여부 판단
  - generated bitmap을 쓰면 prompt/source note 작성
  - 무료 asset을 쓰면 license/attribution note 작성
  - `.meta`, per-file size, LFS 필요 여부 확인
- Verification:
  - asset `.meta` check
  - asset path convention check
  - changed asset size report
  - EditMode descriptor/icon mapping tests
  - PlayMode icon presence
  - Android screenshot smoke
  - manual screenshot review
- Done criteria:
  - 첫 화면, level select, gameplay card/icon이 실제 이미지와 함께 출시 후보처럼 보인다.
  - 필수 아이콘이 화면에서 역할 기억을 돕는다.
  - asset source와 용량 정책이 review 가능하다.

Current implementation note:

- `ReleaseUiArtGenerator`가 `Backgrounds`, `Cards`, `Icons`, `Effects`, `SourceNotes`
  구조와 `ReleaseUiArtCatalog.asset`을 생성한다.
- `ReleaseGameplaySlice` scene은 `ReleaseUiArtCatalog`를 직렬화 참조하고, gameplay
  background, header background, dream/order/storage frame, state/operation/tool/obstacle
  icon을 적용한다.
- Gameplay 화면은 panel transparency, text shadow, button/card chrome을 적용해 첫
  generated art pass보다 덜 debug 화면처럼 보이게 조정했다.
- `Title/Home`과 `Level Select`용 background asset은 생성되어 있지만, 해당 화면 flow와
  최소 flow는 `ReleaseGameController`에 구현되어 있다.
- 앱 시작 화면은 `Home`이며, `Continue`와 `Level Select`를 통해 gameplay로 진입한다.
- `Level Select`는 30개 release level과 unlock/current/locked 상태를 scroll list로
  보여준다.
- Gameplay footer에는 `Levels` 진입이 추가되어 gameplay에서 level list로 돌아갈 수
  있다.
- 현재 화면은 아직 출시 후보 UI의 최종 형태가 아니다. `Home`, `Level Select`,
  `Pause`, `Result` 흐름은 들어갔지만 gameplay 화면은 여전히 programmatic layout의
  밀도와 사각 section 구조가 강하므로, 다음 pass에서는 card/section hierarchy와
  touch-first interaction polish를 더 줄여야 한다.
- 현재 에셋은 외부 무료/유료 asset이 아니라 코드 기반 generated bitmap이므로 별도
  attribution은 필요하지 않다.

Completed implementation pass:

- Summary:
  - 앱이 곧바로 puzzle board로 진입하는 debug flow를 벗어나 `Home -> Level Select ->
    Gameplay` 흐름을 만든다.
  - 목적은 첫 실행 screenshot이 출시 후보 앱의 첫 화면처럼 보이고, 플레이어가 레벨
    진행 상태와 재진입 지점을 자연스럽게 이해하게 만드는 것이다.
- Scope:
  - `ReleaseGameController` 안에 screen flow state를 추가한다.
  - `Home` screen은 `Dream Laundromat` identity, progress summary, `Continue`,
    `Level Select` 진입을 제공한다.
  - `Level Select` screen은 30개 release level, unlock 상태, current progress를
    scroll 가능한 목록으로 보여준다.
  - Gameplay footer에는 `Levels` 진입을 추가해 gameplay에서 level select로 돌아갈 수
    있게 한다.
  - 기존 gameplay board와 solver-driven tests는 계속 접근 가능해야 한다.
- Non-Goals:
  - 최종 result screen redesign
  - pause modal
  - monetization, app icon, store screenshot
  - prefab/UI Toolkit 전환
  - paid/free third-party asset 추가
- Key decisions:
  - 첫 pass는 현재 uGUI/programmatic UI를 유지한다.
  - 새 화면도 `ReleaseUiArtCatalog`의 background asset을 사용한다.
  - 씬 YAML을 손으로 수정하지 않고, 런타임 UI 생성과 기존 scene setup path를 유지한다.
  - 시작 시 session은 기존처럼 highest unlocked level을 준비하지만, 첫 화면은 `Home`
    으로 보여준다.
- Verification:
  - PlayMode: scene starts on Home, Continue opens Gameplay, Level Select lists levels,
    selecting an unlocked level opens Gameplay.
  - Existing PlayMode gameplay checks must still pass after opening Gameplay.
  - EditMode, `release-slice`, `qa-balance`, Android build/run, screenshot smoke를
    순차 실행한다.
- Manual check:
  - screenshot에서 첫 화면이 debug board가 아니라 title/home으로 보이는지 확인한다.
  - level list의 text clipping과 touch target은 사람 눈으로도 확인한다.

Completed implementation pass: Result/Pause and renderer helpers:

- Summary:
  - `Result/Pause flow`와 최소 renderer helper를 구현한다.
  - 목적은 라운드 종료가 footer 문구로만 끝나는 문제를 없애고, gameplay action과
    settings/pause를 분리하며, `ReleaseGameController`의 UI 문자열 생성 책임을 줄이는
    것이다.
- Scope:
  - `Clear Result`와 `Fail Result` screen을 추가한다.
  - clear result는 completed orders, remaining moves, next/replay/level select를 제공한다.
  - fail result는 failure reason, retry, level select를 제공한다.
  - `Pause` screen은 Resume, Restart, Level Select, Home, sound/haptic/contrast toggle을 제공한다.
  - Gameplay header 또는 footer에서 Pause 진입을 제공한다.
  - 카드/상태/result 문자열 생성은 `ReleaseGameplayCardRenderer`와
    `ReleaseResultSummary` 같은 pure helper로 일부 분리한다.
- Non-Goals:
  - 최종 애니메이션, SFX, haptic polish
  - 완전한 prefab/UI Toolkit 전환
  - 모든 gameplay section renderer의 전면 재작성
  - undo/hint 추가
- Key decisions:
  - 결과 화면은 `Gameplay` 위 footer message가 아니라 별도 screen mode로 다룬다.
  - result/pause screen은 기존 `ReleaseGameSession` state를 읽고, puzzle rules는 수정하지 않는다.
  - 화면 전환은 현재 uGUI/programmatic 구조를 유지한다.
  - renderer helper는 scene object를 참조하지 않는 static/pure helper로 시작한다.
- Verification:
  - PlayMode: Pause 진입/Resume/Restart/Level Select flow.
  - PlayMode: solver clear 후 Clear Result screen이 열리고 Next/Replay가 동작한다.
  - PlayMode: fail condition 후 Fail Result screen과 Retry가 동작한다.
  - Existing gameplay, art sprite, level select tests는 계속 통과해야 한다.
  - EditMode, `release-slice`, `qa-balance`, Android build/run, screenshot smoke를 순차 실행한다.
- Manual check:
  - clear/fail 순간이 gameplay footer보다 명확히 보이는지 확인한다.
  - pause/settings가 gameplay action처럼 오인되지 않는지 확인한다.

Implementation result:

- `ReleaseGameController`에 `Pause`, `ClearResult`, `FailResult` screen mode를 추가했다.
- Gameplay footer에 `Pause` 진입을 추가하고, pause screen에서 Resume, Restart,
  Level Select, Home, Sound/Haptic/Contrast toggle을 제공한다.
- Clear result는 completed orders, remaining moves, Next, Replay, Level Select를 제공한다.
- Fail result는 failure reason, Retry, Level Select를 제공한다.
- 카드/상태/operation/modifier/result copy 생성 책임을 `ReleaseGameplayCardRenderer`와
  `ReleaseResultSummary`로 분리했다.
- PlayMode 테스트는 Pause/Resume, Clear Result Next, Fail Result Retry를 검증한다.
- EditMode 테스트는 scene object 없이 renderer/result helper copy가 만들어지는지 검증한다.

Verification result:

- EditMode Unity Test Runner: 72 passed, 0 failed.
- PlayMode Unity Test Runner: 18 passed, 0 failed.
- `DreamLaundromat\release-slice.cmd`: `Valid=True`, `Errors=0`.
- `DreamLaundromat\qa-balance.cmd`: `Valid=True`, `AccessibilityValid=True`.
- Android batchmode target import check: exit code 0.
- `DreamLaundromat\screenshot-smoke.cmd -DeviceId emulator-5554`: passed.

Remaining manual gate:

- Result/Pause 실제 화면의 감각, spacing, copy tone은 사람이 기기에서 확인해야 한다.
- screenshot smoke는 이번 실행에서 현재 앱 상태의 gameplay 화면을 확인했으므로,
  result/pause visual taste 확인을 대체하지 않는다.

Completed implementation pass: Icon-first gameplay board:

- Problem:
  - 현재 gameplay board는 각 꿈/주문 카드가 상태 4축을 문장으로 풀어 쓰기 때문에 모바일
    퍼즐 게임이라기보다 QA/debug table처럼 보인다.
  - 플레이어가 매 턴 반복해서 보는 정보는 `Taint/Mood/Clarity/Stability` 설명문이 아니라
    꿈의 상태 조합, 주문 요구 조합, 선택 가능한 조작이어야 한다.
- Goal:
  - 기본 보드는 아이콘, 짧은 slot id, progress chip 중심으로 읽히게 한다.
  - 긴 설명은 선택/상태 패널에만 남겨서 학습 가능성과 접근성을 유지한다.
  - puzzle rule이나 level data는 바꾸지 않고 presentation만 바꾼다.
- Scope:
  - Dream card: `Dream 0 + Laundry tag + 4줄 상태 설명`을 `D0 + 4개 state chip + lock/empty chip`으로 축소한다.
  - Order card: `Request sheet` 설명을 `O0 + completed/target + requirement chip`으로 축소한다.
  - Storage card: `Shelf 0 + Stored dream + 상태 설명`을 `S0 + stored/empty chip + state chip`으로 축소한다.
  - Operation button: 변화 설명을 버튼에서 제거하고 `Wash/Soothe/Clarify/Settle`과 icon만 남긴다.
  - Header/guidance: 긴 level guidance와 player question은 gameplay 첫 화면에서 줄이고,
    선택 패널에 현재 선택/힌트를 보여준다.
  - Preview section은 compact chips와 selected detail panel로 바꾼다.
- Non-Goals:
  - 최종 애니메이션, drag/drop, juice feedback
  - prefab/UI Toolkit 전환
  - rules, solver, level pack 변경
  - 신규 외부/유료 asset 도입
- Verification:
  - PlayMode: gameplay screen에서 `D0`, `O0`, `S0`, `Focus`/`Selected` 계열의 짧은 UI가 보인다.
  - PlayMode: 기본 gameplay screen에서 `Taint:`, `Mood:`, `Clarity:`, `Stability:` 긴 축 설명은 직접 노출되지 않는다.
  - PlayMode: clear/fail/pause 기존 flow가 계속 통과한다.
  - EditMode: renderer helper가 compact label을 반환한다.
  - Unity Test Runner EditMode/PlayMode, release-slice, Android batchmode, screenshot smoke를 실행한다.
- Self-review:
  - 이 pass는 실제 재미를 새로 만들기보다 `퍼즐 오브젝트를 조작하는 화면`처럼 보이게 하는
    presentation debt 상환이다.
  - 텍스트를 너무 지우면 규칙 학습이 어려워질 수 있으므로 selected/focus panel에 상세를
    남긴다.
  - 완성 후에도 드래그, 애니메이션, 조작 피드백이 약하면 여전히 게임 감각이 부족할 수 있다.

Implementation result:

- Dream/Order/Storage card의 기본 본문에서 `Taint:`, `Mood:`, `Clarity:`, `Stability:`
  4줄 설명을 제거하고 slot id와 state/requirement chip icon 중심으로 바꿨다.
- Operation button에서는 변화 설명을 제거하고 icon + short action name만 남겼다.
- Gameplay header는 level title, goal, moves, guided prompt만 보여준다.
- Gameplay 중 sound/haptic/contrast settings row는 제거하고 Pause screen으로 역할을
  분리했다.
- Focus/Preview panel은 선택 정보와 preview를 compact text로만 보여준다.
- Footer message는 level guidance를 반복하지 않고 기본 상태에서는 `Ready` 중심의 짧은
  상태만 보여준다.

Verification result:

- PlayMode Unity Test Runner: 18 passed, 0 failed.
- `DreamLaundromat\release-slice.cmd`: `Valid=True`, `Errors=0`.
- `DreamLaundromat\qa-balance.cmd`: `Valid=True`, `AccessibilityValid=True`.
- Android batchmode target check: exit code 0.
- `DreamLaundromat\run.cmd -Build -DeviceId emulator-5554`: passed.
- `DreamLaundromat\screenshot-smoke.cmd -DeviceId emulator-5554`: passed.
- Manual screenshot check:
  - `DreamLaundromat/Logs/android-gameplay-after-icon-pass.png`
  - 새 gameplay 화면에서 card/order state가 icon chip 중심으로 표시되는 것을 확인했다.

Remaining design gap:

- 아직 section frame이 많고 버튼 조작이 click/tap command 중심이라 `게임판을 만지는 느낌`은
  약하다.
- 다음 visual/game-feel pass는 drag/drop 또는 tap-combo flow, 선택 halo, action preview
  animation, submit feedback, result transition animation을 다뤄야 한다.

Completed implementation pass: Game-feel V1:

- Goal:
  - 플레이어가 카드를 탭했을 때 `내가 지금 무엇을 잡고 있는지`, `어떤 주문과 맞는지`,
    `어떤 action이 변화를 만들지`를 즉시 볼 수 있게 한다.
  - 완전한 drag/drop이나 최종 animation 이전에, tap 기반 퍼즐 조작에서도 게임판 반응이
    느껴지게 한다.
- Scope:
  - selected dream/order/storage card에 halo를 표시한다.
  - 선택한 dream이 받을 수 있는 order 또는 선택한 order에 맞는 dream에 ready halo를 표시한다.
  - operation button에는 선택 dream 기준의 after-state chip preview를 붙인다.
  - submit button은 `Pick`, `No`, `Ready` 상태를 구분한다.
  - 성공/실패 action 후 footer message가 짧게 pulse되도록 한다.
  - Focus panel은 selected/target/preview 정보를 계속 compact하게 보여준다.
- Non-Goals:
  - drag/drop gesture
  - final animation timeline
  - particles, advanced haptics, SFX redesign
  - rules/solver/level data 변경
- Verification:
  - PlayMode: dream/order 선택 후 Focus panel에 `D0`, `O0`, `Ready`가 노출된다.
  - PlayMode: 기존 clear/fail/pause flow가 계속 통과한다.
  - EditMode, PlayMode, release-slice, qa-balance, Android build/install, screenshot smoke를 실행한다.
- Self-review:
  - 이 pass는 `게임판 반응성`을 만들기 위한 첫 단계다.
  - 여전히 section 기반 UI이므로 최종 game-feel은 drag/drop 또는 direct manipulation pass가 필요하다.

Implementation result:

- 선택한 dream/order/storage card와 선택 조합이 맞는 card에 halo를 표시한다.
- 선택한 dream에 적용 가능한 operation button에는 after-state chip preview를 붙인다.
- Submit button은 선택 전 `Pick`, 불일치 `No`, 제출 가능 `Ready`로 짧게 구분한다.
- 성공/실패 action 후 footer message가 짧게 pulse된다.
- 동적 UI pool 재사용 시 이전 child UI가 남지 않도록 reused object의 child를 detach/deactivate한다.

Completed implementation pass: Text Reduction V1:

- Goal:
  - gameplay board에서 읽어야 하는 영어 설명문을 줄이고, card/chip/icon/halo 중심으로
    퍼즐 상태를 읽게 한다.
  - Home, Level Select, Pause, Result처럼 메뉴 이해에 필요한 문구는 유지하되, 실제 puzzle
    board의 command-like copy를 줄인다.
- Scope:
  - `Dreams`, `Orders`, `Actions`, `Tools / Blocks`, `Playing`, `Guide:`처럼 설명적인 board
    copy를 제거하거나 `D`, `O`, `S`, `Focus` 수준으로 축소한다.
  - Tools/Blocks가 없는 라운드에서는 해당 section을 숨긴다.
  - storage 빈칸의 `Empty` 텍스트를 제거하고 slot id와 비어 있는 frame으로 상태를 읽게 한다.
  - Recall row는 stored dream을 선택했고 되돌릴 active slot이 있을 때만 표시한다.
  - operation button은 icon + marker(`W`, `So`, `Cl`, `Se`) 중심으로 표시한다.
  - footer selection message는 `D0`, `O0`, `S0`, `Pick D`처럼 짧게 표시한다.
- Non-Goals:
  - drag/drop 조작
  - 최종 art direction 또는 particle/SFX/haptic pass
  - menu screen 전체의 icon-only 전환
  - rules/solver/level data 변경
- Verification:
  - PlayMode: gameplay screen의 핵심 영역은 object 기준으로 존재하고, 긴 축 설명과
    legacy guide/status copy가 보이지 않는다.
  - PlayMode: D0/O0 선택 후 Focus panel과 Submit button이 `Ready` 상태를 보여준다.
  - Android screenshot: 기본 gameplay와 Ready 선택 상태에서 stale `Empty`/selection sentence가
    남지 않는다.
- Implementation result:
  - section title과 action label을 compact label로 줄였다.
  - 빈 Tools section과 불필요한 Recall row를 숨겼다.
  - storage 빈칸의 `Empty` copy를 제거했다.
  - footer selection/error copy를 compact code로 바꿨다.
  - release-slice CLI wrapper와 editor command가 공통 solver validation budget을 쓰도록 맞췄다.
- Verification result:
  - `DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`: 72 passed, 0 failed.
  - `DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`: 19 passed, 0 failed.
  - `DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`: `Valid=True`, `Errors=0`.
  - `DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`: `Valid=True`, `AccessibilityValid=True`.
  - Android batchmode target check: exit code 0.
  - `DreamLaundromat\run.cmd -Build -DeviceId emulator-5554 -BootTimeoutSeconds 240 -BuildTimeoutSeconds 1200`: passed.
  - `DreamLaundromat\screenshot-smoke.cmd -DeviceId emulator-5554`: passed.
  - Manual screenshot check:
    - `DreamLaundromat/Logs/android-text-reduction-v1-final.png`
    - `DreamLaundromat/Logs/android-ready-v1-final.png`
- Self-review:
  - 텍스트는 줄었지만 layout은 여전히 section frame 중심이다.
  - 다음 pass는 drag/drop 또는 tap-combo direct manipulation, card movement animation,
    footer/menu icon treatment를 별도로 다루는 편이 좋다.

### RUI-013 - Multi-Resolution Screenshot Checks

- Outputs:
  - screenshot review artifacts
  - optional screenshot batch script
- Work:
  - 1080x1920 baseline
  - 720x1280 small screen check
  - long/tall screen check
  - text clipping and overlap notes
- Verification:
  - Android screenshot smoke
  - optional batch artifact existence check
- Done criteria:
  - 주요 화면비에서 출시 후보 UI가 깨지지 않는다.

### RUI-014 - QA Report And Review Checklist Update

- Outputs:
  - updated QA report sections
  - release UI manual review checklist
- Work:
  - level visual checklist를 screen flow까지 확장
  - result/settings/level select review 항목 추가
  - screenshot artifact path를 PR summary에 쓰기 쉽게 정리
- Verification:
  - `.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`
- Done criteria:
  - PR review에서 무엇을 봐야 하는지 명확하다.

### RUI-015 - Full Verification Gate

- Outputs:
  - verification summary
- Work:
  - 필수 자동 검증 순차 실행
  - Android build/run/screenshot smoke
  - manual gate 정리
- Verification:
  - 아래 Verification And Test Plan 실행
- Done criteria:
  - 출시 UI V1 PR을 review할 수 있는 상태다.

## PR Plan

기본 PR:

- `game/release-ui-design-v1`

사용자가 현재 큰 PR 흐름을 유지하고 싶다면 같은 브랜치의 다음 묶음으로 진행할 수 있다.
다만 PR 본문에서는 `RUI-001`부터 `RUI-015`까지 sub-gate를 분리한다.

권장 sub-gate:

1. design token and presenter split
2. app flow screens
3. gameplay redesign
4. result/pause/settings
5. screenshot and QA verification

merge 정책:

- Codex는 merge하지 않는다.
- protected branch push도 하지 않는다.
- PR 생성과 review까지만 수행한다.

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

- changed text files trailing whitespace check
- `.meta` missing check under `DreamLaundromat/Assets`
- asset path convention check under `DreamLaundromat/Assets/_Project/Art/UI`
- changed bitmap asset size report and LFS review note
- screenshot non-empty check
- focused activity check
- logcat fatal/crash pattern check
- accessibility contrast/touch target audit
- level select tile descriptor tests
- result screen PlayMode tests

수동 검증:

- Title/Home 첫 인상
- Title/Home background가 게임 정체성을 전달하는지
- Level Select가 30개 레벨을 부담 없이 보여주는지
- Gameplay card readability
- state/operation/tool/obstacle icon이 실제 크기에서 구분되는지
- 실패 이유가 납득되는지
- clear result가 보상처럼 느껴지는지
- one-hand reachability
- 실제 Android 기기 haptic/audio 감각
- 작은 화면/긴 화면 text clipping
- 실제 재미, 반복 피로도, visual taste

## CLI And Manual Boundary

Codex가 CLI에서 할 수 있는 것:

- UI presenter/helper 구현
- programmatic UI 구조 변경
- generated/simple asset 생성과 `.meta` 확인
- EditMode/PlayMode tests
- Unity batchmode validation
- Android build/install/launch smoke
- screenshot smoke
- QA report 갱신
- PR 생성과 PR review

사람이 해야 하는 것:

- PR merge
- protected branch push
- 실제 기기 장시간 조작감 판단
- 최종 visual taste 판단
- 최종 SFX/haptic 감각 판단
- 유료 asset 구매 여부 결정
- Google Play listing, signing secret, release AAB 승인

## Risks

### R1 - 출시 UI 범위가 너무 커질 수 있음

대응:

- Title/Home, Level Select, Gameplay, Pause, Result까지만 V1으로 제한한다.
- store asset, app icon final, monetization은 제외한다.

### R2 - `ReleaseGameController`가 더 커질 수 있음

대응:

- 첫 milestone에서 presenter/renderer 분리를 먼저 한다.
- scene UI를 바꾸기 전에 pure descriptor tests를 만든다.

### R3 - 예쁜 UI가 퍼즐 판독성을 해칠 수 있음

대응:

- 장식보다 state readability를 우선한다.
- color-only 표현을 금지한다.
- screenshot review와 PlayMode text/icon presence를 유지한다.

### R4 - Programmatic UI만으로 출시 감각이 부족할 수 있음

대응:

- first pass는 programmatic UI로 구조와 디자인 token을 검증한다.
- prefab/serialized reference 전환은 별도 gate로 판단한다.

### R5 - Asset이 repo 용량과 라이선스 문제를 만들 수 있음

대응:

- paid asset은 기본 제외한다.
- bitmap asset은 크기와 LFS 필요 여부를 확인한다.
- generated asset도 `.meta`와 출처/용도 note를 남긴다.

### R6 - Screenshot smoke가 visual quality를 증명한다고 착각할 수 있음

대응:

- screenshot smoke는 crash/focus/nonblank/layout sanity용으로만 둔다.
- visual taste와 재미는 manual gate로 분리한다.

### R7 - Level Select가 content pipeline을 앞질러 과해질 수 있음

대응:

- 30개 fixed level pack 기준으로 단순 grid/list를 만든다.
- chapter map이나 event surface는 제외한다.

## Deferred Or Out Of Scope

game-local backlog:

- complete prefab/UI Toolkit migration
- final illustration pack
- final app icon and store screenshots
- localization
- monetization/analytics/privacy flow
- cloud save
- challenge/event visual variants
- full hint UI
- 100+ launch level visual batch review

repo-level TODO 대상은 아니다. 현재 항목들은 특정 게임의 출시 UI/디자인 backlog다.

## First Implementation Step

최초 추천 작업은 `RUI-001`과 `RUI-002`를 함께 진행하는 것이었다.

구체적으로:

1. 최신 Android screenshot을 baseline으로 두고 UI 문제를 문서/QA report에 기록한다.
2. `ReleaseVisualStyle`을 출시 UI용 design token set으로 확장한다.
3. token completeness와 contrast/touch target EditMode 테스트를 추가한다.

이유:

- design token 없이 화면부터 바꾸면 색상/spacing/button style이 흩어진다.
- baseline screenshot이 있어야 출시 UI V1의 개선을 검토할 수 있다.
- token test가 먼저 있으면 이후 Title/Home, Level Select, Result 화면이 같은 규칙을 쓴다.

## Implementation Status

2026-06-18 기준 출시 UI V1의 CLI 구현과 Android screenshot 검증은 완료했다.

완료된 항목:

- Title/Home, Level Select, Gameplay, Pause, Result 화면 흐름 구현
- `ReleaseVisualStyle` 기반 color/touch/contrast token과 UI presence test 보강
- Dream/Order/Storage card object treatment와 state/requirement chip 적용
- operation/submit/store/footer/navigation 버튼 surface PNG 적용
- title/gameplay/level select background PNG 적용
- `ReleaseUiArtCatalog`와 `ReleaseUiArtGenerator` 기반 asset catalog 구성
- generated asset source note와 `.meta` 보존 검증
- Android screenshot smoke와 level screenshot batch로 대표 화면 확인

최근 검증 결과:

```powershell
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
# Valid=True, Levels=30, Errors=0, Warnings=0

.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
# Total=85 Passed=85 Failed=0 Skipped=0

.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
# Total=20 Passed=20 Failed=0 Skipped=0

.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900
# Android debug build/install/launch completed with exit code 0

.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
# Android screenshot smoke passed

.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0 -TimeoutSeconds 300 -NoAutoStart
# Android level screenshot batch passed
```

남은 manual gate:

- 실제 visual taste가 출시 후보 수준인지 사람이 판단해야 한다.
- 30개 level을 직접 플레이하며 UI가 퍼즐 판단을 방해하지 않는지 확인해야 한다.
- 실제 Android 기기에서 touch comfort, haptic/audio 감각, 작은 화면/긴 화면 가독성을
  확인해야 한다.

다음 추천 작업:

1. PR 생성/리뷰 전 최신 screenshot과 검증 결과를 PR body에 포함한다.
2. 30레벨 수동 플레이테스트 결과를 `RELEASE_MANUAL_PLAYTEST_CHECKLIST.md`에 기록한다.
3. 결과에 따라 `ReleaseGameController` 분리 또는 UI polish 2차를 선택한다.

## Open Decisions

현재 구현을 막는 결정은 없다. 아래 기본값으로 진행한다.

- Art style 기본값: flat/vector-like base with light texture
- Asset 기본값: code-native UI와 generated bitmap/icon 조합
- First asset priority 기본값: Title/Home background, Level Select background,
  Dream/Order frame, state/operation icons
- Asset folder 기본값: `DreamLaundromat/Assets/_Project/Art/UI/`
- Paid asset 기본값: 사용하지 않음
- Free third-party asset 기본값: license 확인 전 repository에 추가하지 않음
- Generated asset 기본값: `SourceNotes/`에 prompt/source note 기록
- UI architecture 기본값: programmatic UI 유지, presenter/renderer 분리 우선
- Level Select 기본값: compact grid/list, chapter map 제외
- PR 기본값: 하나의 PR 안에서 sub-gate를 분리

나중에 사용자가 결정하면 좋은 항목:

- generated bitmap background/icon set의 최종 스타일
- 무료 asset을 실제로 도입할지 여부
- prefab 기반 UI로 전환할 시점
- 최종 art style을 painterly/card-like로 강화할지 여부
- launch 전에 app icon/store screenshot까지 같은 PR에서 다룰지 여부

## Self-Review

검토 기준:

- `docs/IMPLEMENTATION_PLANNING.md` 필수 섹션을 포함했다.
- Game planning checklist의 Core Fun, Pillars, Rules, Grammar, Progression,
  Content Production, UX, Satisfaction, World, Prototype Criteria를 모두 반영했다.
- 기존 `VISUAL_UX_DIRECTION_PLAN.md`와 중복되는 자동화 항목은 계승하되, 이번 문서는
  출시 UI 화면 흐름과 design system에 초점을 맞췄다.
- 출시 후보 UI에 필요한 실제 art/image asset 목록, 저장 위치, 생성/무료/유료 asset
  정책, 용량/LFS 검토 기준을 포함했다.
- 자동 검증과 manual gate를 분리했다.
- user decision이 필요한 항목은 기본값과 함께 Open Decisions에 분리했다.
- `docs/TODO.md`에 넣을 공통 인프라 항목과 game-local backlog를 구분했다.

자체 수정한 점:

- 처음에는 gameplay 화면 redesign만으로 좁힐 수 있었지만, 출시 후보 UI에는
  Title/Home, Level Select, Pause, Result가 필요하므로 scope에 포함했다.
- final art asset 제작과 store 준비는 범위를 키우므로 Non-Goals로 분리했다.
- 단순히 "아이콘이 필요하다" 수준에서 멈추지 않고, Priority 1-4 asset set과
  `Assets/_Project/Art/UI/` 폴더 정책으로 구현 단위를 구체화했다.
- "진짜 출시 가능한 디자인"이라는 목표가 감각 평가를 포함하므로, 자동 검증만으로
  완료라고 주장하지 않고 manual gate를 명확히 남겼다.

남은 위험:

- 실제 visual taste는 사람이 판단해야 한다.
- programmatic UI로 출시 수준까지 갈 수 있는지는 구현 중 다시 판단해야 한다.
- icon/asset 도입이 시작되면 repo 용량과 LFS 기준을 다시 확인해야 한다.
