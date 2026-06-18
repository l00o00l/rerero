# Visual Game UI V2 Plan

## Summary

이번 pass는 배경 이미지를 적용한 뒤에도 화면이 여전히 텍스트 중심의 개발 UI처럼 보이는 문제를 줄인다.
목표는 `DreamLaundromat` release slice의 핵심 플레이 화면에서 꿈 카드, 주문 카드, 보관 슬롯, 처리 버튼이 실제 게임 오브젝트처럼 보이도록 만드는 것이다.

## Planning References

- `DreamLaundromat/docs/RELEASE_UI_DESIGN_PLAN.md`
- `DreamLaundromat/docs/VISUAL_UX_DIRECTION_PLAN.md`
- `DreamLaundromat/docs/DIRECT_MANIPULATION_GAME_FEEL_V2_PLAN.md`
- `docs/IMPLEMENTATION_PLANNING.md`

## Prototype Goal

대표 Android screenshot에서 `D/O/S/W` 같은 디버그 약어보다 꿈 카드, 주문서, 보관 바구니, 처리 기계 조작부가 먼저 읽혀야 한다.
규칙 이해를 위한 핵심 정보는 유지하되, 상태 전달은 텍스트보다 아이콘, 색, 배지, 진행 게이지를 우선한다.

## Scope

- Gameplay 화면의 section title을 플레이어 언어로 교체한다.
- Dream/Order/Storage card의 슬롯 표기를 디버그 약어에서 release UI 표기로 바꾼다.
- 카드 내부에 상단 밴드, 슬롯 배지, 진행 게이지, 보관 shelf cue 같은 시각 요소를 추가한다.
- Operation/Submit/Store/Recall 버튼의 약어를 줄이고 의미 있는 label을 사용한다.
- 기존 background PNG가 validation 실행 시 덮어써지지 않도록 generator 동작을 유지한다.
- PlayMode, release validation, Android build/run, screenshot smoke, 대표 level screenshot으로 검증한다.

## Non-Goals

- 최종 상용 원화, 캐릭터, 애니메이션 제작은 제외한다.
- 퍼즐 규칙, 레벨 난이도, 아이템/방해요소 설계 변경은 제외한다.
- 전체 `ReleaseGameController` 분리 리팩터링은 제외한다.

## Key Decisions

- 이번 pass는 새 외부 에셋을 추가하지 않고 기존 PNG/icon catalog와 코드 기반 UI 조합으로 처리한다.
- 카드의 slot id는 디버그 약어 `D0`, `O0`, `S0` 대신 `#1`, `Order 1`, `Basket 1`처럼 플레이어에게 보이는 표현을 사용한다.
- Operation 버튼은 icon만 남기지 않고 `Wash`, `Soothe`, `Clarify`, `Settle` label을 유지한다. 접근성과 학습성을 위해 최소 텍스트는 필요하다.
- 배경 이미지가 보이도록 panel alpha를 낮추되, 본문 텍스트 contrast test와 Android screenshot을 반드시 확인한다.

## Target Platforms

- Primary: Android portrait, one-hand touch.
- Verification target: Unity PlayMode, Android emulator/device build/run, screenshot smoke.
- Manual boundary: 실제 취향, 손맛, 화면 밀도 판단은 사람이 최종 확인해야 한다.

## Architecture

- Runtime UI 변경은 `ReleaseGameController`의 rendering helper 안에서 좁게 처리한다.
- Shared color/token 변경은 `ReleaseVisualStyle`에만 둔다.
- 순수 규칙 모델과 `DynamicLab` 코드는 건드리지 않는다.
- 기존 `ReleaseUiArtCatalog`와 `.meta` 참조는 유지한다.

## Scene And UI Plan

- Header: 기존 level title/guidance는 유지한다.
- Dream section: `Dream Queue`와 moves/order progress를 표시하고, 각 card는 꿈 조각처럼 보이도록 top band와 hero icon을 강조한다.
- Order section: `Orders`와 order progress meter를 표시해서 주문서 완료율이 텍스트만으로 보이지 않게 한다.
- Preview section: `Focus`는 유지하되 디버그 문구가 과해지지 않도록 한다.
- Storage section: `Storage Basket`으로 명명하고 empty slot도 보관 바구니처럼 보이게 한다.
- Action section: operation button은 full action label과 icon을 같이 사용한다.

