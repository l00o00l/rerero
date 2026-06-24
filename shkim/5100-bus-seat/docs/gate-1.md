# Gate 1

Gate 1은 이 프로젝트에서 가장 먼저 통과해야 하는 검증 단계다. 이 단계가 통과하기 전에는 폴링 수집기나 프론트엔드를 만들지 않는다.

## Purpose

확인할 것은 하나다.

```text
GBIS가 5100번 양방향 차량 위치 응답에서 차량별 잔여좌석 값을 분석 가능한 형태로 주는가.
```

## Steps

1. `getBusRouteList`로 `5100`을 검색한다.
2. route 후보가 여러 개면 후보를 모두 출력하고 멈춘다.
3. 사람이 상행/하행 또는 기점/종점 기준으로 사용할 routeId를 확정한다.
4. 확정된 각 routeId에 대해 `getBusLocationListv2`를 한 번 호출한다.
5. raw 응답을 콘솔에 출력한다.
6. 잔여좌석 필드명을 raw 응답에서 확인한다.

## Run

의존성을 설치하고 `.env`에 `GBIS_SERVICE_KEY`를 넣는다.

```bash
npm install
copy .env.example .env
```

route 후보만 확인하려면 다음을 실행한다.

```bash
npm run gate1
```

양방향 routeId를 고른 뒤 위치 정보 raw 응답을 확인한다.

```bash
npm run gate1 -- <routeId>:up <routeId>:down
```

`up`과 `down`은 임시 라벨이다. 실제 방향을 알면 더 명확한 이름으로 바꿔도 된다.

## Route Candidate Output

route 후보 출력에는 최소한 다음 정보를 포함한다.

```text
routeId
routeName
regionName
routeTypeName
startStationName 또는 유사 필드
endStationName 또는 유사 필드
companyName 또는 유사 필드
```

필드명은 GBIS 응답에 맞춘다. 없는 필드는 비워두거나 raw 객체에 그대로 남긴다.

## Pass Criteria

Gate 1 통과 조건은 다음과 같다.

```text
- 5100번 양방향 routeId가 확정됨
- 각 routeId의 getBusLocationListv2 응답에서 운행 차량이 1대 이상 확인됨
- 차량 응답에 잔여좌석으로 보이는 필드가 존재함
- remainSeatCnt 계열 값 중 0, -1, null이 아닌 값이 최소 1개 이상 있음
- raw 응답을 사람이 확인해 실제 필드명을 확정함
```

## Stop Criteria

다음 중 하나라도 해당하면 구현을 멈추고 피벗한다.

```text
- routeId를 확정할 수 없음
- getBusLocationListv2 응답에 차량 목록이 없음
- 잔여좌석 필드가 없음
- 잔여좌석 값이 0, -1, null로만 옴
- 양방향 중 한쪽만 데이터 품질이 정상이고 다른 쪽은 분석 불가함
```

## Notes

`remainSeatCnt`는 추정 필드명이다. 실제 필드명은 Gate 1 raw 응답에서 확인한다.

service key는 `.env`에서만 읽는다. 콘솔 로그에 service key가 노출되지 않게 한다.
