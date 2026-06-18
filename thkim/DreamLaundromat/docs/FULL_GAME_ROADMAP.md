# Full Game Roadmap

## Summary

이 문서는 `DreamLaundromat`을 현재의 `Dynamic Puzzle Lab` 프로토타입에서
출시 가능한 모바일 퍼즐 게임으로 끌고 가기 위한 전체 로드맵이다.

목표는 당장 모든 기능을 한 PR에 구현하는 것이 아니다. 먼저 끝까지 가는 경로를
명확히 고정하고, 이후 구현 PR들이 이 경로에서 벗어나지 않도록 기준을 제공하는
것이다.

로드맵의 중심 질문:

- 이 게임이 실제로 어떤 재미를 제공해야 하는가?
- 지금 만든 `Dynamic Puzzle Lab` 엔진을 실제 유저용 게임으로 어떻게 바꿀 것인가?
- 많은 레벨을 어떻게 만들고, 검증하고, 선별할 것인가?
- 모바일 출시까지 필요한 UX, 콘텐츠, 품질, 빌드, 스토어 준비를 어떤 순서로 처리할
  것인가?

## Planning References

- [DreamLaundromat implementation plan](PLAN.md)
- [Dynamic Puzzle Lab implementation plan](DYNAMIC_PUZZLE_LAB_PLAN.md)
- [Modifier Engine implementation plan](MODIFIER_ENGINE_PLAN.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)
- [Unity project conventions](../../docs/CONVENTIONS.md)
- [Mobile Android guidance](../../docs/MOBILE_ANDROID.md)
- [Dream Laundromat concept](../../../concepts/puzzle/dream-laundromat.md)
- [Dream Laundromat planning index](../../../concepts/puzzle/dream-laundromat-planning/README.md)

## Current Baseline

이미 완료된 기반:

- `DynamicRoundDefinition`, `DynamicRoundState`, seeded stream, storage, sample round
  구조
- `DynamicRulesEngine`, hard validator, design validator
- BFS solver, replay verifier, state hash
- stage recipe, candidate generator, batch simulation report
- `PreviewSwap` item, `LockedActiveDreamSlot` obstacle, modifier pipeline
- `DynamicLabDebugGame`, `DynamicLabDebug.unity` debug play surface
- EditMode/PlayMode 테스트와 `dynamic-lab.cmd` batch 검증 스크립트
- `ReleaseGameplaySlice.unity` 제품형 gameplay scene
- 30개 release level pack, guided tutorial data, local progression/settings save
- item/obstacle V1, modifier impact validation, QA/balance report
- Android build/run, screenshot smoke, level screenshot batch wrapper
- release UI V1: Home, Level Select, Gameplay, Pause, Result 화면과 generated UI
  surface/background assets
- Alpha readiness UI pass: gameplay layout 압축, action dock compact label, modifier
  compact label, operation marker label

현재 상태의 의미:

- 퍼즐 엔진의 가능성은 확인했다.
- 실제 플레이 가능한 제품형 release slice와 Android 자동 검증 경로가 생겼다.
- gameplay UI의 큰 text-heavy 문제는 Alpha readiness branch에서 상당 부분 줄였다.
- 아직 실제 재미, 난이도, 조작감, visual taste는 사람이 30레벨을 플레이하며
  판정해야 한다.
- 다음 단계는 Phase 14 `Alpha Build`로 바로 넘어가기 전에 PR review와
  `RELEASE_PLAYTEST_RESULTS.md` 기반 수동 playtest 결과를 바탕으로 레벨/UX를 조정하는
  것이다.

## Product Constraints

- Platform: Android mobile first
- Orientation: Portrait
- Input: one-hand touch
- Genre: puzzle
- Session: 짧은 레벨 단위, 빠른 재시도
- Release ambition: 실제 출시 가능성을 고려한 구조
- Development style: CLI 가능한 작업은 script와 batchmode로 자동화
- Human-only boundary: PR merge, 실제 기기 조작감 평가, 스토어 계정/서명 비밀 관리

## Design Thesis

`DreamLaundromat`의 핵심 재미는 꿈을 무조건 좋은 상태로 만드는 데 있지 않다.

재미는 제한된 처리 공간 안에서 꿈의 상태를 주문에 맞게 조율하고, 어떤 꿈을 어떤
주문에 쓸지 판단하며, preview와 랜덤 입력을 읽고 다음 몇 수를 계획하는 데서 나온다.

