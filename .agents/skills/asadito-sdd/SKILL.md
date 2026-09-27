---
name: asadito-sdd
description: "Use Asadito's lightweight product specs and acceptance flow when changing game rules, MVP scope, or gameplay implementation."
---

# Asadito SDD

For a meaningful feature, use the smallest applicable chain: spec → acceptance → implementation → Unity validation. Small changes already defined by the current spec do not need new documents.

- User decisions override specs; specs override acceptance/code when they conflict.
- Canonical product docs are `specs/product/mvp-scope.md`, `specs/product/systems/`, `specs/acceptance/mvp-gate.md` and `specs/visual/`. Legacy pages link to these; do not duplicate rules there.
- First Playable is level 1 of five: two guests, chorizo and tira, charcoal, fire preparation/ember movement, thermal cooking, tray, service allocation, evaluation, stars/progress/save and retry. No butcher shop.
- Keep `docs/ai/current-state.md` to the milestone, validation, blockers and three next steps.
- Delegate only independent work that saves context/risk. Give agents specific paths and acceptance and choose the cheapest capable model; escalate after a failed attempt or for delicate system design.
- Routing: Product/SDD handles rules and acceptance; Unity Gameplay handles C#/scene/simulation; Visual/Animation handles assets and feedback; QA handles the Unity acceptance pass. Use A/low for search and edits, B/low–medium for routine implementation, C/high only for difficult simulation/architecture or repeated failures.
