# 04. Puzzle Grammar

`Puzzle Grammar`는 수백 개의 레벨을 만들기 위한 문법이다. 좋은 문법은 새 콘텐츠를 만들 때 매번 새 시스템을 만들지 않아도 된다.

## Grammar Components

퍼즐은 아래 요소의 조합으로 구성된다.

```text
Input Dreams
  + Machines
  + Storage Constraints
  + Customer Orders
  + Move Limit
  = Level
```

## Attribute Vocabulary

꿈 조각은 여러 속성 축으로 표현한다. 한 레벨에 너무 많은 축을 동시에 노출하지 않는다. 각 속성은 최소 하나의 기계, 목표, 제약과 연결되어야 한다.

| Attribute | Values | Main Machine | Puzzle Role |
| --- | --- | --- | --- |
| `stain` | `None`, `Nightmare` | Washer | 세탁 순서 |
| `moisture` | `Dry`, `Wet` | Dryer | 건조 순서 |
| `color` | chapter-defined colors | DyeVat | 색상 매칭 |
| `form` | `Flat`, `Folded` | Folder | 공간 압박 완화 |
| `damage` | `None`, `Torn` | Mender | 복구 순서 |
| `emotion` | 슬픔, 추억, 평온 등 | WarmDryer | 테마와 변환 깊이 |

## Operation Vocabulary

각 조작은 명확한 입력과 출력을 가져야 한다.

| Operation | Input | Output | Cost |
| --- | --- | --- | --- |
| `Wash` | `stain=Nightmare` | `stain=None`, `moisture=Wet` | 1 turn |
| `DryInDryer` | `moisture=Wet` | `moisture=Dry` | 1 turn |
| `Dye` | 색 없는/다른 색 꿈 | 목표 색 꿈 | 1 turn |
| `Fold` | `form=Flat`, `moisture=Dry` | `form=Folded` | 1 turn |
| `Unfold` | `form=Folded` | `form=Flat` | 1 turn |
| `Mend` | `damage=Torn` | `damage=None` | 1 turn |
| `Submit` | 주문과 맞는 꿈 | 주문 진행 | 1 turn |

## Constraint Vocabulary

레벨 난이도는 상태 수보다 제약에서 나온다.

- `Capacity Limit`: 바구니와 기계의 정수 용량
- `Move Limit`: 제한 턴
- `Machine Capacity`: 기계당 한 번에 처리 가능한 꿈 수
- `Order Sequence`: 특정 손님 먼저 제출
- `State Restriction`: 젖은 꿈은 특정 바구니에 둘 수 없음
- `Transformation Side Effect`: 세탁하면 젖음이 붙음, 염색 후 세탁하면 색이 약해짐

## Goal Vocabulary

목표는 명확해야 한다.

- 특정 속성 조건의 꿈 N개 제출
- 여러 손님 주문 모두 완료
- 제한 턴 이하로 완료
- 특정 꿈 조각을 망가뜨리지 않고 완료
- 보너스 목표: 남은 턴, 콤보 제출, 완벽한 정리

## Valid Level Shape

좋은 레벨은 아래 구조 중 하나를 가진다.

### A. Conversion Chain

목표 속성 조합까지 여러 기계를 거쳐야 한다.

예:

```text
stain=Nightmare, moisture=Dry -> Wash -> stain=None, moisture=Wet -> DryInDryer -> stain=None, moisture=Dry -> Submit
```

### B. Storage Puzzle

정답은 간단하지만 임시 보관 공간이 부족하다.

예:

```text
Washer capacity 1, Dryer capacity 1, Basket capacity 2, Dream 4 pieces
```

### C. Order Timing

제출 순서가 공간을 연다.

예:

```text
Customer A를 먼저 완료해야 Basket이 비고, Customer B를 처리할 수 있다.
```

### D. Rule Reversal

이미 배운 규칙을 반대로 사용한다.

예:

```text
일부 손님은 stain=Nightmare가 남은 꿈을 원한다.
```

## Invalid Level Patterns

아래 패턴은 피한다.

- 해법이 하나뿐인데 그 이유가 시각적으로 드러나지 않음
- 주문이 텍스트를 읽어야만 이해됨
- 턴 제한이 빠듯하지만 조작 애니메이션이 길어 답답함
- 같은 속성 조합을 가진 꿈이 너무 많아 구분이 어려움
- 레벨 실패가 마지막 턴에야 드러남

## Combination Budget

한 레벨에 새 개념을 너무 많이 넣지 않는다.

- 초반: 새 개념 1개 + 기존 개념 1개
- 중반: 새 개념 1개 + 기존 개념 2-3개
- 후반: 새 개념 없음 + 기존 개념의 압축/반전
- 이벤트: 익숙한 규칙 + 새 목표/스킨 중심

## Data Implication

레벨 데이터는 최소한 아래를 표현해야 한다.

- 꿈 조각 목록과 초기 속성
- 기계 목록과 변환 규칙
- 바구니/기계 capacity 배치
- 손님 주문 목록
- 제한 턴
- 튜토리얼 표시 여부
- 보너스 목표