## Task Breakdown

### VGUI-001 - Debug Label Replacement

- Outputs:
  - Gameplay section title과 slot label 교체
- Concrete work:
  - `D/O/S` section title 제거
  - `D0/O0/S0` visible label 제거
  - PlayMode test 기대값 갱신
- Verification:
  - PlayMode labels
  - Android screenshot
- Done criteria:
  - 대표 screenshot에서 디버그 약어가 주요 UI로 보이지 않는다.

### VGUI-002 - Card Object Treatment

- Outputs:
  - Dream/Order/Storage card visual cue 추가
- Concrete work:
  - 카드 top band, slot badge, order progress meter, storage shelf cue 추가
  - selected/ready halo와 충돌하지 않게 layer 순서 유지
- Verification:
  - PlayMode scene load
  - representative level screenshot
- Done criteria:
  - 카드가 단순 검은 박스가 아니라 게임 오브젝트처럼 보인다.

### VGUI-003 - Action Area Treatment

- Outputs:
  - Operation/Submit/Store/Recall button label 개선
- Concrete work:
  - operation marker 대신 full label 사용
  - store/recall label을 `Store 1`, `Recall 1`로 변경
- Verification:
  - PlayMode interaction tests
  - Android screenshot
- Done criteria:
  - 버튼이 규칙 약어보다 조작 의미를 먼저 전달한다.

## PR Plan

이번 pass는 현재 release slice PR에 포함한다. 별도 PR로 쪼개지 않는다.

## Verification And Test Plan

- `git diff --check -- . ':!DreamLaundromat/Logs' ':!DreamLaundromat/Builds'`
- missing `.meta` scan under `DreamLaundromat/Assets`
- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
- `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900`
- `.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0 -TimeoutSeconds 300 -NoAutoStart`

## CLI And Manual Boundary

Codex can implement, build, run tests, capture screenshots, and inspect generated PNGs from CLI.
Human review is still needed for final taste, premium art direction, readable touch feel on a real phone, and whether the UI now feels sufficiently like a release game.

## Risks

- Lower panel opacity can reduce text contrast.
- More visual children inside cards can break touch targets if raycast settings are wrong.
- Existing PlayMode tests may overfit old debug labels.
- Generated placeholder art may still not reach final store-quality visual fidelity.

## Self Review

- Scope is narrow enough for one PR: yes.
- Verification covers automated tests, Unity validation, Android build, and screenshot: yes.
- User decisions required before implementation: none. Recommended default is to proceed with full labels and object-style card treatment.
- TODO impact: none. This is game-local visual backlog, not shared workflow or repository infrastructure.

## First Implementation Step

Update `ReleaseGameController` rendering helpers and matching PlayMode label assertions.

## Dedicated Sprite Asset V2 Execution

### Summary

이번 추가 패스는 `Dream Queue`, `Orders`, `Storage Basket`가 색만 다른 UI 카드가 아니라 서로 다른 게임 오브젝트처럼 보이도록 전용 카드 표면 PNG를 교체한다. 규칙, 레벨, 입력 흐름은 유지하고, release slice의 시각적 신뢰도를 높이는 데 집중한다.

### Scope

- `Assets/_Project/Art/UI/Cards/release-dream-card-frame.png`
- `Assets/_Project/Art/UI/Cards/release-order-sheet-frame.png`
- `Assets/_Project/Art/UI/Cards/release-storage-shelf-frame.png`
- `Assets/_Project/Art/UI/Cards/release-operation-button-frame.png`
- `Assets/_Project/Art/UI/Cards/release-submit-button-frame.png`
- `Assets/_Project/Art/UI/Cards/release-storage-action-frame.png`
- 같은 파일명과 `.meta`를 유지해서 `ReleaseUiArtCatalog` 참조가 흔들리지 않게 한다.
- `ReleaseUiArtGenerator`의 기존 PNG 보존 동작을 유지해서 validation 실행이 수동 교체 에셋을 덮어쓰지 않게 한다.

### Non-Goals

- 최종 출시용 외주/AI 완성 아트 확정은 이번 범위가 아니다.
- 꿈 조각, 주문 규칙, 장애물, 레벨 밸런스 변경은 포함하지 않는다.
- 카드 내부 텍스트나 규칙 설명을 이미지 안에 굽지 않는다.

