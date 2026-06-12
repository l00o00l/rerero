# Unity Mobile Game Conventions

These conventions are intentionally small. Add rules only after repeated need
or a real production risk.

## Baseline Sources

- Unity C# naming and style tips: https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity
- Unity project organization guidance: https://unity.com/how-to/organizing-your-project
- Microsoft C# coding conventions: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- Unity Smart Merge: https://docs.unity3d.com/6000.4/Documentation/Manual/SmartMerge.html
- Unity Editor serialization settings: https://docs.unity3d.com/6000.4/Documentation/Manual/class-EditorManager.html

## Project Layout

Owned project content should live under `Assets/_Project/`.

```text
Assets/
  _Project/
    Art/
    Audio/
    Editor/
    Materials/
    Prefabs/
    Scenes/
    ScriptableObjects/
    Scripts/
    Settings/
    Tests/
    UI/
  ThirdParty/
Packages/
ProjectSettings/
```

Rules:

- Keep `Assets/_Project/` free of vendor package internals.
- Put editor-only scripts under an `Editor/` folder.
- Keep runtime assemblies independent from editor-only code.
- Prefer feature folders inside `Scripts/` when systems grow:
  `Scripts/Gameplay`, `Scripts/UI`, `Scripts/Infrastructure`, etc.
- Avoid dumping shared code into `Common` unless the responsibility is clear.
- After creating a project from a Unity template, review and remove unused
  sample scenes, template scene assets, empty `Resources/` folders, and direct
  template packages before the project-shell PR is considered ready.

## Unity Version Control Settings

Unity project settings should use:

- Asset Serialization Mode: `Force Text`.
- Version control mode compatible with external Git tooling.
- `.meta` files visible and committed.

Scene, prefab, material, animation, and asset files are serialized YAML. They
should be treated as merge-sensitive files even when Git can text-merge them.

This machine is configured with UnityYAMLMerge for this Git repo. To configure a
new clone, run:

```powershell
$MergeTool = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Data\Tools\UnityYAMLMerge.exe"
git lfs install --local
git config merge.unityyamlmerge.name "Unity Smart Merge"
git config merge.unityyamlmerge.driver "'$MergeTool' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## C# Style

Naming:

- Types, methods, properties, public fields: `PascalCase`.
- Local variables and parameters: `camelCase`.
- Private instance fields: `_camelCase`.
- Private static fields: `s_camelCase`.
- Constants: `k_PascalCase`.
- Interfaces: `IName`.
- Boolean members should read as predicates: `isReady`, `hasTarget`,
  `CanMove`, `IsGameOver`.

Structure:

- One `MonoBehaviour` per file.
- File name must match the public type name.
- Use namespaces that reflect project ownership, for example
  `Thkim.Gameplay`, `Thkim.UI`, `Thkim.Infrastructure`.
- Keep serialized fields private unless external code must access them.
- Prefer properties or methods over public mutable fields for runtime state.
- Keep `Awake`, `OnEnable`, `Start`, `Update`, and `OnDisable` short and
  delegate work to named methods.

Formatting:

- Four spaces for C# indentation.
- Braces on new lines for types, methods, and control blocks.
- Prefer explicit types when `var` would hide meaning.
- Use `var` only when the type is obvious from the right-hand side.

## Unity Scripting Rules

- Prefer serialized references over runtime scene searches.
- Avoid adding singletons by default. Use them only for genuine process-wide
  services with clear lifetime and test strategy.
- Keep gameplay data in `ScriptableObject` assets when designers need to tune
  values or when the same data is reused by many objects.
- Avoid hard-coded scene names, tags, layers, and animator parameter strings in
  gameplay code. Centralize constants when string use is unavoidable.
- Do not introduce reflection, dynamic loading, or code generation in runtime
  paths without a clear platform reason.
- Avoid per-frame allocations in hot paths. Be careful with LINQ, closures,
  boxing, string interpolation, and repeated component lookups.
- Runtime UI text should update only when the displayed value changes. Avoid
  formatting strings or assigning `Text.text`/TMP text every frame when the
  value is unchanged.

## Assets

- Commit every asset with its `.meta` file.
- Do not move or rename Unity assets outside the Unity Editor unless the
  matching `.meta` file is moved or renamed in the same change.
- Source art, audio, video, and model files should use Git LFS according to
  `.gitattributes`.
- Generated artifacts and import caches belong outside Git.
- Avoid `Resources/` for new content unless the loading behavior is intentional
  and documented in the PR.
- Empty `Resources/` folders should be removed. Keeping one implies an
  intentional loading strategy.
- Prefer Addressables or explicit scene/prefab references for scalable content.

## Scenes And Prefabs

- Keep bootstrapping separate from gameplay content.
- Prefer additive scenes only when the loading model is explicit.
- Large scene or prefab edits should be isolated in their own PR when possible.
- Avoid concurrent editing of the same scene or prefab across branches.
- When a scene/prefab conflict occurs, use UnityYAMLMerge first, then inspect
  the result in Unity before committing.

## Dependencies

- Prefer Unity Package Manager packages over copied vendor source when possible.
- Pin package versions in `Packages/manifest.json` and commit
  `Packages/packages-lock.json`.
- Document third-party SDK purpose, owner, and platform impact in the PR.
- Do not add analytics, ads, attribution, crash, or social SDKs without a
  privacy and Android manifest review.

## Tests

- Put Edit Mode tests under `Assets/_Project/Tests/EditMode`.
- Put Play Mode tests under `Assets/_Project/Tests/PlayMode`.
- Every game with automated tests should provide a simple local wrapper such as
  `<GameName>/test.cmd` plus `scripts/run-tests.ps1`.
- Unity Test Runner scripts must fail when the result XML is missing or when
  the XML result is not `Passed`.
- Do not pass `-quit` with Unity `-runTests`; the Test Runner exits Unity after
  writing results.
- For gameplay logic, keep pure C# logic testable outside MonoBehaviours where
  practical.
- Every bug fix should either add a focused test or explain why automated
  coverage is not practical yet.
