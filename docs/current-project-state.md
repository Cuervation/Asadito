# ASADITO — current operational state

**Checked:** 2026-09-30. Canonical quick reference for active behavior; verify code/data when changing it. Dated QA detail: [`docs/ai/current-state.md`](ai/current-state.md), [`docs/ai/mvp-status.md`](ai/mvp-status.md).

- **Scope:** 12 progressive levels, 18 data-driven foods, six visual cooking states each. L1 is the tutorial, not the content limit.
- **Loop:** one portrait scene; runtime UI in `Assets/Asado/Scripts/AsaditoGame.cs`. Grill starts uniformly hot (210 °C); no ignition, charcoal, fuel or movable embers.
- **Input:** tap food to select/manipulate with tongs; drag to move or plate on the serving board; tap selected food to flip; double-tap the board to serve a complete order.
- **Core data:** `FoodCatalog.json`/`FoodCatalog`, `FoodCookingModel`/`FoodState`, `GrillHeatModel` and `MvpLevelCatalog`. See [`repo-map.md`](repo-map.md).
- **Out of scope:** economy/shop, restaurant management, multiplayer, walking NPCs, other appliances and charcoal management.
- **Latest documented QA:** footprint update EditMode 21/21 and validator PASS; its PlayMode run stopped at 8/10 (`editor_unfocused`). Physical touch/performance/art sign-off remain unverified. Earlier 10/10 is not evidence for that update.

Update this page when live behavior changes. Detailed rules stay in the one relevant spec; test/build evidence stays dated in `docs/ai/`.
