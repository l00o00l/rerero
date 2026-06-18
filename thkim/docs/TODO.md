# TODO

이 문서는 할 만하고 가치가 있지만, 사용자의 선택, 현재 작업 범위, 접근 권한,
시점 문제 때문에 의도적으로 미룬 공통 기반 작업을 기록한다.

여기에는 전체 개발 환경, 공유 워크플로우, 저장소 정책, 프로젝트 인프라, 앞으로의
반복 작업 효율에 영향을 주는 항목만 적는다. 일반적인 위시리스트나 개별 기능
backlog로 쓰지 않는다. 각 항목에는 왜 중요한지, 왜 미뤘는지, 가장 작게 시작할 수
있는 다음 단계가 포함되어야 한다.

## 작성 기준

- 지금 당장 하지 않기로 했지만, 공통 기반 작업으로서 가치와 이유가 분명한 항목만
  적는다.
- 단순 아이디어, 막연한 개선, 개별 구현 계획에서 남은 세부 작업, 특정 기능 하나에
  국한된 polish, 이미 불필요하다고 판단한 일은 적지 않는다.
- 게임별 세부 backlog는 해당 계획 문서, PR notes, 또는 issue tracker에 둔다.
- 항목은 한글로 작성하되, 경로, 명령어, API 이름, 코드 식별자는 원문을 유지할
  수 있다.
- 완료했거나 더 이상 유효하지 않은 항목은 지우지 말고 `완료` 또는 `폐기`로
  옮겨 맥락을 남긴다.

## 항목 포맷

```markdown
### YYYY-MM-DD - 제목

- 상태: 열림 | 대기 | 차단 | 완료 | 폐기
- 배경: 이 항목이 나온 작업 맥락.
- 중요한 이유: 지금은 미뤘지만 나중에 처리할 가치가 있는 이유.
- 보류한 이유: 이번 작업에서 하지 않은 구체적인 이유.
- 다음 단계: 다시 시작할 때 가장 작게 할 수 있는 액션.
- 완료 기준: 이 항목을 닫아도 된다고 판단할 수 있는 조건.
- 관련: 관련 문서, 파일, PR, 명령어.
```

## 열림

### 2026-06-17 - Unity batchmode 검증 직렬 실행 가드

- 상태: 열림
- 배경: `DreamLaundromat` 검증 중 `test.cmd -Mode EditMode`와 `dynamic-lab.cmd`를
  동시에 실행했을 때 같은 Unity project를 두 batchmode 프로세스가 열면서
  `dynamic-lab.cmd`가 report를 만들지 못하고 즉시 실패했다. 단독 재실행 시에는
  정상 통과했다.
- 중요한 이유: 앞으로 PR 검증이나 장시간 자동 작업에서 Unity batchmode 명령을 병렬로
  실행하면 실제 코드 회귀가 아닌 project lock 충돌을 실패로 오인할 수 있다.
- 보류한 이유: 이번 작업의 주 범위는 `DreamLaundromat` release gameplay slice
  구현이며, 공통 검증 orchestrator나 lock 파일 기반 실행 큐를 만드는 것은 별도
  인프라 작업이다.
- 다음 단계: `docs/IMPLEMENTATION_PLANNING.md` 또는 공통 검증 스크립트에 Unity
  project별 batchmode 명령은 직렬 실행한다는 규칙을 추가하고, 필요하면
  `scripts/check-pr.ps1` 같은 wrapper에서 project lock을 감지한다.
- 완료 기준: 로컬 PR 검증 절차에서 Unity batchmode 명령이 같은 project에 대해
  병렬로 실행되지 않으며, 충돌 시 원인을 명확히 안내한다.
- 관련: `DreamLaundromat/test.cmd`, `DreamLaundromat/dynamic-lab.cmd`,
  `DreamLaundromat/release-slice.cmd`.

### 2026-06-15 - Android 실행 스크립트 공통 템플릿 정리

- 상태: 열림
- 배경: `DreamLaundromat/run.cmd` 검증 중 `ANDROID_HOME`/`ANDROID_SDK_ROOT`가
  Unity 내장 SDK를 가리키면 Android Studio AVD의 `emulator.exe`가 system image를
  찾지 못하고 종료되는 문제가 확인되었다. DreamLaundromat 스크립트는 이번 작업에서
  Android Studio SDK 우선 탐색과 에뮬레이터 프로세스 환경 보정으로 수정했다.
