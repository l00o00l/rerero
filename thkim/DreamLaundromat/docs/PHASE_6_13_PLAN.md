# Phase 6-13 Alpha Foundation Plan

## Summary

이 문서는 `DreamLaundromat`의 `FULL_GAME_ROADMAP.md` Phase 6-13을 하나의 큰
구현 goal로 묶는 실행 계획이다.

Phase 1-5에서 만든 `Release Gameplay Slice`는 10개 레벨, 제품형 scene, tap/select
interaction, fixed level validation, Android build smoke를 갖춘 상태다. 이번 목표는
그 위에 Alpha 전 단계의 기반을 붙이는 것이다.

핵심 목표:

- item/obstacle이 실제 퍼즐 결정을 넓힌다.
- 첫 플레이어가 tutorial/onboarding 없이 문서를 읽지 않아도 기본 규칙을 배운다.
- level clear와 unlock 상태가 local save로 유지된다.
- state, order, item, obstacle을 모바일에서 더 명확하게 읽을 수 있다.
- action feedback, result feedback, haptic fallback 구조가 생긴다.
- QA/balance report가 레벨 품질을 수치와 수동 gate로 함께 관리한다.

이번 goal은 “출시 완료”가 아니다. 다만 Phase 14 `Alpha Build`로 넘어갈 수 있도록,
자동 검증 가능한 gameplay, UX, tooling, QA 기반을 만드는 것을 완료 기준으로 삼는다.

## Planning References

- [Full Game Roadmap](FULL_GAME_ROADMAP.md)
- [Release Gameplay Slice Plan](RELEASE_GAMEPLAY_SLICE_PLAN.md)
- [Dynamic Puzzle Lab implementation plan](DYNAMIC_PUZZLE_LAB_PLAN.md)
- [Modifier Engine implementation plan](MODIFIER_ENGINE_PLAN.md)
- [Implementation Planning Guide](../../docs/IMPLEMENTATION_PLANNING.md)
- [Unity project conventions](../../docs/CONVENTIONS.md)
- [Mobile Android guidance](../../docs/MOBILE_ANDROID.md)
- [Dream Laundromat concept](../../../concepts/puzzle/dream-laundromat.md)
- [Dream Laundromat planning index](../../../concepts/puzzle/dream-laundromat-planning/README.md)

## Prototype Goal

이번 goal의 가설:

`DreamLaundromat`은 핵심 규칙을 늘리기보다 item/obstacle, tutorial, feedback,
progression, QA loop를 얹었을 때 “반복 가능한 모바일 퍼즐 제품”의 구조로 발전할 수
있다.

검증해야 하는 질문:

- item/obstacle이 정답 버튼이나 hidden trap이 아니라 새로운 판단을 만드는가?
- 30개 수준의 fixed level pack이 자동 검증과 QA report로 관리되는가?
- 첫 10레벨이 rule unlock과 tutorial을 함께 수행하는가?
- 앱을 종료하고 다시 열어도 진행도가 유지되는 구조가 있는가?
- 화면과 feedback이 player에게 상태, 가능 action, 실패 이유를 즉시 전달하는가?

자동 검증으로 확인할 수 있는 것:

- modifier별 hard validation, solver, replay
- tutorial step progression
- save serialize/deserialize와 migration
- level pack validation과 QA/balance report
- PlayMode scene smoke와 Android build/install/launch

자동 검증으로 확인할 수 없는 것:

- 실제 재미
- 손가락 조작감
- 장시간 피로감
- visual/audio 만족감
- 난이도 체감

이 항목은 manual gate로 남긴다.

## Scope

포함 범위:

- Phase 6: `OrderPin`, `DreamRefresh`, `OperationSoftBlock` 계열 modifier 추가
- Phase 7: level-local tutorial step data와 guided input lock V1
- Phase 8: local progression/save, level unlock, clear record, settings save
- Phase 9: state icon/tag visual language, readable palette, UI style guide
- Phase 10: lightweight action animation, basic generated SFX, Android haptic fallback
- Phase 11: safe area, touch target audit, pause/settings, text readability checks
- Phase 12: release slice validation, QA report, log collection, smoke wrapper 강화
- Phase 13: level review checklist, balance report, difficulty band, known issue notes