출시형 게임은 다음 감각을 제공해야 한다.

- 현재 상태를 읽는 재미
- 변환 순서를 계획하는 재미
- 공간이 부족할 때 선택을 압축하는 재미
- item과 obstacle이 정답 버튼이나 hidden trap이 아니라 새로운 판단을 만드는 재미
- 실패해도 왜 실패했는지 이해되고 바로 다시 시도하고 싶은 감각

## Core Rules Snapshot

현재 출시형 규칙의 기준은 `Dynamic Puzzle Lab`이다.

핵심 entities:

- `Dream`: `taint`, `mood`, `clarity`, `stability` 상태 축을 가진다.
- `Order`: 특정 상태 조합을 요구하고, 조건을 만족한 꿈을 제출받는다.
- `Active Dream Slot`: 현재 조작 가능한 꿈이 놓이는 공간이다.
- `Active Order Slot`: 현재 처리해야 하는 주문이 놓이는 공간이다.
- `Preview`: 다음 꿈/주문을 미리 보여 주어 랜덤 입력을 공정하게 만든다.
- `Storage`: 지금 당장 쓰지 않을 꿈을 임시로 보관하는 공간이다.
- `Modifier`: item과 obstacle을 모두 표현한다.

핵심 actions:

- `ApplyOperation`: `Wash`, `Soothe`, `Clarify`, `Settle` 같은 deterministic
  operation으로 꿈 상태를 바꾼다.
- `SubmitDream`: 주문 조건을 만족한 꿈을 제출한다.
- `StoreDream` / `RecallDream`: storage를 이용해 순서와 공간을 조정한다.
- `UseItem`: charge를 가진 item modifier를 직접 사용한다.

clear/failure 기준:

- 목표 주문 수를 완료하면 clear된다.
- 이동 수가 떨어지거나, 더 이상 꿈이 없거나, 유효한 action이 없으면 fail된다.
- 실패 이유는 UI에서 player가 이해할 수 있는 형태로 노출되어야 한다.

규칙 경계:

- operation 결과에는 random success/fail을 넣지 않는다.
- hidden obstacle과 설명 없는 random 발동은 launch 기본 규칙에서 제외한다.
- full hint, booster economy, liveops modifier는 launch core가 안정된 뒤 판단한다.

## Game Pillars

### One-Hand Clarity

모든 주요 정보는 세로 화면에서 한손으로 읽고 조작할 수 있어야 한다.

영향:

- 꿈 상태 4축은 색상만으로 구분하지 않고 icon, shape, text를 조합한다.
- tap target은 모바일 기준으로 충분히 커야 한다.
- action 후보, 불가능한 action, 변환 결과가 즉시 보인다.

### Deterministic Actions, Variable Inputs

행동 결과는 예측 가능해야 하고, 새로움은 꿈/주문/preview/레벨 배치에서 나온다.

영향:

- operation 자체에는 random success/fail을 넣지 않는다.
- generator와 fixed level pack은 seed와 validator로 재현 가능해야 한다.
- player가 모르는 hidden random 발동은 출시형 기본 규칙에서 제외한다.

### Puzzle From Assignment Before Space

공간 관리는 중요하지만, 게임이 단순 공간정리로 축소되면 안 된다.

영향:

- 각 라운드는 "어떤 꿈을 어떤 주문에 쓸 것인가"라는 assignment 질문을 포함해야 한다.
- storage pressure는 assignment 결정을 압박하는 역할이어야 한다.
- design validator는 direct-submit, 단순 operation-submit cadence, 낮은 order
  competition을 계속 경고해야 한다.

### Preview Makes Randomness Fair

랜덤 입력은 매번 다른 상황을 만들되, player가 미리 보고 판단할 수 있어야 한다.

영향:

- dream/order preview는 핵심 UX 정보다.
- preview가 너무 적으면 운빨로 느껴지고, 너무 많으면 계산 피로가 된다.
- 레벨별 preview count는 난이도 knob로 관리한다.

### Small Rules, Many Situations

출시 초반은 규칙 수를 적게 유지하고, 레벨 상황의 조합으로 다양성을 만든다.

영향:

- item/obstacle은 한 번에 많이 추가하지 않는다.
- 새 규칙은 chapter나 level arc 단위로 하나씩 소개한다.
- 레벨 생산 파이프라인은 rule explosion보다 recipe와 metric 조정을 우선한다.