- 중요한 이유: 앞으로 게임마다 `run.cmd`와 `scripts/run-emulator.ps1`를 유지하면
  SDK 탐색, AVD 부팅 대기, APK 설치/실행 검증 로직이 반복되고 서로 달라질 수 있다.
  공통 템플릿이 있으면 새 Unity 모바일 프로젝트를 만들 때 실행 스크립트 품질을
  일정하게 유지할 수 있다.
- 보류한 이유: 이번 PR의 주 목적은 DreamLaundromat 프로토타입 구현과 검증이다.
  기존 게임 스크립트 전수 수정이나 템플릿 추출은 범위가 커져 별도 PR에서 다루는
  편이 안전하다.
- 다음 단계: `DreamLaundromat/scripts/run-emulator.ps1`의 SDK 탐색, boot wait,
  `-BuildOnly` 흐름을 기준 템플릿으로 정리하고, 기존 게임 스크립트에도 같은 동작을
  적용할지 검토한다.
- 완료 기준: 새 게임을 만들 때 재사용할 수 있는 Android 실행 스크립트 템플릿이
  문서화되어 있고, 기존 게임 스크립트가 같은 SDK 탐색/에뮬레이터 부팅 규칙을 따른다.
- 관련: `DreamLaundromat/run.cmd`, `DreamLaundromat/scripts/run-emulator.ps1`,
  `PocketDodger/scripts/run-emulator.ps1`, `docs/IMPLEMENTATION_PLANNING.md`.

### 2026-06-12 - GitHub 서버 측 merge 보호 설정 활성화

- 상태: 차단
- 배경: Codex가 PR 생성/리뷰까지만 하고 merge는 하지 않도록 로컬 Codex 규칙,
  Git hook, 저장소 지침을 추가했다.
- 중요한 이유: 로컬 Codex 규칙과 Git hook은 실수를 줄이지만, 직접 push,
  force push, 브랜치 삭제, self-merge를 권위 있게 막는 장치는 GitHub branch
  protection 또는 repository ruleset이다.
- 미룬 이유: 현재 인증된 GitHub 계정 권한은 `WRITE`이고, repository ruleset
  생성 시 `404 Not Found`가 반환되었다. 이 설정 변경에 owner/admin 권한이
  부족한 상황과 일치한다.
- 다음 단계: 저장소 owner/admin 계정으로 `main`에 ruleset 또는 branch
  protection rule을 만든다. PR 필수, 승인 1개 이상, 마지막 push 작성자가
  아닌 사람의 승인, conversation resolution, force push/삭제 차단을 요구한다.
- 완료 기준: GitHub에서 `main` 보호 규칙이 활성화되고, 현재 작업 계정이 혼자
  직접 push하거나 self-merge할 수 없음을 확인한다.
- 관련: `docs/MERGE_GUARDRAILS.md`, `.codex/rules/no-merge.rules`,
  `.githooks/`.

### 2026-06-12 - 기본 브랜치 이름을 `main`에서 `master`로 변경

- 상태: 차단
- 배경: 현재 GitHub 저장소 기본 브랜치는 `main`이지만, 사용자는 기본 브랜치
  이름을 `master`로 쓰고 싶다고 요청했다.
- 중요한 이유: 기본 브랜치 이름은 PR base, 로컬 sync 명령, GitHub 보호 규칙,
  문서 예시, 자동화 기본값에 영향을 준다. 원하는 이름으로 통일해두면 이후
  워크플로우가 덜 헷갈린다.
- 보류한 이유: 현재 인증된 GitHub 계정 권한은 `WRITE`이고, GitHub에서 기본
  브랜치 rename/default branch 변경은 repository admin 권한이 필요한 설정이다.
  또한 원격 기본 브랜치 변경 없이 로컬만 `master`로 바꾸면 추적 브랜치와 문서가
  서로 어긋난다.
- 다음 단계: repository owner/admin 권한으로 GitHub에서 `main` 브랜치를
  `master`로 rename하고 default branch가 `master`인지 확인한다. 이후 로컬에서
  `git branch -m main master`, `git fetch origin`,
  `git branch -u origin/master master`, `git remote set-head origin -a` 순서로
  추적 브랜치를 정리한다.
- 완료 기준: GitHub default branch가 `master`이고, 로컬 `master`가
  `origin/master`를 추적하며, 문서와 보호 규칙에서 기본 브랜치 표기가
  `master` 기준으로 정리되어 있다.
