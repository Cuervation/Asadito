---
name: asadito-unity-verify
description: "Use when a change needs Unity compilation, asset/scene import, Editor inspection, PlayMode, or Android build evidence."
---

# Asadito Unity Verify

Route through [`docs/repo-map.md`](../../../docs/repo-map.md), inspect changed paths/dependencies, then run the lowest-cost check that covers the risk. Never run the full suite or Android build automatically.

## Risk levels and minimum validation

| Level | Examples | Minimum sufficient checks |
|---|---|---|
| **1 — Trivial** | Color, copy, position/scale, reference, hide a control, non-critical constant | Review scoped diff/references and affected scene/prefab/config. Preview only if visible layout/readability can regress. If no executable C# changed, no compile or gameplay test. No suite/build. |
| **2 — Local** | One control, food placement, isolated condition/component | Relevant compile/import and one focused test if available; otherwise exercise only that system. Exclude unrelated domains. |
| **3 — Medium** | New food/data, serving change, local cooking behavior, logic animation, 2–3 connected systems | Compile, affected EditMode tests/validator, and one directed integration/PlayMode check if runtime flow changed. No full suite unless evidence says scope is broader. |
| **4 — Transversal** | Cross-domain refactor, shared gameplay core, level/save flow, architecture change | Compile; broad tests for touched domains; run the full suite only when shared contracts, multiple core systems, migration or acceptance-wide behavior is at risk and focused checks cannot give adequate confidence. A large file/diff alone is not enough. |
| **5 — Release/build** | Explicit Android build/release, platform/build-setting change, release sign-off | Full required suites and content validator, build, install/smoke and release checks from `docs/android-release.md` / `specs/acceptance/mvp-gate.md`. |

## Selection and fail-fast

1. Start with `git diff --name-only`; map changed behavior to its real dependencies. File location suggests tests but does not prove coverage.
2. Check diff/syntax and local references, then run a specific test/filter, then directed integration, then broad suite, then build—in that order.
3. Relevant fixtures: `FoodCatalogTests`, `HeatAndCookingTests`, `ServingAndProgressTests` (EditMode), `FirstPlayableFlowTests` (PlayMode); content uses `Tools/validate_food_content.py`.
4. Keep a task-local record of passed checks and which changed paths/behaviors they cover. Do not rerun them unless covered files/dependencies change or new evidence raises risk.
5. Fix a focused failure before escalating; stop when evidence covers the changed contract. Expand after a real cross-system dependency, unexplained failure or inadequate coverage—not habit.

## Examples

| Request | Route/check |
|---|---|
| “Cambiá el color de un botón” | **Trivial:** diff + local/Game View visual check if needed; no suite/build. |
| “Hacé más grande el vacío” | **Local/medium:** check food definition, `FoodFootprintLayout`/hit target and initial placement; focused catalog/footprint test or validator. No full suite. Runtime uses data/hit targets, not a per-food collider unless one is introduced. |
| “Mové la tabla” | **Trivial** if display-only: inspect `BuildInterface` and preview. **Local** if raycast/drop bounds change: one serving/touch check. No gameplay suite by default. |
| “Sacá el botón Servir” | Search UI labels/references; current flow serves by double-tapping the board. If only confirming/removing stale UI, verify UI/board references; test serving only if input behavior changes. |
| “Hacé doble click para servir” | **Medium:** compile plus `ServingAndProgressTests` and the affected PlayMode serving flow. |
| “Refactorizá AsaditoGame” | **Transversal only if responsibilities/contracts cross domains:** compile + affected suites; full suite if broad shared flows are affected, not merely because the file is large. |
| “Prepará build Android” | **Release:** full required validation, ARM64 Android build and install/smoke checks. |

Reuse an already-open Editor/session; do not reopen Unity, reimport everything or rebuild Android for a local edit. Inspect Console only after relevant Unity work. If MCP is unavailable, state what could not be checked; old logs/file inspection do not prove current compilation or PlayMode. `editor_unfocused`/timeout is not a pass; focus the Editor and retry only if useful.

For bridge setup/compatibility consult `UNITY_MCP_SETUP.md` only when needed. Current behavior and dated QA limits: [`docs/current-project-state.md`](../../../docs/current-project-state.md), [`docs/ai/mvp-status.md`](../../../docs/ai/mvp-status.md).
