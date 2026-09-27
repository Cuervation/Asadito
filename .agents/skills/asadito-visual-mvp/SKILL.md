---
name: asadito-visual-mvp
description: "Create or integrate Asadito visual assets and interactions, preserving the Visual Bible, asset status and early animation/feedback requirements."
---

# Asadito Visual MVP

Read `specs/visual/` for style/states and `docs/art/asset-manifest.md` before generating or replacing game art.

- Use ImageGen for new raster game assets; copy final project assets into `Assets/` and mark them FINAL or PROVISIONAL. Mark reference-only output CONCEPT. Use only FINAL/PROVISIONAL/CONCEPT/TODO. Concepts are not runtime assets.
- Do not overwrite art already in the project; create a versioned sibling and update references when replacing.
- Core interactions need movement plus clear visual feedback early: cooking/embers, turning, placing on tray, serving and guest reaction.
- Call art DONE only after the asset imports in Unity and remains readable at mobile UI size over the gameplay background.