추가 범위:

- Phase 13이 의미 있으려면 10개 레벨만으로는 부족하므로, fixed level pack을 30개
  수준으로 확장한다.
- 30개 레벨은 최종 Alpha 품질이 아니라, QA/balance loop를 검증하기 위한 pre-alpha
  pack이다.

## Non-Goals

이번 goal에서 제외한다.

- Google Play 등록
- release signing, keystore, secret 입력
- monetization, ads, IAP
- analytics SDK
- cloud save
- release AAB
- 최종 상용 art/audio asset 구매
- 80개 이상 soft launch level pack
- liveops/event/daily puzzle
- localization
- full hint solver UI
- 외부 playtest 운영

## Key Decisions

- 작업 브랜치: 기존 `game/dream-laundromat-release-slice`를 유지한다.
- PR 정책: 사용자가 요청하거나 Phase 6-13 gate가 통과되기 전까지 PR을 만들지 않는다.
- merge 정책: Codex는 PR merge, `git merge`, protected branch push를 하지 않는다.
- level pack 목표: 30개 fixed levels.
- modifier 목표: launch core에 남길 수 있는 3개 추가 후보를 구현한다.
- item 기본값: player가 직접 쓰는 `DreamRefresh`, `PreviewSwap`.
- obstacle 기본값: visible constraint인 `OrderPin`, `OperationSoftBlock`,
  `LockedActiveDreamSlot`.
- tutorial 기본값: 첫 구현은 data-driven step + guided input lock이다.
- save 기본값: `PlayerPrefs` 기반 local save V1, version field 포함.
- settings 기본값: sound, haptic, reduced motion toggle.
- art 기본값: 외부 asset 구매 없이 코드/Unity UI 기반 visual language를 먼저 만든다.
- audio 기본값: generated short tones를 runtime에서 만들거나 AudioClip factory로 생성한다.
- haptic 기본값: Android capability가 없으면 no-op으로 fallback한다.
- QA 기본값: CSV/Markdown 둘 중 하나가 아니라, 사람이 읽는 Markdown report와 tooling이
  읽을 수 있는 structured summary를 함께 만든다.

## Target Platforms

Primary:

- Android mobile
- Portrait
- One-hand touch
- Unity build target: Android

Secondary:

- Unity Editor PlayMode
- Windows batchmode validation

Required wrappers:

- `DreamLaundromat/release-slice.cmd`
- `DreamLaundromat/test.cmd`
- `DreamLaundromat/dynamic-lab.cmd`
- `DreamLaundromat/run.cmd`

Unity batchmode 주의:

- 같은 Unity project에 대해 batchmode 명령을 병렬 실행하지 않는다.
- `docs/TODO.md`에 공통 workflow 개선 항목이 기록되어 있으므로, 이번 작업에서는
  명령을 순차 실행한다.

## Architecture

기본 방향:

- `DynamicLab`은 pure puzzle engine으로 유지한다.
- Phase 6 modifier 확장은 solver, generator, replay, hash, UI가 모두 이해하게 한다.
- `ReleaseSlice` product layer는 tutorial, save, settings, feedback, QA data를 붙인다.
- Scene UI는 당장 prefab system으로 크게 재구성하지 않고, 현재 programmatic UI를
  정리하며 testable helper를 추가한다.

예상 namespace:

- `Thkim.DreamLaundromat.DynamicLab`
  - 새 modifier effect
  - modifier validation
  - action enumeration
  - metrics
- `Thkim.DreamLaundromat.Gameplay.ReleaseSlice`
  - progression/save/settings
  - tutorial controller
  - feedback controller
  - level pack metadata
  - QA/balance report model
- `Thkim.DreamLaundromat.Editor.ReleaseSlice`
  - validation report
  - QA report
  - scene setup
  - log collection

Runtime data flow:

1. `ReleaseGameController` creates `ReleaseGameSession`.
2. `ReleaseGameSession` loads `ReleaseLevelDefinition`.
3. `ReleaseTutorialController` checks current level tutorial steps.
4. UI renders selected state and allowed actions.
5. `DynamicRulesEngine` applies action through `DynamicModifierPipeline`.
6. `ReleaseProgressStore` records clear/unlock/settings.
7. `ReleaseFeedbackController` emits animation/audio/haptic feedback.
8. Validation scripts produce release validation and QA/balance reports.

