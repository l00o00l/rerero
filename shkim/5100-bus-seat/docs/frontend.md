# Frontend

프론트엔드는 로컬에서 실행되는 Next.js App Router 앱이다. 수집기가 쓰는 SQLite 파일을 서버 사이드 Route Handler가 읽고, 클라이언트 컴포넌트는 일반 API처럼 fetch한다.

## Runtime

SQLite 접근은 `better-sqlite3`를 사용한다. 이 라이브러리는 Edge Runtime에서 동작하지 않으므로 DB를 읽는 Route Handler에는 반드시 Node 런타임을 명시한다.

```ts
export const runtime = 'nodejs';
```

## API Shape

Route Handler는 SQLite를 직접 읽고 JSON을 반환한다.

예상 API:

```text
GET /api/status
GET /api/heatmap?direction=...
GET /api/stations?direction=...
GET /api/station-detail?direction=...&stationSeq=...
```

클라이언트 컴포넌트는 DB를 직접 알지 않는다.

## Screens

### 1. Station Time Heatmap

정류장 순번과 10분 시간대별 탑승 실패율을 보여준다.

기본 색상 기준은 평균 잔여좌석이 아니라 실패율이다.

툴팁에는 다음 값을 함께 보여준다.

```text
station_seq
station_name
time bucket
fail_rate
avg_remain_seat_cnt
sample_count
unknown_count
```

### 2. Station Detail

정류장 하나를 선택하면 시간대별 잔여좌석 분포를 보여준다.

산점도 또는 박스 플롯 중 구현이 단순한 방식을 선택한다. 표본 수가 적으면 신뢰하기 어렵기 때문에 각 시간 버킷의 `sample_count`를 반드시 노출한다.

### 3. Collection Status

수집기가 살아 있는지 확인하는 상태 위젯이다.

표시 항목:

```text
최근 수집 시각
오늘 누적 snapshot row 수
오늘 unknown row 수
DB 파일 경로 또는 파일 존재 여부
```

## Empty State

데이터가 적을 때 빈 화면을 보여주지 않는다.

```text
수집 중입니다. 현재 N건의 snapshot이 있습니다.
```

데이터가 없으면 Gate 1과 수집기 실행 순서를 확인하도록 안내한다.

## Design Level

이 화면은 검증용 대시보드다. 디자인을 과하게 만들지 않는다.

우선순위:

```text
1. 데이터가 들어오는지 확인
2. 어느 방향/정류장/시간대에서 실패율이 높은지 확인
3. 표본 수가 충분한지 확인
```

