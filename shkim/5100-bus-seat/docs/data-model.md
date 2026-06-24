# Data Model

SQLite는 raw snapshot을 보존하는 방향으로 최소 정규화한다. 분석에 필요한 파생 지표는 조회 시점에 계산한다.

## Database Settings

수집기와 Next.js가 같은 SQLite 파일을 공유하므로 다음 설정을 사용한다.

```sql
PRAGMA journal_mode = WAL;
PRAGMA busy_timeout = 5000;
```

WAL은 로컬에서 읽기와 쓰기가 동시에 일어날 때 잠금 문제를 줄인다. `busy_timeout`은 짧은 write/read 충돌을 바로 실패시키지 않고 기다리게 한다.

## vehicle_snapshot

```sql
CREATE TABLE IF NOT EXISTS vehicle_snapshot (
  collected_at TEXT NOT NULL,
  route_id TEXT NOT NULL,
  direction TEXT NOT NULL,
  veh_id TEXT,
  plate_no TEXT,
  station_seq INTEGER,
  station_id TEXT,
  remain_seat_cnt INTEGER,
  crowded INTEGER,
  low_plate INTEGER,
  raw_json TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_vehicle_snapshot_route_collected
  ON vehicle_snapshot (route_id, collected_at);

CREATE INDEX IF NOT EXISTS idx_vehicle_snapshot_veh_collected
  ON vehicle_snapshot (veh_id, collected_at);
```

`raw_json`은 원본 차량 객체를 한 줄로 저장한다. 필드 누락, 필드명 변경, 나중 분석을 위한 보험이다.

## route_station

정류장 순번을 사람이 읽을 수 있게 하기 위한 기준 테이블이다.

```sql
CREATE TABLE IF NOT EXISTS route_station (
  route_id TEXT NOT NULL,
  direction TEXT NOT NULL,
  station_seq INTEGER NOT NULL,
  station_id TEXT,
  station_name TEXT,
  PRIMARY KEY (route_id, station_seq)
);
```

## Boarding Failure Definition

이번 프로젝트에서는 다음 기준을 사용한다.

```text
remain_seat_cnt <= 0: 탑승 실패
remain_seat_cnt > 0: 탑승 가능
null / -1 / 필드 없음: unknown
```

`-1`은 실패로 보지 않는다. 정보 없음 또는 미제공 값일 가능성이 높기 때문이다.

## Time Bucket

분석 기본 단위는 10분 버킷이다.

```text
05:00-05:09
05:10-05:19
...
```

30분 평균은 앞차 만석과 뒤차 여유가 섞일 수 있어 광역버스 배차 간격 분석에는 너무 거칠다.

## Primary Analysis Grain

프론트엔드와 API의 기본 집계 단위는 다음과 같다.

```text
direction
station_seq
10-minute time bucket
```

주요 지표:

```text
fail_rate = remain_seat_cnt <= 0 비율
avg_remain_seat_cnt
sample_count
unknown_count
```

