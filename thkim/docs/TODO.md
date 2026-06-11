# TODO

이 문서는 할 만하고 가치가 있지만, 사용자의 선택, 현재 작업 범위, 접근 권한,
시점 문제 때문에 의도적으로 미룬 작업을 기록한다.

일반적인 위시리스트로 쓰지 않는다. 각 항목에는 왜 중요한지, 왜 미뤘는지,
가장 작게 시작할 수 있는 다음 단계가 포함되어야 한다.

## 작성 기준

- 지금 당장 하지 않기로 했지만, 작업할 가치와 이유가 분명한 항목만 적는다.
- 단순 아이디어, 막연한 개선, 이미 불필요하다고 판단한 일은 적지 않는다.
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

### 2026-06-12 - Unity 프로젝트 생성 후 자동 포맷과 정적 분석 도입 검토

- 상태: 대기
- 배경: `.editorconfig`와 문서 컨벤션은 이미 있지만, C# formatter와 Roslyn
  analyzer를 자동으로 실행하거나 CI에서 강제하는 단계는 아직 없다.
- 중요한 이유: 자동 포맷과 정적 분석은 코드 스타일 논쟁을 줄이고, Unity/C#
  코드에서 실수, 불필요한 할당, 접근 제한 누락, 네이밍 불일치를 빨리 발견하게
  해준다.
- 보류한 이유: 아직 Unity 프로젝트가 생성되지 않아 실제 `.csproj`, `.sln`,
  `Packages/manifest.json`, assembly definition 구조가 없다. 이 상태에서
  `dotnet format`, analyzer 패키지, CI 검증을 정하면 실제 Unity 생성물과
  맞지 않거나 검증할 수 없는 설정이 될 수 있다.
- 다음 단계: Unity 프로젝트 생성 후 generated `.csproj`와 package 구조를
  확인하고, `dotnet format` 적용 가능성, `.editorconfig` 반영 여부, 필요한
  analyzer 수준을 작은 PR로 검증한다.
- 완료 기준: 로컬에서 실행 가능한 포맷/분석 명령이 문서화되고, Unity 프로젝트
  파일과 충돌하지 않으며, 필요하면 CI 또는 PR 체크로 연결된다.
- 관련: `.editorconfig`, `.gitattributes`, `docs/CONVENTIONS.md`.

## 완료

## 폐기