## Data Model

### Level Metadata

`ReleaseLevelDefinition` should grow to include:

- `ChapterId`
- `DifficultyBand`
- `TutorialStepIds`
- `IntroducedConcepts`
- `ExpectedModifierIds`
- `ManualReviewNote`
- `KnownIssueNote`

### Tutorial

Tutorial data V1:

- `StepId`
- `LevelId`
- `Message`
- `AllowedActionPattern`
- `CompletionCondition`
- `HighlightTarget`

The first pass should cover:

- select dream
- read order
- apply operation
- submit
- preview
- storage
- item
- obstacle

### Progression Save

Save V1:

- `SchemaVersion`
- `HighestUnlockedLevelIndex`
- `ClearedLevelIds`
- `BestMoveCounts`
- `SoundEnabled`
- `HapticsEnabled`
- `ReducedMotionEnabled`

Use `PlayerPrefs` for V1, but keep serialization in a pure class so it can move to file save later.

### QA / Balance

QA report should include:

- `LevelId`
- `ChapterId`
- `DifficultyBand`
- `MinMoves`
- `MoveLimit`
- `MoveSlack`
- `MaxBranchingFactor`
- `AverageBranchingFactor`
- `StorageMoveRatio`
- `OperationDiversity`
- `ItemUseCount`
- `ObstacleBlockedActionCount`
- `DesignWarnings`
- `ManualGate`
- `KnownIssueNote`

## Scene And UI Plan

Current scene:

- `ReleaseGameplaySlice.unity`
- `ReleaseGameController`
- Programmatic Canvas UI

UI additions:

- top progression indicator
- level unlock / clear record display
- tutorial message and highlight affordance
- modifier tooltip/reason text
- pause/settings overlay
- clear summary panel with move count and best move
- fail panel with failure reason and restart
- reduced motion mode
- sound/haptic toggles

Visual language V1:

- state axis tags:
  - `Taint`: Clean / Nightmare
  - `Mood`: Calm / Anxious
  - `Clarity`: Vivid / Blurry
  - `Stability`: Stable / Unsettled
- each state axis gets redundant color + short text + symbol-like marker.
- item and obstacle use distinct UI treatment:
  - item: player-owned tool, charge visible
  - obstacle: visible constraint, blocked reason visible

## Milestones

### M1 - Phase 6-13 Plan Lock

- `PHASE_6_13_PLAN.md` 작성
- 자체 리뷰
- open decisions와 manual gate 분리

### M2 - Modifier Expansion

- 새 modifier effects 추가
- solver/action enumeration/hash/replay/validator 반영
- fixed level pack에 modifier levels 추가

### M3 - Level Pack 30 And QA Data

- 30개 level metadata 구성
- difficulty band와 chapter metadata 추가
- validation report와 QA/balance report 확장

### M4 - Tutorial And Progression

- tutorial step data
- guided lock
- local save/progression/settings
- clear summary와 next unlock

### M5 - UX, Art, Feedback

- state visual language
- basic feedback animation
- generated SFX/haptic fallback
- pause/settings
- accessibility toggles

### M6 - Tooling And Verification Gate

- release validation
- QA report
- EditMode/PlayMode tests
- DynamicLab regression
- Android build/check
- Android install/launch smoke when available
- manual gate documentation

## Task Breakdown

### P613-001 - Baseline Audit

- Outputs:
  - 현재 branch/worktree 상태 기록
- Work:
  - `git status --short --branch -uall`
  - Phase 1-5 산출물 확인
  - `FULL_GAME_ROADMAP.md`와 `RELEASE_GAMEPLAY_SLICE_PLAN.md` 확인
- Verification:
  - Phase 1-5 uncommitted 변경을 삭제하거나 되돌리지 않는다.
- Done criteria:
  - Phase 6-13 작업 시작 기준이 명확하다.

### P613-002 - Phase 6-13 Implementation Plan

