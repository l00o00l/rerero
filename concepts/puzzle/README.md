# Puzzle Game Concepts

이 디렉토리는 세로 화면, 한 손 조작, 출시 가능성까지 고려한 모바일 퍼즐 게임 컨셉을 보관한다.

목적은 아이디어를 잊지 않기 위한 메모가 아니라, 나중에 다시 열어보고 바로 발전시킬 수 있는 설계 초안으로 남기는 것이다. 각 문서는 컨셉의 감성뿐 아니라 레벨 생산성, 이벤트 확장성, 프로토타입 범위까지 함께 기록한다.

## Target Constraints

- 화면: 세로
- 조작: 한 손
- 장르: 퍼즐
- 목표: 연습용을 넘어서 출시 가능성까지 검토
- 핵심 판단 기준: 직관성, 창의성, 레벨 생산성, 이벤트 확장성, 구현 리스크

## Concept List

| Concept | Core Hook | Level Production | Event Fit | Prototype Risk |
| --- | --- | --- | --- | --- |
| [Dream Laundromat](dream-laundromat.md) / [Planning](dream-laundromat-planning/README.md) | 꿈을 세탁하고 변환해 손님에게 돌려주는 퍼즐 | 높음 | 높음 | 중간 |
| [Lost & Found at the End of the Universe](lost-and-found-end-of-universe.md) | 우주 끝 분실물 센터에서 이상한 물건을 주인에게 돌려주는 퍼즐 | 중간-높음 | 높음 | 중간-높음 |
| [Monster Hotel Room Service](monster-hotel-room-service.md) | 괴물 호텔 손님에게 금기 조건을 피해 주문을 배달하는 퍼즐 | 높음 | 높음 | 중간 |
| [Emotion Magnet](emotion-magnet.md) | 감정을 끌고 밀어 마음을 안정시키는 추상 퍼즐 | 중간 | 중간-높음 | 높음 |
| [Time Elevator](time-elevator.md) | 시간 엘리베이터에서 물건이 변하기 전후를 계산하는 퍼즐 | 중간-높음 | 중간 | 중간-높음 |

## Shared Design Notes

출시형 퍼즐은 컨셉만으로 부족하다. 매우 많은 레벨, 시즌 이벤트, 반복 플레이 동기, 난이도 곡선, 레벨 제작 도구가 함께 필요하다. 그래서 각 컨셉은 다음 질문에 답해야 한다.

- 같은 규칙으로 100개 이상의 레벨을 만들 수 있는가?
- 새 규칙을 10-20레벨 단위로 안전하게 추가할 수 있는가?
- 이벤트 테마가 규칙과 충돌하지 않고 자연스럽게 붙는가?
- 레벨을 데이터로 정의하고 자동 검증할 수 있는가?
- 실패했을 때 유저가 "운이 나빴다"보다 "다시 하면 풀 수 있다"고 느끼는가?

## Recommended Next Step

현재 1순위 후보는 `Dream Laundromat`이다. 창의적인 컨셉과 레벨 생산 구조가 가장 잘 맞고, 이벤트/스킨/시즌 확장도 자연스럽다.

`Dream Laundromat`은 상세 기획 문서를 추가했으므로, 다음 단계는 문서 간 충돌을 정리한 뒤 프로토타입 구현 계획으로 넘어가는 것이다.

- [Dream Laundromat Planning](dream-laundromat-planning/README.md) 검토
- 첫 10레벨의 구체 레벨 데이터 초안 작성
- Unity 프로토타입 구현 계획 작성
- 레벨 검증기와 데이터 구조의 최소 범위 확정
