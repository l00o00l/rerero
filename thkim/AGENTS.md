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
- Do not commit generated Unity folders: `Library/`, `Temp/`, `Obj/`, `Logs/`,
  `UserSettings/`, build outputs, or local smoke-test logs.
- Commit `.meta` files with their assets. A missing `.meta` file is a bug.
- Do not hand-edit scene, prefab, or asset YAML unless the change is narrow,
  reviewed carefully, and safer than using Unity APIs.
- Prefer Unity Editor scripts or batchmode commands for project settings,
  imports, builds, and asset generation.
- Treat keystores, passwords, API keys, signing configs, and store credentials
  as secrets. Do not add them to the repository.

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

For PR-facing work, include:

- `git status --short`
- commands/tests/builds run
- known gaps or manual checks still needed

## Review Posture

Review findings must lead the response and include file/line references when
available. Prioritize correctness, regressions, build failures, missing tests,
Unity asset/meta risks, mobile performance, Android release settings, and
security/privacy issues.