- Outputs:
  - `DreamLaundromat/docs/PHASE_6_13_PLAN.md`
- Work:
  - Phase 6-13 scope, milestones, tasks, tests, manual gate 작성
  - 자체 리뷰 후 수정
- Verification:
  - `docs/IMPLEMENTATION_PLANNING.md` 필수 섹션 포함
- Done criteria:
  - 구현자가 질문 없이 다음 task로 진행할 수 있다.

### P613-003 - Modifier Model Expansion

- Outputs:
  - `OrderPin`
  - `DreamRefresh`
  - `OperationSoftBlock`
- Work:
  - `DynamicModifierEffect` 확장
  - `DynamicBuiltInModifiers` factory 추가
  - `DynamicModifierPipeline` 처리 추가
  - hard validator와 action enumeration 반영
- Verification:
  - EditMode modifier tests
  - solver/replay tests
- Done criteria:
  - 새 modifier가 pure model에서 동작하고 solver가 이해한다.

### P613-004 - Modifier Metrics And Validation

- Outputs:
  - item meaningfulness metric
  - obstacle fairness metric
- Work:
  - metrics에 modifier usage와 blocked reason을 더 명확히 기록
  - design validator warning 추가
- Verification:
  - EditMode tests
- Done criteria:
  - QA report가 modifier가 실제 선택에 영향을 주는지 판단할 단서를 제공한다.

### P613-005 - Level Metadata Expansion

- Outputs:
  - extended `ReleaseLevelDefinition`
  - difficulty bands
  - chapter ids
- Work:
  - level metadata 필드 추가
  - 기존 10레벨 metadata 보강
- Verification:
  - pack validation tests
- Done criteria:
  - 각 level의 의도, 난이도, tutorial, QA note를 조회할 수 있다.

### P613-006 - Level Pack 30

- Outputs:
  - 30 fixed levels
- Work:
  - 기존 sample round와 accepted candidate seed를 조합
  - Phase 6 modifier levels 포함
  - tutorial arc와 difficulty band 배치
- Verification:
  - `release-slice.cmd`
  - solver/replay validation
- Done criteria:
  - 30개 level이 모두 자동 검증된다.

### P613-007 - Tutorial Data V1

- Outputs:
  - tutorial step model
  - level tutorial mapping
- Work:
  - first 10 level tutorial steps 정의
  - allowed action pattern과 completion condition 구현
- Verification:
  - EditMode tutorial progression tests
- Done criteria:
  - tutorial data가 runtime state와 독립적으로 검증된다.

### P613-008 - Guided Input Lock

- Outputs:
  - tutorial-aware action gate
  - tutorial message/highlight UI
- Work:
  - `ReleaseGameController` action dispatch 전에 tutorial gate 확인
  - blocked tutorial reason 표시
- Verification:
  - PlayMode tests
- Done criteria:
  - tutorial step이 의도한 action만 허용하고 완료 시 다음 step으로 넘어간다.

### P613-009 - Local Progression Save

- Outputs:
  - save model
  - save serializer
  - `PlayerPrefs` store adapter
- Work:
  - clear record와 highest unlock 저장
  - schema version과 migration hook 추가
- Verification:
  - EditMode serialization/migration tests
  - PlayMode clear 후 unlock tests
- Done criteria:
  - level clear 상태가 session 밖으로 보존된다.

### P613-010 - Settings Save

- Outputs:
  - sound/haptic/reduced motion settings
- Work:
  - settings model과 UI toggle 추가
  - save model에 포함
- Verification:
  - EditMode settings save tests
  - PlayMode settings UI smoke
- Done criteria:
  - feedback 관련 설정이 저장되고 UI에서 변경 가능하다.

### P613-011 - Visual Language V1

- Outputs:
  - state tag renderer
  - palette constants
  - UI style guide section
- Work:
  - 꿈 상태 4축을 text + color + symbol로 표시
  - item/obstacle affordance를 구분
- Verification:
  - PlayMode UI text/symbol presence tests
  - manual screenshot gate
- Done criteria:
  - UI만 보고 state와 modifier의 역할을 구분할 수 있다.

### P613-012 - Feedback V1

- Outputs:
  - action feedback controller
  - generated SFX
  - haptic fallback
  - reduced motion handling
