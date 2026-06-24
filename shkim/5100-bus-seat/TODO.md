# TODO

현재 TODO는 Gate 1 통과에 집중한다. Gate 1이 통과하기 전에는 수집기와 프론트엔드를 구현하지 않는다.

## Now

- [x] TypeScript 프로젝트 최소 설정 만들기
- [x] `.env.example` 만들기
- [x] `.gitignore`에 `.env`, SQLite DB, 로그 파일 제외 추가
- [x] `src/env.ts`에서 환경 변수 읽기
- [x] `src/gbis.ts`에서 GBIS API 호출 함수 만들기
- [x] `src/gate1.ts`에서 `getBusRouteList("5100")` 호출하기
- [x] route 후보를 사람이 고를 수 있게 충분한 필드로 출력하기
- [x] 양방향 routeId 확정 방식 정하기
- [x] `getBusLocationListv2(routeId)` raw 응답 출력하기
- [x] 잔여좌석 필드명과 값 분포 요약하기
- [x] Gate 1 통과/실패 판정 출력하기
- [x] Gate 1 분석 로직 테스트 작성하기
- [x] GBIS URL 생성 테스트 작성하기

## Needs Manual Run

- [x] `.env`에 실제 `GBIS_SERVICE_KEY` 넣기
- [ ] 공공데이터포털에서 경기도_버스노선 조회 API 활용신청/승인 상태 확인하기
- [ ] 공공데이터포털에서 경기도_버스위치정보 조회 API 활용신청/승인 상태 확인하기
- [ ] `npm run gate1`로 route 후보 확인하기
- [ ] 양방향 routeId를 골라 `npm run gate1 -- <routeId>:up <routeId>:down` 실행하기
- [ ] raw 응답에서 실제 잔여좌석 필드명 확인하기
- [ ] 양방향 모두 Gate 1 통과 여부 확정하기

## Blocked by Gate 1

- [ ] SQLite 의존성 추가
- [ ] `vehicle_snapshot` 테이블 생성
- [ ] `route_station` 테이블 생성
- [ ] WAL / busy_timeout 설정
- [ ] 수집기 1분 폴링 구현
- [ ] KST 수집 시각 저장
- [ ] 평일 피크 시간창 판정 구현
- [ ] Windows 작업 스케줄러 실행법 검증

## Blocked by Collector Data

- [ ] Next.js App Router 앱 만들기
- [ ] `better-sqlite3`를 Route Handler에서 읽기
- [ ] Route Handler에 `export const runtime = 'nodejs'` 추가
- [ ] `/api/status` 구현
- [ ] `/api/heatmap` 구현
- [ ] `/api/stations` 구현
- [ ] `/api/station-detail` 구현
- [ ] 수집 상태 위젯 구현
- [ ] 방향별 정류장 x 시간대 실패율 히트맵 구현
- [ ] 정류장 상세 분포 화면 구현
- [ ] 빈 데이터 상태 구현

## Later

- [ ] 한 달 수집 후 unknown 비율 확인
- [ ] 한 달 수집 후 실패율 패턴 확인
- [ ] 한두 정류장 상류 추천 가능성 검토
- [ ] 예측 모델 필요 여부 판단
