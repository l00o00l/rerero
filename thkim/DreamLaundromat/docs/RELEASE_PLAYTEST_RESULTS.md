# Release Playtest Results

## Purpose

이 문서는 `DreamLaundromat` 30레벨 release slice의 수동 플레이테스트 결과를 기록하는
작업 파일이다. 자동 검증은 이미 solvable, warning, build, smoke를 확인하지만, 재미,
피로도, 조작감, visual taste는 사람이 직접 기록해야 한다.

## Current Status

2026-06-18 기준 상태:

- 자동 검증: 통과
- Android 빌드/설치/screenshot: 통과
- 30레벨 수동 플레이테스트: 미실행
- Alpha 판정: 자동 기준은 통과했으나, 수동 플레이테스트 전에는 `Go`로 판정하지 않는다.

## Automatic Baseline

마지막 자동 기준:

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
# Valid=True, AccessibilityValid=True, Warnings=0, DesignNotes=58

.\DreamLaundromat\dynamic-lab.cmd -CandidateCount 4 -TimeoutSeconds 900
# Passed. Total=16 Accepted=12 Rejected=4

.\DreamLaundromat\run.cmd -Build -BootTimeoutSeconds 300 -BuildTimeoutSeconds 1200
# Passed

.\DreamLaundromat\screenshot-smoke.cmd -TimeoutSeconds 900
# Passed

.\DreamLaundromat\level-screenshots.cmd -Build -LevelIndexes 0,4,9,14,29 -TimeoutSeconds 1200 -BuildTimeoutSeconds 1200
# Passed
```

## Result Scale

각 항목은 [Release Manual Playtest Checklist](RELEASE_MANUAL_PLAYTEST_CHECKLIST.md)의 기준을 따른다.

- `0`: 문제 없음.
- `1`: 거슬리지만 진행 가능.
- `2`: 수정 없이는 출시 후보로 보기 어렵다.

## Batch Summary

- 가장 재미있었던 레벨 3개:
- 가장 지루했던 레벨 3개:
- 가장 헷갈렸던 UI/feedback 3개:
- 즉시 수정할 레벨:
- 유지해야 할 level grammar:
- 다음 production pass에서 추가할 mechanic 또는 tutorial:

## Level Results

| Level | Name | Status | Recommended Action | Notes |
| --- | --- | --- | --- | --- |
| DL-RS-001 | Opening Sort | Not run | TBD |  |
| DL-RS-002 | Calm Before Close | Not run | TBD |  |
| DL-RS-003 | Wash Then Focus | Not run | TBD |  |
| DL-RS-004 | Clean And Clear | Not run | TBD |  |
| DL-RS-005 | Incoming Line | Not run | TBD |  |
| DL-RS-006 | One Basket | Not run | TBD |  |
| DL-RS-007 | Not Every Dream Is Clean | Not run | TBD |  |
| DL-RS-008 | Compact Night Shift | Not run | TBD |  |
| DL-RS-009 | Swap The Queue | Not run | TBD |  |
| DL-RS-010 | Reserved Machine | Not run | TBD |  |
| DL-RS-011 | Pinned Order | Not run | TBD |  |
| DL-RS-012 | Refresh The Dream | Not run | TBD |  |
| DL-RS-013 | Cooling Cycle | Not run | TBD |  |
| DL-RS-014 | Quiet Re-sort | Not run | TBD |  |
| DL-RS-015 | Mood Queue | Not run | TBD |  |
| DL-RS-016 | Clear Target | Not run | TBD |  |
| DL-RS-017 | Shelf Pressure | Not run | TBD |  |
| DL-RS-018 | Preview Promise | Not run | TBD |  |
| DL-RS-019 | Compact Refill | Not run | TBD |  |
| DL-RS-020 | Nightmare Request | Not run | TBD |  |
| DL-RS-021 | Swap Under Pressure | Not run | TBD |  |
| DL-RS-022 | Locked Shortcut | Not run | TBD |  |
| DL-RS-023 | Pinned Tempo | Not run | TBD |  |
| DL-RS-024 | Refresh Choice | Not run | TBD |  |
| DL-RS-025 | Blocked Settle | Not run | TBD |  |
| DL-RS-026 | Full Wash | Not run | TBD |  |
| DL-RS-027 | Generated Clean Room | Not run | TBD |  |
| DL-RS-028 | Generated Compact Room | Not run | TBD |  |
| DL-RS-029 | Final Shelf | Not run | TBD |  |
| DL-RS-030 | Last Request | Not run | TBD |  |

## Per-Level Detail Template

필요한 레벨만 아래 형식으로 상세 기록한다.

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

## Alpha Gate Decision

현재 판정: `Pending Manual Playtest`

근거:

- 자동 검증과 Android smoke는 통과했다.
- 대표 screenshot 기준으로 개발용 debug UI처럼 보이는 큰 텍스트 문제는 상당 부분 줄었다.
- 하지만 실제 재미, 피로도, 한손 조작감, haptic/audio 감각은 아직 사람이 기록하지 않았다.

Alpha `Go` 조건:

- 30레벨 수동 플레이테스트에서 release blocker가 없다.
- `Recommended Action`이 `retest` 또는 `tune level`인 레벨이 있어도 Alpha 이후 backlog로
  감당 가능한 범위다.
- 첫 플레이어가 초반 10레벨에서 상태/주문/operation/preview를 이해할 수 있다.
