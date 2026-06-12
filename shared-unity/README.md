# Shared Unity Packages

Common Unity packages used by games in this workspace live here.

Use local package references from each Unity project instead of copying shared
assets into every game's `Assets/` folder.

Example dependency from a game project:

```json
"com.rerero.shared-assets": "file:../../../shared-unity/com.rerero.shared-assets"
```
