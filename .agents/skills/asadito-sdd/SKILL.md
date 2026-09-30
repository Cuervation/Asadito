---
name: asadito-sdd
description: "Use when changing Asadito gameplay rules, food/content data, MVP scope, or acceptance behavior."
---

# Asadito SDD

- Start with [`docs/repo-map.md`](../../../docs/repo-map.md); read the one relevant spec only when changing a rule, resolving intent, or touching acceptance. Specs are not default context.
- Scope/progression: `specs/product/mvp-scope.md`. Heat/cooking: `specs/product/systems/fire-heat.md` and `food-cooking.md`. Serving/scoring: the corresponding `specs/product/systems/` page. Acceptance changes: `specs/acceptance/mvp-gate.md`.
- Small implementation changes already defined by a spec need no new document. User decisions take precedence; verify behavior against runtime/data rather than assuming docs are current.
- Food content is data-driven: inspect `FoodCatalog.json`, the relevant runtime model, level catalog/assets and tests via the repo map; do not add a one-off per-food code path.
- Validate the affected model/system first; add PlayMode/Editor checks only when runtime integration or interaction changes. Never report a test/build as passed if it did not run.

Do not use dated audit/status reports as active gameplay instructions; see [`docs/current-project-state.md`](../../../docs/current-project-state.md).
