# Dream Laundromat Planning

이 디렉토리는 `Dream Laundromat`을 구현하기 전에 확정해야 할 10개 핵심 기획 항목을 상세화한다.

목표는 문서를 많이 만드는 것이 아니라, 구현 전에 게임의 재미, 규칙, 레벨 생산성, UX, 세계관, 프로토타입 검증 기준이 서로 같은 방향을 보도록 맞추는 것이다.

## Planning Documents

1. [Core Fun](01-core-fun.md)
2. [Game Pillars](02-game-pillars.md)
3. [Core Rules](03-core-rules.md)
4. [Puzzle Grammar](04-puzzle-grammar.md)
5. [Level Progression](05-level-progression.md)
6. [Content Production](06-content-production.md)
7. [UX / Interaction](07-ux-interaction.md)
8. [Satisfaction Design](08-satisfaction-design.md)
9. [World / Character](09-world-character.md)
10. [Prototype Success Criteria](10-prototype-success-criteria.md)

출시 가능성 검토용 부록:

- [Retention / Monetization / LiveOps Assumptions](11-retention-monetization-liveops.md)

## Cohesion Model

이 10개 항목은 아래 순서로 서로 영향을 준다.

```text
Core Fun
  -> Game Pillars
  -> Core Rules
  -> Puzzle Grammar
  -> Level Progression
  -> Content Production

UX / Interaction
  -> Core Rules
  -> Puzzle Grammar
  -> Satisfaction Design

World / Character
  -> Core Fun
  -> Satisfaction Design
  -> Event / LiveOps

Retention / Monetization / LiveOps
  -> Content Production
  -> UX / Interaction
  -> Prototype 이후 범위 판단

Prototype Success Criteria
  -> Core Fun 검증
  -> Rules 검증
  -> Level Production 검증
  -> UX 검증
```

## Current Design Thesis

`Dream Laundromat`의 핵심 재미 가설은 다음과 같다.

> 제한된 세탁소 공간에서 이상한 꿈 조각을 원하는 상태로 변환해 손님에게 딱 맞게 돌려줄 때, 정리의 쾌감과 순서 계획의 깨달음이 동시에 생긴다.

따라서 모든 규칙과 콘텐츠는 아래 질문을 통과해야 한다.

- 이 규칙은 꿈을 "정리하고 돌려주는" 재미를 강화하는가?
- 유저가 성공/실패 이유를 즉시 이해할 수 있는가?
- 같은 규칙으로 여러 레벨을 만들 수 있는가?
- 한 손 세로 화면에서 상태를 읽고 조작할 수 있는가?
- 기묘하고 따뜻한 꿈 세탁소 세계관과 어울리는가?

## Planning Priority

구현 전에 가장 먼저 확정해야 하는 것은 아래 4개다.

1. `Core Fun`: 무엇이 재미인지
2. `Core Rules`: 한 판의 규칙이 무엇인지
3. `Puzzle Grammar`: 레벨을 많이 만들 수 있는 문법이 있는지
4. `Prototype Success Criteria`: 첫 구현이 무엇을 검증해야 하는지

나머지 항목은 프로토타입 직전까지 계속 다듬되, 위 4개를 바꾸면 전체 문서를 다시 검토해야 한다.

## Consistency Checklist

기획 변경 전후에 아래를 확인한다.

- 새 규칙이 `Game Pillars` 중 하나 이상을 강화하는가?
- 새 상태나 기계가 `Puzzle Grammar`에 자연스럽게 들어가는가?
- 새 UI 요구가 한 손 조작을 해치지 않는가?
- 새 캐릭터/이벤트가 레벨 생산 모델과 연결되는가?
- 프로토타입 성공 기준을 흐리게 만드는 기능은 아닌가?
