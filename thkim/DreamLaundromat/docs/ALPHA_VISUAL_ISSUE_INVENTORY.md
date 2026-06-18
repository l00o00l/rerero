# Alpha Visual Issue Inventory

## Summary

이 문서는 `ALPHA_READINESS_PLAN.md`의 `AR-002 - Visual Issue Inventory` 산출물이다.
2026-06-18에 생성한 대표 Android screenshot baseline을 기준으로, 현재 release slice가
아직 출시 후보 게임처럼 보이지 않는 이유를 화면별 문제로 분해한다.

이 문서는 취향 평가가 아니라 다음 구현 작업인 `AR-003 - Visual/UI Polish V2
Implementation`의 입력이다.

## Screenshot Baseline

대표 screenshot:

- `DreamLaundromat/Logs/level-screenshots/level-01.png`
- `DreamLaundromat/Logs/level-screenshots/level-05.png`
- `DreamLaundromat/Logs/level-screenshots/level-10.png`
- `DreamLaundromat/Logs/level-screenshots/level-15.png`
- `DreamLaundromat/Logs/level-screenshots/level-30.png`

검증 기준:

- `release-slice`: `Valid=True`, `Warnings=0`, `DesignNotes=58`
- `qa-balance`: `Valid=True`, `AccessibilityValid=True`
- 대표 screenshot batch: `LevelIndexes 0,4,9,14,29` 통과

주의:

- `DreamLaundromat/Logs/`는 commit 대상이 아니다.
- screenshot은 visual review 보조 자료이며, 재미와 손맛을 증명하지 않는다.

## Cross-Screen Issues

### VI-001 - Section Box 느낌이 강함

증상:

- `Dream Queue`, `Orders`, `Focus`, `Storage Basket`, `Tools & Obstacles` 같은 큰 섹션
  제목이 화면을 개발용 dashboard처럼 보이게 한다.
- 카드와 버튼 PNG가 있어도 바깥 레이아웃이 표/패널 위주라 게임판보다 검증 UI에
  가깝다.

영향:

- 플레이어가 “꿈 세탁소의 물체를 조작한다”기보다 “라벨 붙은 UI 영역을 누른다”는
  인상을 받는다.

권장 수정:

- 섹션 제목을 짧은 diegetic label로 바꾼다.
- 빈 섹션 설명 문구를 줄이고, frame/empty slot affordance로 상태를 읽게 한다.
- focus/status 영역은 설명 문장보다 현재 선택 상태와 다음 action 후보를 압축해서 보여준다.

### VI-002 - Empty/No storage 문구가 개발 UI처럼 보임

증상:

- storage slot에 `Empty`가 크게 보인다.
- storage가 없는 레벨에서는 `No storage in this level.` 문장이 노출된다.

영향:

- storage가 물체나 공간이 아니라 debug state처럼 보인다.

권장 수정:

- 빈 storage는 `Open` 또는 text 없는 shelf frame으로 표시한다.
- storage가 없는 레벨은 `No storage in this level.` 대신 비활성 shelf cue 또는 아주 짧은
  `No shelf`로 줄인다.

### VI-003 - Modifier 표현이 tool/obstacle identity보다 텍스트에 의존함

증상:

- `Block`, `Locked Slot 0`, 숫자 `1`이 세 줄 텍스트로 보인다.
- obstacle frame과 icon은 있지만, obstacle이 “고장/예약/제약”인지 텍스트를 읽어야 한다.

영향:

- item/obstacle이 게임 오브젝트가 아니라 상태 리포트처럼 보인다.

권장 수정:

- `Block` 대신 `Fault`, `Hold`, `Jam` 같은 짧은 metaphor label을 사용한다.
- target id와 charge는 compact chip으로 줄인다.
- obstacle과 item의 background tint를 더 분리한다.

### VI-004 - Footer/status가 아직 문장형 상태 표시임

증상:

- footer에 `Ready`가 큰 섹션 제목처럼 보인다.
- selection 전에는 `Pick dream + order` 문장이 focus 영역에 반복된다.

영향:

- 게임판 상태보다 지시문을 읽는 느낌이 남는다.

권장 수정:

- default message를 `Ready`보다 더 짧은 symbol-like state로 두거나, 선택 전에는 focus
  panel을 덜 강조한다.
- 선택 상태가 생겼을 때만 상세 문구를 보여준다.

### VI-005 - Header 정보 밀도가 낮고 높이가 큼

증상:

- title, level id, orders, moves, guidance가 큰 header 안에 있고, 실제 puzzle board가
  아래로 밀린다.

영향:

- 작은 화면에서 gameplay board가 답답해질 위험이 있다.

권장 수정:

- header는 level title + compact objective 중심으로 줄인다.
- guidance는 tutorial/guided level에서만 적극 노출하고 일반 레벨에서는 focus/status로
  이동한다.

## Screen-Specific Notes

### Level 01

관찰:

- 첫 onboarding level로는 card와 order의 관계가 보인다.
- 다만 `Dream Queue`, `Orders`, `Storage Basket`, `Empty`, `Pick dream + order`가
  텍스트 중심으로 남아 있다.

우선 수정:

- `Empty` 제거 또는 `Open` 축소
- section label 축소
- focus copy 축소

### Level 10

관찰:

- `Locked` 상태와 `Tools & Obstacles`가 보이지만 obstacle 의미가 세 줄 텍스트에 의존한다.
- `No storage in this level.` 문장이 화면의 게임 감각을 크게 깬다.

우선 수정:

- no-storage 문구 축소
- obstacle label compact화
- locked slot overlay를 text보다 icon/stripe 중심으로 변경

### Level 30

관찰:

- 후반 대표 레벨도 전체 화면 구조가 Level 01과 거의 같아 progression의 시각 변화가 약하다.
- order/dream card는 읽히지만, 마지막 레벨다운 tension이나 special state가 약하다.

우선 수정:

- level band/difficulty를 header에 더 compact하게 노출
- 후반 레벨의 obstacle/tool cue가 더 선명히 보이게 한다.

## AR-003 Initial Fix Set

첫 구현은 범위를 좁힌다.

- `Dream Queue`, `Orders`, `Focus`, `Storage Basket`, `Tools & Obstacles`를 더 짧고
  게임 내 물체처럼 보이는 label로 교체한다.
- storage 빈칸의 `Empty` 텍스트를 제거하거나 `Open` 수준으로 축소한다.
- storage가 없는 레벨의 `No storage in this level.` 문장을 줄인다.
- modifier label을 compact하게 줄인다.
- footer/status와 focus default copy를 줄인다.
- 기존 PlayMode tests를 새 copy 기준으로 갱신한다.

## AR-003 Applied Result

2026-06-18 첫 구현 pass에서 반영한 결과:

- gameplay section label:
  - `Dream Queue` -> `Dreams`
  - `Orders` -> `Requests`
  - `Focus` -> `Workbench`
  - `Storage Basket` -> `Shelf`
  - `Tools & Obstacles` -> `Tools / Faults`
- storage가 없는 레벨에서는 storage section 자체를 숨겼다.
- 빈 storage slot의 `Empty` 문구를 제거했다.
- card slot label을 `Dream 1`, `Order 1`, `Basket 1`에서 `D1`, `O1`, `S1`로 줄였다.
- 기본 footer `Ready` 메시지를 숨기고, submit 가능 상태는 workbench/action 쪽의 `Match`로
  옮겼다.
- modifier label은 `Item`/`Block`에서 `Tool`/`Fault`로 정리했다.
- 대표 Android screenshot을 새 APK 빌드/설치 후 다시 캡처했다.

당시 남은 문제:

- 화면은 이전보다 간결하지만, 여전히 큰 section panel 구조가 강하다.
- 하단 action 영역은 아직 버튼 텍스트 의존도가 높았다.
- modifier 상세 텍스트는 아직 compact chip/icon으로 충분히 바뀌지 않았다.
- 출시 수준으로 보이려면 card/object 중심 layout, action dock, result moment, level
  progression visual cue를 다음 pass에서 별도 설계해야 한다.