- Work:
  - action, submit, clear, fail, item use feedback
  - settings에 따라 sound/haptic/motion 제어
- Verification:
  - EditMode settings tests
  - PlayMode component smoke tests
- Done criteria:
  - repeated action이 visual/audio/haptic hook을 가진다.

### P613-013 - Pause And Result UX

- Outputs:
  - pause/settings overlay
  - clear/fail summary
- Work:
  - restart, next, settings를 명확히 분리
  - best move/current move 표시
- Verification:
  - PlayMode tests
- Done criteria:
  - clear/fail 이후 다음 행동이 명확하다.

### P613-014 - QA / Balance Report

- Outputs:
  - `qa-balance.cmd`
  - QA report writer
  - known issue/manual gate section
- Work:
  - level validation result를 QA summary로 변환
  - difficulty band별 통계 작성
- Verification:
  - wrapper 실행
  - report artifact 존재 확인
- Done criteria:
  - 각 level이 왜 포함됐는지 report로 설명 가능하다.

### P613-015 - Tooling Cleanup

- Outputs:
  - log collection path
  - validation command summary
- Work:
  - release slice report에 QA report 링크/경로 추가
  - missing XML/log/build artifact 실패 처리 확인
- Verification:
  - scripts fail on missing outputs
- Done criteria:
  - PR 검증 세트가 명령 몇 개로 재현된다.

### P613-016 - Full Verification Gate

- Outputs:
  - 검증 결과 문서화
- Work:
  - 필수 검증 명령 순차 실행
  - Android build/install smoke
  - manual gate update
- Verification:
  - 필수 검증 목록 모두 실행
- Done criteria:
  - Phase 6-13 자동 검증이 통과하고 manual gate가 분리되어 있다.

## PR Plan

사용자 지시에 따라 구현 단위별 PR은 만들지 않는다.

이번 브랜치의 PR 후보:

- 하나의 PR: `DreamLaundromat Phase 1-13 Alpha Foundation`

PR 생성 조건:

- 사용자가 명시적으로 요청한다.
- 또는 Phase 6-13 gate가 통과한다.

PR 본문에 포함할 내용:

- Phase 1-5 산출물 요약
- Phase 6-13 산출물 요약
- 자동 검증 결과
- manual gate와 known gaps
- PR merge/protected branch push 미수행 확인

## Implementation Status

현재 구현된 Phase 6-13 산출물:

- Phase 6:
  - `OrderPin`, `DreamRefresh`, `OperationSoftBlock` modifier model, hard validation,
    action enumeration, solver/replay 처리를 추가했다.
  - `DynamicSampleRounds`에 새 modifier sample round 3개를 추가했다.
  - `ReleaseModifierImpactAudit`로 item 사용 여부, obstacle blocked action, modifier 제거
    비교를 QA/balance report에 추가했다.
- Phase 7:
  - `ReleaseGuidedActionRule`과 level-local tutorial tags를 추가했다.
  - 첫 onboarding level은 guided submit rule로 잘못된 첫 행동을 move 소모 없이 막는다.
- Phase 8:
  - `ReleaseProgressState`, memory store, `PlayerPrefs` store를 추가했다.
  - level clear 시 completed level과 next unlock을 저장한다.
- Phase 9:
  - `ReleaseVisualStyle`로 palette, text color, minimum touch target, contrast 기준을
    코드화했다.
  - release UI의 주요 색상과 touch target 기준이 style token을 참조한다.
  - `ReleaseVisualDescriptors`로 dream state axis와 operation affordance를 text marker로
    표시한다.
- Phase 10:
  - `ReleaseFeedbackEvent`와 feedback sink를 추가했다.
  - Unity runtime sink는 외부 audio asset 없이 action/fail/clear용 generated tone SFX를
    재생한다.
  - Unity runtime sink는 level clear haptic hook을 제공하고 reduced motion/haptic
    setting을 존중한다.
- Phase 11:
  - settings toggle UI를 추가했다.
  - `ReleaseAccessibilityAudit`가 contrast, touch target, guided prompt 기준을 검증한다.
  - Android screenshot 기준으로 guidance text clipping을 줄이고, PlayMode UI presence test를
    보강했다.