### Fast Retry With Honest Failure

실패는 납득 가능해야 하고, 재시도는 빠르게 이어져야 한다.

영향:

- restart, undo, failure reason은 출시 slice에 반드시 포함한다.
- 실패 UI는 벌점보다 "왜 막혔는지"를 알려주는 역할을 한다.
- hint는 출시 전반부에는 제한적으로 다루고, full hint는 후속 phase에서 판단한다.

## World Direction

Tone:

- 조용하고 이상하지만 불쾌하지 않은 꿈 세탁소.
- cozy함이 기본이지만, 꿈의 상태와 주문에는 약간의 기묘함이 있어야 한다.

Player role:

- player는 꿈을 고치는 영웅이 아니라, 손님의 꿈을 읽고 알맞은 상태로 정리해 주는
  야간 세탁소 운영자다.

Visual metaphors:

- 꿈 카드는 세탁물처럼 다루되, 감정과 기억의 조각처럼 보인다.
- `Wash`, `Soothe`, `Clarify`, `Settle`은 실제 세탁소 도구와 꿈 치료의 중간
  은유로 표현한다.
- item은 편법이 아니라 세탁소의 특수 도구처럼 보여야 한다.
- obstacle은 함정이 아니라 고장 난 기계, 예약된 슬롯, 손님의 까다로운 조건처럼
  보이게 한다.

Narrative limits:

- 긴 스토리 컷신보다 레벨, 주문, 고객, visual feedback으로 세계를 전달한다.
- 세계관은 규칙 기억을 도와야 하며, 규칙을 숨기거나 복잡하게 만들면 안 된다.

## Full Roadmap

### Roadmap Dependency

- Phase 1은 실제 구현을 시작하기 전의 상세 계획이다.
- Phase 2-3은 제품형 gameplay scene과 interaction을 만든다.
- Phase 4-5는 slice를 실제 레벨로 채우는 level data pipeline과 level pack이다.
- 따라서 `Release Gameplay Slice` 구현 계획은 Phase 1-5를 한 덩어리로 다뤄야
  한다. 다만 첫 PR에서는 Phase 2-3을 먼저 구현하고, Phase 4-5는 최소 fixed level
  path부터 붙일 수 있다.
- Phase 6 이후는 slice가 playable하고 검증 가능해진 뒤 확장한다.

### Phase 0 - Roadmap And Planning Lock

목표:

- 출시까지의 전체 경로를 문서화한다.
- 기존 prototype 계획, Dynamic Lab 계획, modifier 계획을 하나의 방향으로 묶는다.

산출물:

- `DreamLaundromat/docs/FULL_GAME_ROADMAP.md`
- 다음 PR의 구체 구현 플랜: `DreamLaundromat/docs/RELEASE_GAMEPLAY_SLICE_PLAN.md`

검증:

- 로드맵이 Core Fun, Rules, Puzzle Grammar, Level Progression, Content Production,
  UX, Satisfaction, World, Prototype Criteria를 모두 다루는지 자체 검토한다.
- 현재 구현 상태와 다음 구현 범위가 연결되는지 확인한다.

Exit Criteria:

- 출시까지 필요한 phase와 gate가 명확하다.
- 바로 다음 구현 PR의 주제가 모호하지 않다.

### Phase 1 - Release Gameplay Slice Plan

목표:

- debug surface가 아니라 실제 유저가 플레이할 첫 제품형 slice를 정의한다.

범위:

- 10-15개 레벨
- 실제 gameplay scene
- 라운드 시작, action, clear/fail, restart, next level
- 기본 item/obstacle 표시
- 최소 tutorial
- slice-local fixed level data path
- Android batchmode/build 검증

Non-Goals:

- 전체 progression map
- monetization
- analytics
- liveops
- 최종 art/audio

산출물:

- `RELEASE_GAMEPLAY_SLICE_PLAN.md`
- PR task breakdown
- verification and manual QA checklist

검증:

- 구현 플랜에 자동 테스트, Unity batchmode, Android build, manual playtest 항목이 포함된다.
- user decision이 필요한 사항을 문서 하단에 분리한다.

Exit Criteria:

- 구현자가 자리를 비워도 CLI 가능한 작업은 진행할 수 있을 정도로 task가 쪼개져 있다.
- 수동 확인이 필요한 작업은 자동 작업과 분리되어 있다.

### Phase 2 - Product Gameplay Scene