### Visual Direction

- Dream card: 어두운 세탁소 조명 위에 꿈 거품, 별가루, 부드러운 코어 글로우가 있는 오브젝트로 보이게 한다.
- Order sheet: 작업 지시서, 체크 라인, 접힌 종이 가장자리, 진행 레일 느낌을 준다.
- Storage shelf: 바구니/선반/천 슬롯처럼 보이게 해서 임시 보관 기능이 직관적으로 읽히게 한다.
- Operation button: 세탁소 기계 조작 패널처럼 보이게 해서 `Wash/Soothe/Clarify/Settle` 버튼이 단순 검은 박스처럼 보이지 않게 한다.
- Submit/Store button: 주문 접수대와 보관 선반의 표면 단서를 줘서 하단 액션 영역도 게임 오브젝트처럼 읽히게 한다.
- 모든 PNG는 512x512 기준으로 제작하고, 현재 UI에서 stretch되어도 깨지지 않도록 중앙부를 단순하게 유지한다.

### Verification And Test Plan

- PNG 직접 검수: 세 카드가 서로 다른 역할로 즉시 구분되는지 확인한다.
- `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
- `.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900`
- `.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0 -TimeoutSeconds 300 -NoAutoStart`
- `git diff --check -- . ':!DreamLaundromat/Logs' ':!DreamLaundromat/Builds'`
- missing `.meta` scan under `DreamLaundromat/Assets`

### Self Review

- 범위는 카드 표면 에셋 교체로 한정되어 현재 PR 안에서 검토 가능하다.
- 기존 generator 보존 정책과 `.meta` 유지 원칙이 계획에 포함되어 있다.
- 자동 검증, Android build/run, screenshot 확인이 포함되어 있다.
- 사용자 결정이 필요한 사항은 없다. 최종 취향 판단은 screenshot과 실제 기기 플레이 후 별도 보정한다.

## Footer/Navigation Visual V2 Execution

### Summary

카드와 주요 action 버튼은 전용 sprite가 적용됐지만, gameplay footer의 `Restart/Levels/Pause/Next`와 일부 navigation 버튼은 아직 평평한 텍스트 버튼처럼 보인다. 이번 패스는 footer/navigation 버튼에 게임 표면을 입히고, action/footer의 세로 점유를 줄여 화면이 덜 문서처럼 보이게 한다.

### Scope

- `Assets/_Project/Art/UI/Cards/release-navigation-button-frame.png` 추가
- `ReleaseUiArtCatalog`에 navigation button frame 참조 추가
- gameplay footer 버튼에 navigation surface sprite 적용
- level select footer와 result action 버튼에도 가능한 범위에서 같은 surface 적용
- action/footer panel 높이, padding, spacing, 메시지 font size를 조정해 화면 밀도를 낮춘다.

### Non-Goals

- 핵심 퍼즐 규칙, 레벨 데이터, 선택/드래그 조작 흐름은 변경하지 않는다.
- 최종 icon set, custom font, 애니메이션 polish는 이번 범위가 아니다.
- home/level select 전체 화면 리디자인은 별도 pass로 남긴다.

### Key Decisions

- navigation 버튼은 기계 하단 조작대처럼 보이는 공통 표면을 사용한다.
- 버튼의 실제 의미는 텍스트로 유지한다. 아직 icon-only UI로 바꾸기에는 학습 비용과 접근성 리스크가 크다.
- action/footer는 너무 낮추면 터치 타깃과 가독성이 깨지므로 58px 이상 터치 타깃은 유지한다.

### Verification And Test Plan

- `ReleaseUiArtGenerator.RunFromCommandLine`
- `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
- `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
- `.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900`
- `.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900`
- `.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0 -TimeoutSeconds 300 -NoAutoStart`
- `git diff --check -- . ':!DreamLaundromat/Logs' ':!DreamLaundromat/Builds'`
- missing `.meta` scan under `DreamLaundromat/Assets`

### Self Review

- 범위는 남은 navigation/footer 시각 보강과 레이아웃 밀도 조정으로 제한되어 있다.
- 자동 테스트와 Android screenshot 검증이 포함되어 있다.
- 사용자 결정이 필요한 항목은 없다. icon-only 전환과 전체 home/level select 리디자인은 별도 판단으로 남긴다.