후속 처리:

- `Gameplay Layout V2`와 `Action Dock Readability` pass에서 하단 action 영역과
  modifier 상세 텍스트를 추가로 줄였다.

## Gameplay Layout V2 Applied Result

2026-06-18 추가 layout pass에서 반영한 결과:

- footer navigation을 제거하고 `Restart`, `Levels`, `Pause`를 header 우상단 compact
  controls로 이동했다.
- `Workbench`는 기본 상태에서 숨기고 선택/submit 가능 상태에서만 나타나게 했다.
- `Dreams`/`Requests` section title을 제거하고 dream/order card row를 키웠다.
- 빈 `Shelf`는 기본 상태에서 숨기고, 저장된 꿈이 있거나 선택한 dream을 저장할 수 있을
  때만 나타나게 했다.
- 비활성 `Store 1/2` 버튼은 기본 화면에서 숨겼다.
- modifier가 없는 level에서는 `Tools/Faults` strip이 화면을 차지하지 않는다.

시각 확인:

- Level 01 기본 화면은 이전보다 dream/order/action 중심으로 읽힌다.
- Level 10처럼 obstacle이 있는 화면은 `Fault` strip이 남아 있으나, storage가 없는 상태의
  불필요한 문장은 없다.
- Level 30도 Level 01보다 상태 차이가 더 잘 보이지만, 후반 level만의 특별한 tension은
  아직 약하다.

남은 문제:

- operation button label은 아직 text 중심이다.
- header compact controls는 동작하지만, 최종 출시 UI에서는 icon 또는 menu 형태가 더
  자연스러울 수 있다.
- card가 커졌지만 전체 화면은 여전히 “board + dock” 구조다. 더 게임다운 느낌은 animation,
  action feedback, result moment 강화와 함께 봐야 한다.

## Action Dock Readability Applied Result

2026-06-18 action dock readability pass에서 반영한 결과:

- `Submit Order`를 `Submit`으로 줄였다.
- 선택 후 나타나는 storage action은 `Store S1`, recall action은 `Recall D1` 형식으로
  표시한다.
- operation button은 full label 대신 `W`, `So`, `Cl`, `Se` marker와 icon을 함께 쓴다.
- modifier label은 내부 `DisplayName` 대신 release UI용 compact label을 사용한다.
  - `Preview Swap` -> `Tool / Swap / x1`
  - `Dream Refresh` -> `Tool / Refresh / Pick D` 또는 `Tool / Refresh D1 / x1`
  - `Locked Slot 0` -> `Fault / Lock D1 / x1`
  - `Pinned Order 1` -> `Fault / Pin O2 / x1`
  - `Wash Soft Block` -> `Fault / Jam Wash / x1`
- 대표 Android screenshot을 새 APK 빌드/설치 후 다시 캡처했다.

시각 확인:

- Level 01에서 submit button이 `Submit`으로 줄어들었다.
- Level 10에서 obstacle strip이 `Fault / Lock D1 / x1`로 표시되어 내부 index 노출이
  사라졌다.
- Level 30에서도 action dock의 command text가 이전보다 짧아졌다.

남은 문제:

- operation button은 marker 중심으로 줄었지만, 초반 tutorial에서 `W/So/Cl/Se`가 어떤
  동작인지 충분히 학습되는지 확인해야 한다.
- header navigation은 아직 text button이다. 출시 UI에서는 icon/menu 형태가 더 자연스러울
  수 있다.
- `D/O/S` 축약 표기는 초반 tutorial에서 의미를 충분히 설명해야 한다.

## Non-Goals

- 최종 art pack 제작
- custom font 도입
- result animation timeline
- full prefab/UI Toolkit migration
- 레벨 데이터 수정
- store screenshot 제작

## Verification

필수:

```powershell
git diff --check
.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900
.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900
.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900
```

권장:

```powershell
.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900
.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900
```

Manual gate:

- 실제로 더 게임처럼 보이는지는 screenshot을 사람이 확인해야 한다.
- 텍스트를 줄인 결과 초반 이해가 어려워졌는지 확인해야 한다.