목표:

- `DynamicLabDebugGame`을 대체할 실제 게임용 scene과 flow를 만든다.

산출물:

- `Gameplay.unity` 또는 명확한 출시 slice scene
- `GameFlowController`
- `RoundPresenter`
- `LevelSessionController`
- clear/fail/restart/next UI

구현 방향:

- `DynamicRulesEngine`은 그대로 gameplay rules의 중심으로 둔다.
- debug UI의 즉석 버튼 생성 방식은 제품 UI로 옮기지 않는다.
- UI는 serialized references를 우선하고 scene-wide lookup을 피한다.

검증:

- PlayMode: 첫 레벨 로드, solver solution replay, clear/fail/restart/next
- EditMode: session state transition
- Android batchmode import

Exit Criteria:

- debug scene 없이도 하나의 level을 실제 game flow로 플레이할 수 있다.

### Phase 3 - Core Mobile Interaction

목표:

- 한손 터치 기준으로 꿈, 주문, operation, item, storage를 읽고 조작할 수 있게 한다.

산출물:

- dream card UI
- order card UI
- operation controls
- item controls
- preview lane
- storage/active slot UI
- invalid action feedback

구현 방향:

- 탭 기반 선택을 기본으로 한다.
- drag는 나중에 추가할 수 있지만 첫 release slice의 필수 조건으로 두지 않는다.
- 선택한 꿈에서 가능한 action과 불가능한 action을 구분해서 보여준다.
- operation 결과 preview를 표시한다.

검증:

- PlayMode: 주요 UI element 존재와 action dispatch
- Manual: 360x800, 1080x1920, safe area, 한손 조작감
- Android emulator: 첫 화면 표시와 기본 touch flow

Exit Criteria:

- player가 debug 용어를 몰라도 한 라운드를 시작하고 끝낼 수 있다.

### Phase 4 - Level Data Pipeline V1

목표:

- generator 결과를 사람이 선별하고 fixed level pack으로 저장하는 흐름을 만든다.

산출물:

- level definition asset 또는 JSON-like data path
- level pack list
- candidate report archive policy
- level validation batch script
- fixed seed policy

구현 방향:

- 출시용 레벨은 runtime random generator에 완전히 맡기지 않는다.
- generator는 후보 생산 도구이고, 실제 출시 레벨은 사람이 선별한 fixed pack이다.
- 각 레벨은 design intent, player question, risk note를 가진다.

검증:

- Batch: 모든 fixed level hard validation 통과
- Solver: solvable, min moves 산출
- Design validator: warning을 report에 남기고 사람이 승인/보류 판단
- Regression: level pack 변경 시 전체 검증이 script 하나로 가능

Exit Criteria:

- level pack에 들어간 레벨은 재현 가능하고 자동 검증 가능하다.

### Phase 5 - Level Pack V1

목표:

- 출시 slice에 필요한 초반 레벨 묶음을 만든다.

권장 범위:

- Slice: 10-15 levels
- Alpha: 30 levels
- Soft launch candidate: 80-120 levels
- Launch candidate: 최소 100 levels, 가능하면 150 levels 이상

Level Arc:

1. Level 1-3: 읽기, submit, basic operation
2. Level 4-7: `Wash`, `Soothe`, `Clarify`, `Settle`의 차이
3. Level 8-12: preview와 stream timing
4. Level 13-20: storage pressure
5. Level 21-30: first item/obstacle
6. Level 31-50: assignment + obstacle 조합
7. Level 51-80: chapter variation
8. Level 81+: challenge mix, optional hard levels, event 후보

검증:

- 모든 level solvable
- min moves, branching, repeated operation, direct submit, preview relevance metric 기록
- 사람이 최소 1회 playthrough
- 막힘 지점과 튜토리얼 부족 지점 기록

Exit Criteria:

- 초반 30레벨이 rule unlock 순서를 자연스럽게 가르친다.

### Phase 6 - Item And Obstacle V1 Expansion

목표:

- modifier engine이 실제 게임성을 넓히는지 검증한다.

원칙:

- item은 정답 버튼이 아니라 계획 변경 도구여야 한다.
- obstacle은 hidden trap이 아니라 보이는 제약이어야 한다.
- modifier는 solver, generator, replay, hash, UI가 모두 이해해야 한다.

후보:

- `PreviewSwap`: 이미 구현됨. preview 순서를 바꿔 stream timing을 조정한다.
- `SlotLock`: 이미 구현됨. 특정 active dream slot 사용을 제한한다.
- `OrderPin`: 특정 주문이 일정 turn 동안 고정되어 assignment pressure를 만든다.
- `DreamRefresh`: 제한적으로 active dream 하나를 preview 뒤로 보내지만 move/charge 비용이 있다.
- `SoftBlock`: 특정 operation을 한 번만 막고 해제되어 우회 순서를 요구한다.

검증:

- modifier별 hard validation
- solver replay
- generator acceptance metric
- item required / optional 구분
- UI 표시와 tooltip

Exit Criteria:

- 최소 3개 modifier가 fixed level에서 실제로 의미 있는 선택을 만든다.

### Phase 7 - Tutorial And Onboarding

목표:

- 첫 플레이어가 규칙을 문서 없이 이해하게 한다.

산출물:

- tutorial step data
- guided input lock
- contextual message
- first 10 level tutorial tags

구현 방향:

- 긴 설명을 피하고 실제 조작으로 가르친다.
- 한 레벨에 하나의 새 개념만 소개한다.
- 튜토리얼은 core game flow를 우회하지 않아야 한다.

검증:

- PlayMode: tutorial step progression
- Manual: 첫 10레벨을 처음 보는 사람이 막히는 지점 기록

Exit Criteria:

- 첫 10분 안에 player가 꿈 상태, 주문, operation, preview를 이해한다.

### Phase 8 - Progression And Save

목표:

- level clear 상태와 다음 레벨 진입을 보존한다.

산출물:

- local save model
- level unlock state
- clear record
- settings save
- save migration version

구현 방향:

- 첫 출시는 local save만 사용한다.
- cloud save는 release 이후 또는 별도 phase로 둔다.
- save schema는 version을 가진다.

검증:

- EditMode: save serialize/deserialize, migration
- PlayMode: clear 후 next level unlock
- Manual: app restart 후 진행도 유지

Exit Criteria:

- player가 앱을 껐다 켜도 진행도가 유지된다.

### Phase 9 - Art Direction V1

목표:

- 꿈 세탁소 컨셉이 규칙 이해를 돕도록 visual language를 만든다.

산출물:

- dream card visual system
- state icon set
- machine/order/storage/item/obstacle icon set
- chapter palette
- UI style guide

구현 방향:

- 상태 축은 icon + label + color 조합으로 표시한다.
- decorative art보다 state readability를 우선한다.
- 무료/유료 asset은 라이선스와 repo 저장 정책 확인 후 사용한다.
- 대형 bitmap/audio는 `.gitattributes`와 Git LFS 기준을 따른다.

검증:

- Mobile screenshot review
- colorblind/low contrast basic check
- asset size and LFS check

Exit Criteria:

- 스크린샷만 봐도 꿈, 주문, 기계, item, obstacle의 역할이 구분된다.

### Phase 10 - Audio, Haptics, Juice

목표:

- 반복 action이 피곤하지 않고, 성공/실패가 명확하게 느껴지게 한다.

산출물:

- action feedback timing table
- basic SFX set
- haptic pattern table
- animation timing constants

구현 방향:

- action feedback은 짧고 즉각적이어야 한다.
- solver/logic과 animation timing을 결합하지 않는다.
- haptic은 Android device capability를 고려해 optional로 둔다.

검증:

- Manual: action latency, feedback clarity
- Android: haptic fallback, audio volume balance

Exit Criteria:

- operation, submit, clear, fail, item use가 감각적으로 구분된다.

### Phase 11 - UX Polish And Accessibility

목표:

- 출시 가능한 모바일 UI 품질을 만든다.

산출물:

- safe area handling
- scalable layout
- readable text sizing
- touch target audit
- color/icon redundancy
- pause/settings

검증:

- Multiple resolution screenshots
- Android emulator and physical device check
- Manual: one-hand play, text clipping, overlap, landscape prevention

Exit Criteria:

- 주요 Android 화면비에서 UI 겹침 없이 플레이 가능하다.

### Phase 12 - Tooling And Automation

목표:

- 반복 검증과 빌드를 CLI 중심으로 안정화한다.

산출물:

- `test.cmd`
- `dynamic-lab.cmd`
- level pack validation script
- Android build script
- emulator smoke script
- log collection script

검증:

- clean checkout에서 script 실행
- missing XML/log/build artifact failure 처리
- CI 도입 가능성 검토

