# Unity Project Structure

This guide keeps Unity projects in this workspace usable for both mobile and
non-mobile games without reorganizing every project when a new platform appears.

## Principle

Keep gameplay, rules, data, and shared project assets platform-neutral by
default. Add platform-specific folders and scripts only where platform behavior,
build settings, input, packaging, or smoke testing actually differs.

Do not name a root project or common gameplay folder after a platform unless the
game is truly platform-exclusive. Prefer `DreamLaundromat/Assets/_Project/` over
`DreamLaundromat/Assets/Mobile/`.

## Repo-Level Layout

Use one Unity project directory per game:

```text
thkim/
  docs/
  concepts/
  shared-unity/
  <GameName>/
    Assets/
    Packages/
    ProjectSettings/
    docs/
    scripts/
    run.cmd
    test.cmd
```

Repo-level `docs/` is for shared workflow, conventions, and platform guidance.
Game-local `docs/` is for that game's plan, design notes, verification notes,
and feature backlog.

## Game-Local Unity Layout

Use this baseline under each Unity project:

```text
<GameName>/
  Assets/
    _Project/
      Art/
      Audio/
      Editor/
        BuildPipeline/
      Materials/
      Prefabs/
      Scenes/
      ScriptableObjects/
      Scripts/
        Gameplay/
        Infrastructure/
        Input/
        Levels/
        Rules/
        UI/
      Settings/
      Tests/
        EditMode/
        PlayMode/
      UI/
    ThirdParty/
  Packages/
  ProjectSettings/
  docs/
    PLAN.md
  scripts/
  run.cmd
  test.cmd
```

This structure is valid for mobile, desktop, and other Unity targets. Do not add
platform directories until there is real platform-specific content.

## Platform-Specific Additions

When platform differences are real, add narrow platform-specific locations:

```text
Assets/_Project/
  Editor/
    BuildPipeline/
      Android/
      Windows/
  Scripts/
    Platform/
      Android/
      Desktop/
  Settings/
    Android/
    Windows/
scripts/
  run-emulator.ps1
  run-windows.ps1
  build-android.ps1
  build-windows.ps1
```

Use these folders for platform integration, not general gameplay. A puzzle rules
engine, level model, or shared UI state should stay outside platform folders
unless it truly cannot run on other targets.

## Run And Build Scripts

Each runnable game should have a simple root wrapper:

```text
<GameName>/run.cmd
<GameName>/test.cmd
```

`run.cmd` may delegate to the default target for that game. For multi-platform
games, prefer explicit platform scripts:

- Android: `scripts/run-emulator.ps1`, `scripts/build-android.ps1`
- Windows: `scripts/run-windows.ps1`, `scripts/build-windows.ps1`

Avoid hiding the target platform behind a generic script when the platform
matters for verification or release settings.

## Implementation Planning

Every game-local implementation plan should state:

- Primary target platform for the PR.
- Secondary platforms, if any.
- Input model and screen/orientation assumptions.
- Build target and run script to verify.
- Manual checks that remain platform-specific.

For example, a mobile puzzle prototype should explicitly say Android portrait,
one-hand tap input, emulator smoke, and real-device manual check. A desktop game
should instead state windowed/fullscreen behavior, mouse/keyboard or controller
input, and the relevant desktop build target.

## Review Points

Review platform structure when:

- A folder name implies a platform but contains platform-neutral code.
- Android scripts are reused for non-Android games.
- A build pipeline script silently changes project-wide settings for another
  platform.
- Platform-specific settings are mixed into gameplay rules or data models.
- A PR claims platform verification without running the platform's build or
  smoke path.
