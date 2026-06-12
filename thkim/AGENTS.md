# Codex Project Guidance

## Scope

These instructions apply to the Unity mobile game project under this directory.
Prefer local conventions in this file and `docs/` over generic Unity advice.

## Environment

- Primary shell: PowerShell on Windows.
- Unity Editor: `C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe`.
- Android toolchain: Unity-installed Android SDK, NDK, and OpenJDK.
- GitHub workflow: use `gh` when interacting with pull requests.

## Work Principles

- Keep changes small and directly tied to the request.
- Codex must never merge pull requests or push directly to protected branches.
  PR work is limited to creating, updating, and reviewing PRs. A human must
  perform any merge in GitHub after the required review gates pass.
- Do not run `gh pr merge`, `git merge`, GitHub merge API calls, or direct
  pushes to `main`/`master`. If asked to merge, refuse briefly and explain that
  the repository is configured for human-only merges.
- Do not commit generated Unity folders: `Library/`, `Temp/`, `Obj/`, `Logs/`,
  `UserSettings/`, build outputs, or local smoke-test logs.
- Commit `.meta` files with their assets. A missing `.meta` file is a bug.
- Do not hand-edit scene, prefab, or asset YAML unless the change is narrow,
  reviewed carefully, and safer than using Unity APIs.
- Prefer Unity Editor scripts or batchmode commands for project settings,
  imports, builds, and asset generation.
- Every runnable game should keep its local run scripts inside that game's
  directory. Prefer a simple `run.cmd` wrapper plus
  `scripts/run-emulator.ps1` for Android emulator smoke runs.
- Every tested game should keep its local test scripts inside that game's
  directory. Prefer a simple `test.cmd` wrapper plus `scripts/run-tests.ps1`
  that fails when Unity Test Runner XML is missing or failed.
- Treat keystores, passwords, API keys, signing configs, and store credentials
  as secrets. Do not add them to the repository.

## Deferred Work Tracking

- Use `docs/TODO.md` for deferred work that affects the broader development
  environment, shared workflow, repository policy, project infrastructure, or
  future productivity across tasks.
- Add or update a TODO entry before finishing the task when such deferred work
  is identified. Include the reason it matters, why it was deferred, and the
  smallest useful next step.
- Do not use `docs/TODO.md` for ordinary feature backlog, missed steps from an
  implementation plan, polish ideas for a single game feature, or task-local
  cleanup. Keep those in the relevant plan document, PR notes, or issue tracker.
- Follow the entry format defined in `docs/TODO.md`.
- Write TODO entries in Korean. Keep literal paths, commands, API names, and
  code identifiers in their original form when that is clearer.
- Do not add vague ideas, speculative wishlist items, or work that was rejected
  as unnecessary. If an existing TODO becomes obsolete or completed, update its
  status instead of leaving stale guidance.
- Mention any TODO updates in the final response.

## Unity Conventions

- Project code and owned assets belong under `Assets/_Project/`.
- Third-party packages and imported vendor assets belong under `Assets/ThirdParty/`
  or `Packages/`, depending on how they are distributed.
- Use one `MonoBehaviour` per file, with file name matching class name.
- Prefer explicit serialized dependencies over scene-wide lookup. Avoid broad
  use of `FindObjectOfType`, `GameObject.Find`, and string-based object lookup.
- Avoid per-frame allocations in gameplay paths. Treat `Update`, coroutines,
  LINQ, closures, boxing, and string formatting in hot paths as review points.
- Use Addressables or explicit references for scalable runtime content. Do not
  add new `Resources/` usage without a clear reason.

## Verification

For code-only changes, run the fastest relevant verification available.
For Unity project changes, prefer batchmode checks and Android target import:

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"
& $Unity -batchmode -quit -projectPath <project-path> -buildTarget Android -logFile <log-path>
```

For runnable Android games, keep the run script working and mention it in PR
verification when it is relevant.

For Unity Test Runner checks, prefer the game-local `test.cmd` wrapper when it
exists. Do not use `-quit` together with `-runTests`.

For PR-facing work, include:

- `git status --short`
- commands/tests/builds run
- known gaps or manual checks still needed
- confirmation that no merge or protected-branch push was performed

## Review Posture

Review findings must lead the response and include file/line references when
available. Prioritize correctness, regressions, build failures, missing tests,
Unity asset/meta risks, mobile performance, Android release settings, and
security/privacy issues.