Exit Criteria:

- 매 PR마다 최소 검증 세트가 명령 몇 개로 재현된다.

### Phase 13 - QA And Balance Loop

목표:

- 레벨 품질을 수치와 수동 플레이로 같이 관리한다.

산출물:

- level review checklist
- balance spreadsheet or structured report
- difficulty band labels
- known issue list
- playtest notes

검증 항목:

- solvable
- min moves
- average branching
- repeated operation risk
- preview relevance
- item meaningfulness
- obstacle fairness
- manual frustration notes

Exit Criteria:

- 각 level이 왜 포함됐는지 설명 가능하다.

### Phase 14 - Alpha Build

목표:

- 내부 테스트 가능한 제품형 빌드를 만든다.

범위:

- 30 levels
- tutorial
- progression/save
- core art direction
- basic audio/feedback
- Android debug build

검증:

- Full level pack validation
- EditMode/PlayMode pass
- Android build pass
- emulator smoke
- physical device manual play

Exit Criteria:

- 내부 플레이어가 처음부터 30레벨까지 진행 가능하다.

### Phase 15 - Beta / Soft Launch Candidate

목표:

- 제한된 외부 테스트가 가능한 상태를 만든다.

범위:

- 80-120 levels
- difficulty curve
- settings
- crash-free startup
- privacy/policy decision
- store asset draft

결정 사항:

- analytics 사용 여부
- ads/IAP 사용 여부
- privacy policy 필요 범위
- cloud save 여부

검증:

- Android release-like build
- startup time and memory smoke
- device compatibility sample
- first session UX test

Exit Criteria:

- 외부 사용자에게 설치 파일을 줘도 기본 품질 문제가 바로 드러나지 않는다.

### Phase 16 - Release Readiness

목표:

- Google Play 출시 후보를 준비한다.

산출물:

- package name
- versioning policy
- app icon
- splash
- AAB build script
- signing process outside repo
- store description
- screenshots
- privacy policy
- release checklist

주의:

- Google Play 요구사항은 출시 직전에 공식 문서 기준으로 다시 확인한다.
- keystore, password, API key, signing config secret은 repo에 넣지 않는다.
- release build 설정은 debug build와 분리한다.

검증:

- release AAB build
- signing secret exclusion check
- Android permissions review
- store listing dry run

Exit Criteria:

- Play Console에 올릴 수 있는 후보 artifact와 문서가 준비된다.

### Phase 17 - Launch Candidate

목표:

- 출시 판단 가능한 최종 후보를 만든다.

검증:

- 모든 automated tests pass
- full level pack validation pass
- Android release AAB build pass
- emulator and physical device smoke
- first 30 levels manual pass
- known issue triage
- store policy checklist

Exit Criteria:

- 남은 issue가 출시 차단인지 후속 패치인지 판단되어 있다.

### Phase 18 - Launch And Patch Loop

목표:

- 출시 후 치명 문제 대응과 콘텐츠 확장을 준비한다.

산출물:

- patch branch policy
- hotfix checklist
- post-launch level pack plan
- event content decision
- player feedback intake

검증:

- hotfix build path rehearsal
- save compatibility test
- level data backward compatibility

Exit Criteria:

- 출시 이후에도 main 개발과 hotfix가 충돌하지 않는 운영 방식이 있다.

## PR Roadmap

권장 PR 순서:

1. `roadmap/full-game`: full game roadmap
2. `plan/release-gameplay-slice`: release gameplay slice implementation plan
3. `game/release-gameplay-scene`: product gameplay scene and flow
4. `game/mobile-interaction-v1`: one-hand interaction and readable game UI
5. `game/level-data-pipeline-v1`: fixed level pack and validation pipeline
6. `game/level-pack-v1`: first 15-30 level pack
7. `game/modifiers-v1`: 1-3 additional meaningful modifiers
8. `game/tutorial-v1`: tutorial tags and guided onboarding
9. `game/progression-save-v1`: local progression and save
10. `game/art-direction-v1`: readable visual language and state icons
11. `game/feedback-juice-v1`: audio, animation, haptics, result moments
12. `game/mobile-polish-v1`: safe area, screen sizes, accessibility basics
13. `game/build-automation-v1`: Android build/run/log scripts
14. `game/alpha-build`: 30-level internal alpha
15. `game/beta-soft-launch`: soft launch candidate
16. `game/release-readiness`: store, signing process, release AAB checklist

