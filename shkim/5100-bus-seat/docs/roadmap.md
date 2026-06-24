# Roadmap

이 문서는 5100 Bus Seat 프로젝트의 전체 구현 순서를 고정한다. 현재 목표는 한 달 검증용 로컬 도구를 만드는 것이다.

## Current Status

```text
현재 단계: Gate 1 스크립트 구현 완료, 실제 serviceKey로 수동 검증 전
다음 단계: npm run gate1로 route 후보 확인
```

Gate 1을 실제 GBIS 응답으로 통과하기 전에는 수집기, SQLite 적재 자동화, Next.js 대시보드를 구현하지 않는다.

## 0. Documentation

프로젝트 범위와 구현 순서를 문서로 고정한다.

완료 조건:

```text
- 프로젝트 README 작성
- Gate 1 기준 작성
- 수집기 설계 작성
- SQLite 데이터 모델 작성
- 프론트엔드 설계 작성
- Windows 작업 스케줄러 운영 문서 작성
- 전체 로드맵 작성
- 현재 TODO 작성
```

상태:

```text
Done
```

## 1. Gate 1

GBIS가 5100번 양방향에 대해 차량별 잔여좌석 값을 실제로 주는지 확인한다.

작업:

```text
- TypeScript 프로젝트 최소 설정
- .env.example 작성
- GBIS API 호출 유틸 작성
- getBusRouteList("5100") 호출
- route 후보 출력
- 양방향 routeId 확정
- getBusLocationListv2(routeId) 1회 호출
- raw 응답 출력
- remainSeatCnt 계열 필드 품질 요약
```

완료 조건:

```text
- 5100번 양방향 routeId가 확정됨
- 각 방향에서 운행 차량이 1대 이상 확인됨
- 잔여좌석 필드가 존재함
- 0, -1, null이 아닌 잔여좌석 값이 최소 1개 이상 확인됨
- 통과/실패 결과가 콘솔에서 명확히 보임
```

상태:

```text
Needs manual GBIS run
```

## 2. Collector

Gate 1 통과 후 수집기를 구현한다.

작업:

```text
- SQLite 초기화 코드 작성
- vehicle_snapshot 테이블 생성
- route_station 테이블 생성
- WAL / busy_timeout 설정
- KST 수집 시각 생성
- 평일 시간창 판정
- 양방향 routeId 1분 폴링
- 차량 snapshot transaction insert
- 오류 발생 시 다음 주기로 계속 진행
```

완료 조건:

```text
- 수집기를 수동 실행하면 SQLite 파일이 생성됨
- 피크 시간창 안에서 snapshot row가 쌓임
- 시간창 밖에서는 GBIS 호출을 하지 않음
- 네트워크/API 오류가 프로세스를 죽이지 않음
```

상태:

```text
Blocked by Gate 1
```

## 3. Local Operation

Windows PC에서 한 달간 수집기를 안정적으로 돌릴 수 있게 한다.

작업:

```text
- npm script 정리
- README 실행 순서 보강
- Windows 작업 스케줄러 등록 절차 확인
- 절전 모드 해제 옵션 문서화
- 호출량 계산 재확인
```

완료 조건:

```text
- 사용자가 수집기를 직접 켤 수 있음
- 작업 스케줄러로 피크 시간 전 자동 실행 가능
- 오늘 수집 row 수를 DB에서 확인 가능
```

상태:

```text
Blocked by Collector
```

## 4. Next.js Dashboard

SQLite에 데이터가 들어오기 시작한 뒤 로컬 대시보드를 만든다.

작업:

```text
- Next.js App Router 설정
- better-sqlite3 서버사이드 읽기 설정
- Route Handler에 export const runtime = 'nodejs' 명시
- /api/status 구현
- /api/heatmap 구현
- /api/stations 구현
- /api/station-detail 구현
- 수집 상태 위젯 구현
- 정류장 x 10분 버킷 실패율 히트맵 구현
- 정류장 상세 분포 화면 구현
- 빈 데이터 상태 구현
```

완료 조건:

```text
- npm run dev로 로컬 대시보드가 뜸
- 최근 수집 시각과 오늘 row 수가 보임
- 방향별 히트맵이 보임
- 정류장 선택 시 시간대별 분포와 표본 수가 보임
- DB가 비어도 빈 화면 대신 현재 상태를 보여줌
```

상태:

```text
Blocked by Collector data
```

## 5. One-Month Validation

한 달 동안 데이터를 쌓고 패턴이 보이는지 확인한다.

검증 항목:

```text
- 출근 시간대 특정 정류장에서 실패율이 높아지는가
- 퇴근 시간대 방향별 차이가 보이는가
- 표본 수가 충분한 시간대와 부족한 시간대가 구분되는가
- unknown 값 비율이 분석을 망칠 정도로 높지 않은가
- sleep 또는 네트워크 문제로 생긴 데이터 공백이 어느 정도인가
```

완료 조건:

```text
- 최소 2주 이상 평일 피크 데이터가 쌓임
- 실패율 패턴이 시각적으로 확인됨
- 다음 단계로 예측 모델이 필요한지 판단 가능
```

상태:

```text
Future
```

## 6. Deferred Work

이번 범위에서 명시적으로 하지 않는 작업이다.

```text
- Vercel 배포
- Turso/Postgres 이전
- MCP 서버 래핑
- 예측 모델
- 한두 정류장 상류 추천
- 배차 슬롯 군집화
- 공휴일 캘린더 연동
- 모바일 최적화
- 알림 기능
```

이 항목들은 한 달 검증 후 데이터 품질과 실제 사용성을 보고 다시 판단한다.