- Phase 12:
  - `qa-balance.cmd`와 `scripts/run-qa-balance.ps1`을 추가했다.
  - release validation/balance report는 Windows PowerShell 한글 판독성을 위해
    UTF-8 BOM으로 기록한다.
  - `screenshot-smoke.cmd`와 `scripts/run-screenshot-smoke.ps1`로 Android focused activity,
    process, PNG screenshot, fatal logcat smoke를 확인한다.
- Phase 13:
  - default level pack을 30개로 확장했다.
  - `ReleaseLevelPackValidator`가 30 level gate, modifier coverage, difficulty band,
    tutorial tag, guided rule prefix를 검증한다.
  - `ReleaseBalanceReportBuilder`가 QA/balance summary를 생성한다.
  - `ReleaseVisualReviewChecklist`가 level별 visual/UX manual review 항목을 QA report에
    추가한다.

아직 사람이 직접 확인해야 하는 manual gate:

- 모바일 세로 화면에서 30개 level 전체 판독성, 터치 정확도, 텍스트 겹침 확인
- 실제 재미, 난이도 피로도, 반복감, 실수 복구 감각 평가
- Android 기기/에뮬레이터의 haptic 동작 확인
- 실제 audio asset/SFX 품질과 mix는 별도 제작 후 확인
- 출시 signing, store 계정, privacy/store listing은 별도 release gate에서 확인

## Verification And Test Plan

필수 검증:

```powershell
git status --short --branch -uall
git diff --check
```

`.meta` 누락 확인:

```powershell
$assets = Get-ChildItem -Path 'DreamLaundromat\Assets' -Recurse -File | Where-Object { $_.Extension -ne '.meta' }
$missing = foreach ($asset in $assets) {
    if (-not (Test-Path -LiteralPath ($asset.FullName + '.meta'))) { $asset.FullName }
}
if ($missing) { $missing } else { 'No missing .meta files under DreamLaundromat\Assets.' }
```

Unity validation:

```powershell
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
.\DreamLaundromat\screenshot-smoke.cmd
```

Android:

```powershell
.\DreamLaundromat\run.cmd -Build -BuildOnly -BuildTimeoutSeconds 1200
.\DreamLaundromat\run.cmd -BootTimeoutSeconds 240 -BuildTimeoutSeconds 1200
```

Test expectations:

- pure model: EditMode tests
- modifier solver/replay: EditMode tests
- tutorial flow: EditMode and PlayMode tests
- save/progression: EditMode and PlayMode tests
- UI flow: PlayMode tests
- QA report: batchmode wrapper output test
- Android: build and install/launch smoke

## CLI And Manual Boundary

Codex가 CLI에서 할 수 있는 것:

- code implementation
- level pack expansion
- solver/replay/validation tests
- Unity batchmode validation
- Android build/install/launch smoke when local environment supports it
- report generation
- documentation update
- PR creation if user asks

사람이 해야 하는 것:

- PR merge
- protected branch push
- 실제 기기 장시간 조작감 판단
- tutorial 이해도에 대한 신규 사용자 관찰
- 재미/난이도/피로감 판단
- 최종 visual/audio 취향 판단
- signing secret, store account, paid asset purchase

## Risks

### R1 - Phase 6-13 범위가 너무 넓어질 수 있음

대응:

- Alpha 전 기반으로 제한한다.
- store, monetization, external SDK, release signing은 제외한다.

### R2 - Modifier가 solver complexity를 급격히 키울 수 있음

대응:

- hidden random effect는 넣지 않는다.
- state hash와 action enumeration이 이해하는 deterministic effect만 추가한다.
- per-level validator를 유지한다.

### R3 - 30개 level이 solvable이지만 재미없을 수 있음

대응:

- QA report와 difficulty band를 만든다.
- manual playtest gate를 완료 조건에서 분리한다.

### R4 - Programmatic UI가 커져 review가 어려울 수 있음

대응:

- shared UI helper를 도입하되, 대규모 prefab 전환은 이번 scope에서 제외한다.
- PlayMode tests로 핵심 UI existence와 flow를 고정한다.