각 PR은 하나의 명확한 gate를 통과해야 한다. 큰 PR이 필요할 때도 PR 본문에서
sub-gate를 분리한다.

## Verification Strategy

모든 PR 공통:

- `git status --short --branch`
- relevant EditMode tests
- relevant PlayMode tests
- `git diff --check`
- `.meta` 누락 확인
- generated folder/build output/log exclusion 확인

Unity/gameplay PR:

- game-local `test.cmd`
- target scene batchmode import
- Android target import/build check when relevant

Level/content PR:

- full fixed level pack validation
- solver pass
- design validator report
- changed levels manual review note

Release PR:

- Android release-like build
- signing secret exclusion
- permissions review
- physical device smoke
- store policy checklist

Manual checks:

- 실제 모바일 조작감
- 화면비별 readability
- 재미/난이도/피로감
- tutorial 이해도
- store account and signing secret handling

## Gate Policy

이 로드맵은 한 번에 끝까지 방향을 잡기 위한 문서지만, 구현은 gate를 통과하면서
진행한다. 어떤 phase가 실패했는데도 다음 phase로 넘어가면 나중에 더 큰 비용으로
되돌아오게 된다.

### Go

다음 조건을 만족하면 다음 phase로 진행한다.

- 자동 검증이 통과한다.
- 해당 phase의 exit criteria가 충족된다.
- manual check가 필요한 항목은 확인됐거나 명확한 known gap으로 기록된다.
- 새로 발견된 위험이 다음 phase의 목표를 무너뜨리지 않는다.

### Iterate

다음 조건이면 같은 phase에서 수정한다.

- 재미는 보이지만 UX, level tuning, feedback이 부족하다.
- solver/generator는 통과하지만 사람이 플레이했을 때 특정 레벨군이 지루하다.
- UI는 동작하지만 모바일 화면에서 상태 판독이 어렵다.
- item/obstacle이 규칙적으로는 맞지만 puzzle decision을 충분히 만들지 못한다.

### Pause Or Replan

다음 조건이면 다음 phase로 진행하지 않고 로드맵을 수정한다.

- Core Fun이 실제 플레이에서 확인되지 않는다.
- 4개 상태 축이 모바일에서 읽히지 않고 단순화 없이는 해결되지 않는다.
- level production이 자동 검증과 수동 선별을 합쳐도 감당할 수 없다.
- 출시 준비 항목이 gameplay 품질보다 우선순위를 빼앗기기 시작한다.

## Content Production Model

Level creation flow:

1. Stage recipe 작성
2. Candidate generator 실행
3. Hard validator와 solver 통과 확인
4. Design validator warning 확인
5. 사람이 candidate를 플레이하거나 replay 검토
6. Fixed level pack에 승격
7. Level intent, player question, risk note 기록
8. 전체 pack regression 검증

Level acceptance 기준:

- clear solution이 존재한다.
- 최소 이동 수가 목표 difficulty band에 맞다.
- direct submit만으로 끝나지 않는다.
- 같은 operation 반복만 요구하지 않는다.
- preview가 의미 있는 결정을 만든다.
- item/obstacle이 있으면 실제 선택에 영향을 준다.
- 실패했을 때 납득 가능한 이유가 있다.

## Release Scope Decisions

출시 전 반드시 결정할 사항:

- launch level count
- monetization 없음 / 광고 / IAP 중 무엇을 택할지
- analytics 사용 여부
- cloud save 제외 여부
- hint system 범위
- daily puzzle/event를 launch에 넣을지
- 무료/유료/외부 asset 사용 정책
- localization 시작 언어

현재 기본값:

- 첫 playable slice에는 monetization, analytics, cloud save를 넣지 않는다.
- launch candidate 전까지는 local save를 기본으로 한다.
- 외부 SDK는 privacy/Android manifest review 전에는 추가하지 않는다.
- 무료/유료 asset은 라이선스, 저장소 포함 가능 여부, LFS 필요 여부를 확인한 뒤
  도입한다.

## Risk Register

### R1 - 4개 상태 축이 모바일에서 복잡할 수 있음

대응:

- state icon system을 early phase에서 검증한다.
- text-only UI로 오래 끌지 않는다.
- state 축 추가보다 state readability를 우선한다.

### R2 - 퍼즐이 공간관리로만 느껴질 수 있음

대응:

