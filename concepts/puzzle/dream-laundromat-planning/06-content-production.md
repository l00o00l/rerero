# 06. Content Production

출시 가능한 퍼즐 게임은 레벨을 계속 만들고 검증할 수 있어야 한다. `Dream Laundromat`은 처음부터 데이터 기반 레벨과 검증 흐름을 고려해야 한다.

## Content Goals

초기 목표:

- 프로토타입: 10레벨
- 1차 수직 슬라이스: 30레벨
- 소프트런치 후보: 100레벨
- 출시 후보: 300레벨 이상

이 목표를 달성하려면 레벨 제작이 코드 수정 없이 가능해야 한다.

## Level Data Model

레벨 데이터는 JSON 또는 Unity `ScriptableObject`로 정의할 수 있다. 초기에는 Unity 작업 편의성을 위해 `ScriptableObject`를 쓰되, 구조는 JSON으로도 직렬화 가능하게 유지한다.

필수 필드:

- `levelId`
- `chapterId`
- `moveLimit`
- `dreams`
- `machines`
- `baskets`
- `orders`
- `tutorialHints`
- `bonusGoals`

예상 구조:

```text
Level
  Dreams[]
    id
    initialAttributes
  Machines[]
    id
    type
    capacity
    allowedAttributes
  Baskets[]
    id
    capacity
    restrictions
  Orders[]
    customerId
    requiredAttributes[]
```

## Level Creation Workflow

1. 레벨 의도 작성
   - 가르칠 규칙
   - 압박 지점
   - 예상 해법 길이

2. 레벨 데이터 작성
   - 꿈 조각
   - 기계
   - 바구니
   - 주문
   - 제한 턴

3. 자동 검증
   - 풀 수 있는지
   - 최소 해법 턴 수
   - 막힌 상태가 있는지

4. 수동 플레이
   - 첫 해법 발견 시간
   - 실패 이유 명확성
   - 조작 피로

5. 난이도 태깅
   - Tutorial
   - Easy
   - Medium
   - Hard
   - Event

## Solver / Validator

초기에는 완전한 자동 생성기보다 `검증기`가 우선이다.

검증기가 해야 할 일:

- 레벨이 클리어 가능한지 확인
- 최소 턴 수 계산
- 제한 턴이 최소 턴보다 너무 빡빡하지 않은지 확인
- 모든 주문에 필요한 상태가 실제로 만들 수 있는지 확인
- 사용되지 않는 기계나 꿈 조각 경고

나중에 자동 생성기는 이 검증기 위에 만든다.

## Difficulty Scoring

난이도 점수는 다음 요소의 합으로 계산한다.

- 변환 단계 수
- 꿈 조각 수
- 바구니 여유 capacity
- 기계 병목 수
- 주문 수
- 반전 주문 수
- 제한 턴 여유
- 유사한 꿈 조각 수

예:

```text
difficulty =
  conversionSteps * 1.0
  + dreams * 0.5
  + bottlenecks * 1.5
  + reverseOrders * 2.0
  - spareCapacity * 0.8
  - moveSlack * 0.5
```

점수는 절대 기준이 아니라 레벨 정렬과 리뷰 우선순위를 위한 도구다.

## Event Content

이벤트 레벨은 새 시스템을 과하게 넣지 않는다.

이벤트가 바꿀 수 있는 것:

- 손님 스킨
- 꿈 조각 외형
- 주문 패턴
- 보너스 목표
- 일부 특수 규칙

이벤트가 바꾸면 위험한 것:

- 기본 조작
- 기본 기계 의미
- 주문 판독 방식
- 되돌리기 정책

## Level Review Checklist

레벨 리뷰 시 확인한다.

- 목표가 한눈에 읽히는가?
- 첫 조작 후보가 최소 2개 이상 있는가?
- 실패했을 때 이유가 명확한가?
- 해법이 너무 기계적인 반복은 아닌가?
- 같은 챕터의 이전 레벨과 다른 깨달음이 있는가?
- 다음 레벨을 만들 때 재사용 가능한 구조인가?

## Tooling Roadmap

1. 레벨 데이터 에디터
2. 플레이어블 레벨 미리보기
3. 자동 클리어 검증기
4. 난이도 점수 계산
5. 레벨 배치/필터 UI
6. 이벤트 레벨 복제/변형 도구
