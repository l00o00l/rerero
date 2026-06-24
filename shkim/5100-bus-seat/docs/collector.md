# Collector

수집기는 TypeScript로 작성하는 단순 Node 스크립트다. 역할은 GBIS의 차량 위치 응답을 주기적으로 호출하고, 차량별 raw snapshot을 SQLite에 저장하는 것이다.

## Principle

수집기는 똑똑하게 분석하지 않는다.

```text
getBusLocationListv2(routeId) 응답을 차량 단위 raw snapshot으로 저장한다.
```

중복 제거, 도착 이벤트 추출, headway 계산, 배차 슬롯 군집화는 이번 범위에서 하지 않는다.

## Polling

대상은 5100번 양방향 routeId다.

```text
평일 출근: 05:00-10:00 KST
평일 퇴근: 17:30-20:30 KST
주기: routeId별 1분 1회
```

한 번의 `getBusLocationListv2(routeId)` 호출은 운행 중인 전 차량의 현재 정류장 순번과 잔여좌석 snapshot을 준다. 따라서 특정 정류장뿐 아니라 상류 정류장 분석까지 같은 데이터로 커버한다.

## Insert Strategy

차량 목록 하나를 수집할 때는 SQLite transaction으로 묶어 insert한다. 로컬 SQLite에서는 이 방식이 단순하고 쓰기 중단 위험이 낮다.

수집 시각은 KST offset을 포함한 ISO 문자열로 저장한다.

```text
예: 2026-06-24T05:01:00+09:00
```

`new Date().toISOString()`은 UTC 문자열을 만들기 때문에 그대로 사용하지 않는다.

## Error Handling

수집기는 네트워크 오류, API 오류, 일시적 빈 응답 때문에 종료되지 않는다.

```text
- 오류를 콘솔에 남김
- 해당 주기는 건너뜀
- 다음 1분 주기에 다시 시도함
```

공공데이터포털 호출 한도 초과 또는 인증 오류처럼 반복되는 문제는 로그를 보고 사람이 조치한다.

## Route Metadata

프론트엔드에서 정류장 순번만 보여주면 해석하기 어렵다. Gate 1 이후 가능한 경우 노선 정류장 목록을 한 번 가져와 `route_station` 기준 데이터로 저장한다.

이 데이터는 분석용 raw snapshot과 분리한다.

## Vehicle Identity

`veh_id`는 일중 dedup이나 흐름 확인에는 쓸 수 있지만, 날짜를 넘는 차량 정체성으로 쓰지 않는다. 차량은 날짜별로 로테이션될 수 있다.

나중에 반복 패턴을 보려면 차량 ID가 아니라 출발 시각대 기반 배차 슬롯을 복원해야 한다. 이번 범위에서는 슬롯 군집화를 하지 않는다.