### R5 - Feedback/audio/haptic이 모바일 기기마다 다르게 느껴질 수 있음

대응:

- settings와 fallback을 먼저 만든다.
- 감각 평가는 manual gate로 남긴다.

### R6 - Local save가 테스트를 오염시킬 수 있음

대응:

- save key prefix를 분리한다.
- tests는 isolated key를 사용하고 cleanup한다.

## Deferred Or Out Of Scope

game-local backlog:

- 80-120 soft launch levels
- final art asset production
- final audio mixing
- store release AAB
- paid asset purchase
- analytics/privacy policy decision
- cloud save
- localization
- live event content

`docs/TODO.md`에는 위 항목을 기록하지 않는다. 단일 게임 기능 backlog이기 때문이다.
공통 workflow나 repo infrastructure deferred work가 생길 때만 `docs/TODO.md`에 한글로
기록한다.

## Open Decisions

안전한 기본값으로 진행한다:

- 30개 fixed levels
- `PlayerPrefs` local save V1
- generated/basic SFX
- no-op haptic fallback
- guided tutorial lock only for early concept levels
- QA report in Markdown plus structured summary

사용자 판단이 있으면 바꿀 수 있는 결정:

- 30개보다 많은 pre-alpha level을 만들지 여부
- drag interaction을 추가할지 여부
- 외부 무료/유료 asset을 더 도입할지 여부
- tutorial을 더 강한 guided lock으로 만들지 여부

현재는 질문하지 않고 기본값으로 진행한다.

## First Implementation Step

최초 계획 기준의 첫 구현은 `P613-003 - Modifier Model Expansion`이었다.

이유:

- Phase 6 modifier 확장이 이후 level pack, tutorial, QA/balance report의 입력이 된다.
- 새 modifier가 solver와 validation에 들어가야 30개 pack을 자동 검증할 수 있다.
- UI polish와 feedback은 modifier가 확정된 뒤 붙이는 편이 낫다.

## Implementation Status

2026-06-18 기준 Phase 6-13의 CLI 구현과 자동 검증은 완료했다.

완료된 항목:

- modifier model과 built-in modifier pipeline 확장
- modifier 영향 metric과 validation/report 반영
- 30개 release level pack 구성
- guided tutorial data와 early guided input lock 추가
- local progression save와 settings save 추가
- state marker, operation affordance, card/action visual language V1 추가
- pause/result/settings UX V1 추가
- feedback timing table과 QA/balance report 추가
- Android screenshot smoke와 level screenshot batch wrapper 추가

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

- 30개 level 실제 플레이 재미, 피로도, 반복감 확인
- item/obstacle이 정답 버튼이나 hidden trap처럼 느껴지지 않는지 확인
- 실제 Android 기기에서 touch comfort, haptic/audio 감각 확인
- 작은 화면과 긴 화면에서 visual taste와 텍스트 겹침 확인

## Self-Review

검토 기준:

- `docs/IMPLEMENTATION_PLANNING.md` 필수 섹션을 포함했다.
- `FULL_GAME_ROADMAP.md` Phase 6-13을 모두 scope와 task에 반영했다.
- 자동 검증과 manual gate를 분리했다.
- Phase 13을 위해 30개 level pack 확장을 명시했다.
- PR을 하나로 유지하라는 사용자 지시를 반영했다.
- Codex 금지 작업인 merge/protected push를 제외했다.
- game-local backlog와 `docs/TODO.md` 기준을 분리했다.

자체 수정한 점:

- 처음에는 Phase 6-13을 기능별로만 나눌 수 있었지만, Phase 13의 QA/balance가
  실질적으로 동작하려면 level pack 확장이 필요하므로 30개 level을 포함했다.
- visual/audio polish를 최종 품질로 정의하지 않고, 자동 검증 가능한 hook과 settings
  중심으로 줄였다.
- 수동 playtest가 필요한 재미/난이도 판단은 완료 기준이 아니라 manual gate로 분리했다.

남은 위험:

- 실제 재미와 밸런스는 사람이 플레이해야 판단할 수 있다.
- 새 modifier가 solver search space를 키우면 level별 complexity 제한 조정이 필요할 수 있다.
