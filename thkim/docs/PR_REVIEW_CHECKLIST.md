# Pull Request Review Checklist

Use this checklist for human review and Codex review. Findings should be
ordered by severity and include file/line references when possible.

## PR Summary Requirements

Every PR should explain:

- what changed
- why it changed
- how it was verified
- screenshots or recordings for visible UI/gameplay changes
- known risks or follow-up work

## Required Local Checks

At minimum, report:

```powershell
git status --short
```

Then include the relevant checks for the change:

- C# tests or Unity test runner results
- game-local test wrapper, for example `.\<GameName>\test`
- Unity batchmode import/build target check
- target-platform build result, such as Android APK/AAB or Windows player
- target-platform smoke test, such as emulator, physical device, or desktop run
- `adb logcat` notes for runtime issues

For `git diff --check`, distinguish hand-authored files from Unity-generated
YAML. Raw full-PR checks can report Unity serializer whitespace; do not present
that as a clean full-PR check unless the command actually covered the full diff.

## General Code Review

- Does the change solve the stated problem without unrelated refactors?
- Are edge cases and failure states handled?
- Are public APIs, serialized fields, and asset references stable?
- Is the code understandable without excessive comments?
- Are tests added or consciously deferred with a reason?
- Are errors logged with enough context but without leaking secrets?

## Unity Review

- Are `.meta` files present for every new/moved asset?
- Are generated folders excluded from Git?
- Did scenes, prefabs, or assets change unexpectedly?
- Are scene/prefab changes small enough to review?
- Are serialized references assigned and not relying on fragile lookups?
- Are package changes pinned in `manifest.json` and `packages-lock.json`?
- Are editor-only classes isolated under `Editor/` or editor assemblies?
- Were Unity template leftovers removed or justified: sample scenes, template
  scene assets, empty `Resources/`, default input actions, and unused direct
  packages?
- Does the folder/script layout follow `docs/UNITY_PROJECT_STRUCTURE.md`?
- Are platform-specific folders used only for real platform-specific behavior?

## Platform Review

- Is the PR's primary target platform explicit?
- Are input, screen/orientation, build target, run script, and smoke path clear?
- Is platform-neutral gameplay code kept out of platform-specific folders?
- Could project-wide platform settings break another target?

## Mobile Performance Review

- Does gameplay code allocate in `Update`, physics callbacks, animation events,
  UI refresh loops, or other hot paths?
- Are repeated component lookups cached where appropriate?
- Are LINQ, closures, boxing, reflection, and string formatting avoided in hot
  paths?
- Are UI labels/text refreshed only when values change instead of every frame?
- Are textures, audio, shaders, particles, and post-processing appropriate for
  target mobile devices?
- Does the change affect startup time, memory footprint, battery, or thermals?
- Is profiling needed before accepting the change?

## Android Release Review

- For release changes, is the output AAB rather than APK?
- Are IL2CPP and ARM64 enabled for release?
- Does the target API level meet current Google Play requirements?
- Are development build and script debugging disabled for release?
- Are signing secrets excluded from Git?
- Are Android permissions justified?
- Are third-party SDKs checked for manifest changes, native libraries, privacy,
  and 16 KB page-size compatibility?

## Git And Asset Review

- Are large binary assets handled by Git LFS according to `.gitattributes`?
- Are build outputs, logs, crash dumps, and memory captures ignored?
- Are renames represented cleanly instead of delete/add churn?
- Are Unity YAML conflicts resolved with UnityYAMLMerge and verified in Unity?

## Security And Privacy Review

- No credentials, tokens, private keys, keystores, or service account files.
- No accidental collection of device identifiers, location, contacts, or other
  sensitive data.
- No new network endpoint without purpose and ownership.
- No debug backdoor, test menu, or verbose logging in release builds.

## Review Modes

Use these modes when asking Codex or a human for focused review:

- `general`: correctness, regressions, maintainability
- `unity`: assets, scenes, prefabs, serialization, packages
- `platform`: target platform assumptions, build/run scripts, folder placement
- `mobile-performance`: allocations, frame time, memory, battery
- `android-release`: AAB, API level, IL2CPP, ARM64, signing, permissions
- `security-privacy`: secrets, SDK behavior, permissions, data flow