- assignment metric과 order competition을 level review 기준에 넣는다.
- storage pressure는 assignment를 압박할 때만 사용한다.

### R3 - Generator가 재미없는 solvable level을 많이 만들 수 있음

대응:

- generator는 후보 생산기로 제한한다.
- 사람이 fixed pack 승격을 결정한다.
- design validator warning을 무시하지 않고 review note에 남긴다.

### R4 - Item이 정답 버튼이 될 수 있음

대응:

- item required/optional metric을 분리한다.
- item 없이도 풀리는 비교 solve를 유지한다.
- item 사용이 새로운 tradeoff를 만들지 않으면 level에서 제외한다.

### R5 - Obstacle이 불공정하게 느껴질 수 있음

대응:

- obstacle은 항상 visible해야 한다.
- action 후보 단계에서 막힘 이유를 보여준다.
- hidden random obstacle은 launch scope에서 제외한다.

### R6 - 출시 준비가 gameplay보다 먼저 커질 수 있음

대응:

- store, analytics, monetization, liveops는 alpha 이후 decision gate로 둔다.
- release slice는 재미와 UX 증명에 집중한다.

### R7 - Unity scene/YAML 변경이 review를 어렵게 만들 수 있음

대응:

- pure rules/model changes와 scene changes를 가능하면 PR에서 분리한다.
- scene/prefab 변경은 작게 유지하고 PlayMode 검증을 붙인다.

## Open Decisions

아직 확정하지 않아도 되는 결정:

- launch level count는 `Alpha Build` 이후 결정한다.
- monetization과 analytics는 `Beta / Soft Launch Candidate` 전에 결정한다.
- full hint system은 level pack v1과 onboarding 결과를 보고 결정한다.
- cloud save는 launch 이후에도 늦지 않다.
- daily/event content는 core 100 levels가 안정된 뒤 판단한다.

가까운 시점에 결정해야 하는 사항:

- `Release Gameplay Slice`의 level 수: 기본값 10-15
- product gameplay scene 이름과 진입 flow
- fixed level data 형식: ScriptableObject 유지 또는 JSON-like export 병행
- 첫 추가 modifier 후보 1-2개

## Next Document

현재 다음 실행 문서는 `DreamLaundromat/docs/ALPHA_READINESS_PLAN.md`다.

`RELEASE_GAMEPLAY_SLICE_PLAN.md`와 `PHASE_6_13_PLAN.md`를 통해 Phase 1-13의 CLI 구현과
자동 검증 기반은 마련되었다. 이제 Phase 14 `Alpha Build`로 바로 넘어가기 전에,
다음 항목을 한 번의 Alpha readiness pass로 검증하고 보강해야 한다.

- UI/비주얼 완성도 V2
- Game Feel / Direct Manipulation V2
- `ReleaseGameController`와 UI 구조 hardening
- 30레벨 수동 playtest와 level/UX tuning
- 80-120레벨 production loop V1

`ALPHA_READINESS_PLAN.md`는 자동 검증으로 확인할 수 있는 항목과 사람이 직접 판단해야
하는 재미, 조작감, visual taste, haptic/audio 감각을 분리한다.

## Self-Review

이 로드맵은 planning checklist 기준으로 다음을 포함한다.

- Core Fun: 상태 읽기, assignment, 순서 계획, 공간 압박, 빠른 재시도
- Game Pillars: one-hand clarity, deterministic actions, assignment-first,
  preview fairness, small rules, fast retry
- Core Rules: Dynamic Lab engine을 기준으로 action, clear/failure, modifier 방향을
  유지
- Puzzle Grammar: recipe, generator, fixed level pack, level arc
- Level Progression: slice, alpha, soft launch, launch level 범위
- Content Production: candidate 생성, 검증, 선별, fixed pack 승격
- UX / Interaction: product gameplay scene, touch, preview, invalid feedback
- Satisfaction Design: audio, haptics, animation, clear/fail moment
- World / Character: World Direction, art direction, 꿈 세탁소 visual language phase
- Prototype Success Criteria: phase별 gate와 exit criteria

현재 문서의 의도적 한계:

- 각 phase의 코드를 바로 구현할 정도의 세부 task는 아니다.
- 다음 구현 PR은 별도 `RELEASE_GAMEPLAY_SLICE_PLAN.md`에서 쪼갠다.
- Google Play 정책, target API, SDK 요구사항은 출시 직전에 공식 문서로 재확인해야 한다.