- 관련: `docs/MERGE_GUARDRAILS.md`, `.github/pull_request_template.md`,
  `AGENTS.md`.

### 2026-06-12 - Unity 프로젝트 기반 자동 포맷과 정적 분석 도입 검토

- 상태: 열림
- 배경: `.editorconfig`와 문서 컨벤션은 이미 있지만, C# formatter와 Roslyn
  analyzer를 자동으로 실행하거나 CI에서 강제하는 단계는 아직 없다.
- 중요한 이유: 자동 포맷과 정적 분석은 코드 스타일 논쟁을 줄이고, Unity/C#
  코드에서 실수, 불필요한 할당, 접근 제한 누락, 네이밍 불일치를 빨리 발견하게
  해준다.
- 보류한 이유: `PocketDodger` Unity 프로젝트는 생성됐지만, 이번 작업의 범위는
  게임 prototype과 로컬 실행/테스트 스크립트 안정화였다. formatter/analyzer는
  generated `.csproj` 재생성, Unity analyzer 호환성, CI 연결 여부를 별도 PR에서
  검증하는 편이 안전하다.
- 다음 단계: `PocketDodger`의 generated `.csproj`와 assembly definition 구조를
  기준으로 `dotnet format` 적용 가능성, `.editorconfig` 반영 여부, 필요한
  analyzer 수준을 작은 PR로 검증한다.
- 완료 기준: 로컬에서 실행 가능한 포맷/분석 명령이 문서화되고, Unity 프로젝트
  파일과 충돌하지 않으며, 필요하면 CI 또는 PR 체크로 연결된다.
- 관련: `.editorconfig`, `.gitattributes`, `docs/CONVENTIONS.md`.

### 2026-06-12 - Unity 프로젝트 기반 CI 검증 도입 검토

- 상태: 열림
- 배경: PR 템플릿과 로컬 검증 규칙은 추가했지만, GitHub Actions 같은 서버 측
  자동 검증은 아직 없다.
- 중요한 이유: CI가 있으면 로컬에서 빠뜨린 포맷, 문서, Unity batchmode import,
  Android build target 확인을 PR 단계에서 반복 가능하게 검증할 수 있다.
- 보류한 이유: `PocketDodger` 프로젝트와 로컬 `run`/`test` 스크립트는 생겼지만,
  GitHub Actions에서 Unity 라이선스, Android 모듈, 캐시, 실행 시간 비용을
  어떻게 다룰지 아직 검증하지 않았다.
- 다음 단계: 가장 작은 CI부터 추가한다. 우선 문서/라인 엔딩/금지 파일 검증을
  넣고, 이후 `PocketDodger/test`, Unity baseline verify, Android target 검증을
  분리해서 확장한다.
- 완료 기준: PR에서 자동으로 실행되는 최소 CI가 있고, 실패 시 원인을 확인할 수
  있으며, Unity 라이선스와 캐시 동작이 문서화된다.
- 관련: `.github/pull_request_template.md`, `docs/PR_REVIEW_CHECKLIST.md`,
  `docs/MOBILE_ANDROID.md`.

### 2026-06-12 - Unity YAML diff 검증 스크립트 도입

- 상태: 열림
- 배경: `git diff --check origin/main...HEAD`는 Unity가 생성한 `.meta`,
  `.asset`, `ProjectSettings` YAML의 trailing whitespace를 대량으로 보고한다.
- 중요한 이유: raw `git diff --check` 결과를 그대로 PR 게이트로 쓰면 실제
  hand-authored 오류와 Unity serializer 노이즈가 섞여 리뷰 신뢰도가 떨어진다.
- 보류한 이유: 이번 작업에서는 컨벤션과 PR 체크리스트에 기준을 명시했지만,
  hand-authored 파일만 검사하거나 Unity YAML 결과를 별도 분류하는 자동 스크립트는
  아직 만들지 않았다.
- 다음 단계: `scripts/check-pr.ps1` 같은 로컬 검증 스크립트를 만들고, Markdown,
  JSON, C#, PowerShell 등 hand-authored 파일에는 `diff --check`를 강제하되 Unity
  YAML trailing whitespace는 별도 요약으로 분류한다.
- 완료 기준: 로컬에서 한 명령으로 PR 검증을 실행할 수 있고, Unity YAML 노이즈와
  실제 텍스트 오류가 분리되어 보고된다.
- 관련: `docs/PR_REVIEW_CHECKLIST.md`, `.gitattributes`, Unity serialized YAML.

## 완료

## 폐기
