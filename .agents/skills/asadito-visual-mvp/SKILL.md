---
name: asadito-visual-mvp
description: "Use for new/replaced raster art, visual-state assets, gameplay animation/feedback, or substantial composition changes."
---

# Asadito Visual MVP

- Use [`docs/repo-map.md`](../../../docs/repo-map.md) to find actual asset paths and runtime consumers. Load only the relevant section of `specs/visual/`; do not read the full Visual Bible for a local code-only color/offset.
- For new/replaced game art, follow `specs/visual/art-direction.md`, `docs/art/asset-manifest.md`, and the Unity import workflow. Use ImageGen for new raster assets; retain existing art by making versioned alternatives, and record status as FINAL/PROVISIONAL/CONCEPT/TODO. Concepts are not runtime assets.
- Current gameplay is always-hot and top-down: food, tongs, grill and serving board are the affordances. Do not design charcoal/ember/ignition or named food-action button interactions.
- Check animation-specific rules in `specs/visual/animation.md` and `docs/art/animation-manifest.md` only when changing motion/feedback. Verify Unity import and mobile-size readability when art/runtime assets change.

Current behavior and QA boundaries: [`docs/current-project-state.md`](../../../docs/current-project-state.md).
