# Release Manual Playtest Checklist

## Purpose

이 문서는 `DreamLaundromat` release slice 30레벨을 사람이 직접 플레이하면서 기록할
평가 기준이다. 자동 검증은 solvable 여부, warning, build, smoke를 확인하지만,
퍼즐의 재미, 조작감, 정보 가독성, 지루함은 사람이 직접 판단해야 한다.

## Test Setup

- Target: Android portrait
- Input: one-hand touch
- Build: development build 허용
- 시작 전 자동 검증:
  - `.\DreamLaundromat\test.cmd -Mode EditMode -TimeoutSeconds 900`
  - `.\DreamLaundromat\test.cmd -Mode PlayMode -TimeoutSeconds 900`
  - `.\DreamLaundromat\release-slice.cmd -TimeoutSeconds 900`
  - `.\DreamLaundromat\qa-balance.cmd -TimeoutSeconds 900`
  - `.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 900`
  - `.\DreamLaundromat\level-screenshots.cmd -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 900`

## Current Automatic Baseline

2026-06-18 기준 수동 플레이테스트 전 자동 baseline은 아래 상태다.

- `release-slice`: `Valid=True`, `Levels=30`, `Errors=0`, `Warnings=0`
- EditMode: `88/88` 통과
- PlayMode: `20/20` 통과
- Android debug build/install/launch: 통과
- Android screenshot smoke: 통과
- 대표 level screenshot batch: `LevelIndexes 0,4,9,14,29` 기준 통과
- gameplay UI는 `Gameplay Layout V2`와 `Action Dock Readability` pass 이후 기준이다.
  `Submit`, operation marker, compact modifier label이 반영된 빌드로 평가한다.

아직 완료하지 않은 것:

- 30개 level 전체 수동 플레이테스트
- 실제 Android 기기에서 한손 touch comfort, haptic/audio 감각 확인
- 작은 화면/긴 화면에서 텍스트 겹침과 visual taste 확인

## Scoring

각 항목은 `0-2`로 기록한다.

- `0`: 문제 없음.
- `1`: 거슬리지만 진행 가능.
- `2`: 수정 없이는 출시 후보로 보기 어렵다.

## Per-Level Record

각 레벨마다 아래 형식으로 기록한다.

```md
### DL-RS-000 - Level Name

- Clear: Yes/No
- Attempts:
- Time:
- Move Pressure: 0/1/2
- Input Feel: 0/1/2
- Target Readability: 0/1/2
- State Readability: 0/1/2
- Feedback Clarity: 0/1/2
- Puzzle Interest: 0/1/2
- Repetition/Boredom: 0/1/2
- Item/Obstacle Fairness: 0/1/2
- DesignNotes Triage:
  - order competition:
  - first-solution cadence:
  - preview relevance:
- Notes:
- Recommended Action: keep / tune UI / tune level / add tutorial / retest
```

## Evaluation Questions

### Input Feel

- tap과 drag 중 어떤 조작이 자연스러운가?
- drag 시작 threshold가 너무 민감하거나 둔하지 않은가?
- invalid drop이 실수로 보이는가, 규칙 위반으로 이해되는가?
- 한 손 조작에서 하단 action 영역과 카드 영역 이동이 피곤하지 않은가?

### Target Readability

- 선택한 dream이 갈 수 있는 order/storage가 즉시 보이는가?
- storage에서 돌아갈 수 있는 빈 dream slot이 명확한가?
- order 조건을 카드 아이콘만으로 이해할 수 있는가?
- target halo가 배경/카드 프레임에 묻히지 않는가?

### State Readability

- dream 상태 4축이 한눈에 들어오는가?
- operation preview가 실제 다음 결정을 돕는가?
- item/obstacle 상태가 버튼 텍스트 없이도 충분히 드러나는가?
- locked/pinned/soft-block이 불공정해 보이지 않는가?

### Puzzle Interest

- 최소 한 번 이상 “어느 꿈을 먼저 처리할지” 고민하게 되는가?
- operation 순서가 단순 반복으로 굳어지지 않는가?
- storage가 공간 정리 노동이 아니라 의미 있는 선택을 만드는가?
- preview/random stream이 새 판단을 만들고 있는가?

### Repetition And Boredom

- 같은 operation-submit 패턴이 여러 레벨 연속 반복되는가?
- order competition이 부족해 자동으로 답이 보이는가?
- move limit이 압박이 아니라 귀찮은 세금처럼 느껴지는가?
- 레벨의 intent가 실제 플레이에서 느껴지는가?

## DesignNotes Triage

`DesignNotes`는 바로 warning으로 보지 않는다. 수동 플레이에서 아래처럼 분류한다.

- `Keep`: 튜토리얼 목적상 단순해야 하고, 실제로 지루하지 않다.
- `Tune`: 의도는 맞지만 반복, 압박, 정보 부족이 느껴진다.
- `Escalate`: release gate warning으로 올려야 할 만큼 레벨 품질을 해친다.

우선순위:

1. 플레이어가 답을 모른 채 실수하게 만드는 정보 부족.
2. 정답 패턴이 너무 빨리 고착되는 레벨.
3. storage가 선택이 아니라 빈칸 관리로만 느껴지는 레벨.
4. item/obstacle이 퍼즐을 풍부하게 하지 않고 정답 버튼이나 억제 장치처럼 느껴지는 레벨.

## Batch Summary

30레벨 완료 후 아래를 요약한다.

- 가장 재미있었던 레벨 3개와 이유.
- 가장 지루했던 레벨 3개와 이유.
- 가장 헷갈렸던 UI/feedback 3개.
- 즉시 수정할 레벨 목록.
- 유지해야 할 level grammar.
- 다음 production pass에서 추가할 mechanic 또는 tutorial.
