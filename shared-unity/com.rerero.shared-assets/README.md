# Rerero Shared Assets

Reusable Unity assets for small game prototypes in this workspace.

## Layout

- `Runtime/Art/`: shared sprites, textures, materials, prefabs, and visual assets.
- `Runtime/Audio/`: shared sound effects and music loops.
- `Runtime/UI/`: shared UI art and reusable UI prefabs.
- `Editor/`: editor-only import helpers or validation scripts.
- `LICENSES/`: license files and source notes for third-party assets.

## Asset Rules

- Keep third-party assets under a provider/pack folder.
- Add the original source URL and license file before using an external pack.
- Prefer permissive assets such as CC0 for early prototypes.
- Do not put game-specific tuning, scenes, or gameplay prefabs here.
