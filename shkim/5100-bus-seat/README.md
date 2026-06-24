# 5100 Bus Seat

5100번 경기 광역버스의 잔여좌석 데이터를 한 달간 로컬에서 수집하고, 정류장과 시간대별 탑승 실패 패턴을 확인하는 검증용 프로젝트다.

## Goal

검증할 가설은 두 가지다.

1. GBIS의 5100번 잔여좌석 데이터가 분석에 쓸 수 있는 품질로 오는가.
2. 정류장과 시간대별로 탑승 실패 패턴이 눈에 보이는가.

이번 단계에서는 예측 모델, 배포, MCP 래핑, 외부 DB 이전을 하지 않는다. 모든 코드는 한 대의 로컬 Windows PC에서 실행한다.

## Scope

- 대상 노선: 5100번 양방향
- 대상 정류장: 모든 탑승 정류장
- 수집기: TypeScript Node 스크립트
- 저장소: 로컬 SQLite
- DB 접근: `better-sqlite3`
- 프론트엔드: Next.js App Router
- 차트: `recharts` 등 가벼운 차트 라이브러리

## Out of Scope

- Vercel 배포
- Turso/Postgres 등 외부 DB
- 예측 모델
- MCP 서버 래핑
- 복잡한 백엔드 프레임워크
- 배차 슬롯 군집화
- 도착 이벤트 추출

## Data Collection Windows

수집기는 평일 피크 시간대에만 동작한다.

```text
출근: 05:00-10:00 KST
퇴근: 17:30-20:30 KST
주기: routeId별 1분 1회
```

양방향 routeId가 2개라면 예상 호출량은 다음과 같다.

```text
출근 5시간 = 300회
퇴근 3시간 = 180회
하루 합계 = 480회 / routeId 1개

양방향 routeId 2개:
480 * 2 = 하루 960회

평일 23일 기준:
960 * 23 = 월 22,080회
```

공공데이터포털의 실제 일일 호출 한도가 `960회/일`과 디버깅 여유분보다 큰지 확인해야 한다.

## Execution Order

1. Gate 1 스크립트로 5100번 routeId 후보를 찾고 양방향 routeId를 확정한다.
2. 각 routeId에 대해 `getBusLocationListv2`를 한 번 호출해서 raw 응답과 잔여좌석 필드 품질을 확인한다.
3. Gate 1이 통과하면 수집기를 실행해서 SQLite에 raw snapshot을 쌓기 시작한다.
4. 데이터가 들어오기 시작한 뒤 Next.js 프론트엔드를 붙인다.

## Environment

공공데이터포털 service key와 DB 경로는 `.env`로만 관리한다. service key는 코드에 하드코딩하거나 커밋하지 않는다.

예상 환경 변수:

```text
GBIS_SERVICE_KEY=
SQLITE_DB_PATH=./data/5100-bus-seat.sqlite
```

## Gate 1

의존성을 설치한 뒤 `.env`에 공공데이터포털 service key를 넣는다.

```bash
npm install
copy .env.example .env
```

먼저 route 후보를 출력한다.

```bash
npm run gate1
```

후보 목록에서 5100번 양방향 routeId를 확인한 뒤, 각 routeId를 인자로 넣어 위치 정보 raw 응답과 잔여좌석 품질을 확인한다.

```bash
npm run gate1 -- <routeId>:up <routeId>:down
```

방향 이름은 사람이 구분하기 위한 라벨이다. 실제 상행/하행 이름을 알고 있으면 `seoulbound`, `suwonbound`처럼 더 명확하게 넣는다.

## Docs

- [Roadmap](./docs/roadmap.md)
- [TODO](./TODO.md)
- [Gate 1](./docs/gate-1.md)
- [Collector](./docs/collector.md)
- [Data Model](./docs/data-model.md)
- [Frontend](./docs/frontend.md)
- [Scheduler](./docs/scheduler.md)
