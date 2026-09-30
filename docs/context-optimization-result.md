# Context-routing optimization — result

This is a short implementation note for the audit in [`agents-skills-context-audit.md`](agents-skills-context-audit.md). No gameplay or Unity runtime files were changed.

## Before → after

- **Before:** task → informal skill/spec choice → rediscover paths → possibly open the 121 KB `AsaditoGame.cs` or several status docs; old five-level/charcoal guidance could misroute work.
- **After:** task → short `AGENTS.md` route and risk class → only the relevant skill (if any) → `repo-map.md`/`rg` → narrow file range → risk-proportional check. Specs and dated QA reports are on demand.

## Six routing checks

| Request | Load / first files | Skip | Proportionate check |
|---|---|---|---|
| Change a button color | AGENTS → map if needed → `rg` label; relevant `AsaditoGame.cs` builder/snippet | State/specs/skills, Unity suite/build | Diff; preview if contrast/layout can change; compile only if C# changed. |
| Make vacío larger | SDD → food-sizing map entries → `FoodCatalog.json`, `FoodFootprintLayout.cs`; food-cooking spec only if semantics are unclear | Whole catalog/art Bible, unrelated systems | Food validator + affected EditMode sizing tests; preview/PlayMode if touch bounds or layout changed. |
| Move table next to grill | Visual skill → layout map → `BuildInterface` range and existing `MesitaAsador`/`TablaAsador` art | Cooking/scoring specs, all art inventory | Editor preview for composition; affected serving/touch check if hit region moves. |
| Remove Serve button | AGENTS/map → `rg` action label in `AsaditoGame.cs`; confirm `ServingBoardTouch.cs` flow | New spec/skill if confirming it is already absent | Text/symbol search; if behavior changes, serving spec + targeted serving test. Current runtime serves by double-tapping a full board. |
| Add a new cut | SDD + Visual; scope/food-cooking/food-states specs → JSON, catalog/model/sprite loader, atlas, level catalog, tests/validator | Unrelated scoring/UI/Android unless affected | Validator, affected EditMode, import/visual check, PlayMode when runtime flow changes. |
| Fix complex cooking bug | SDD + Unity Verify → heat/cooking specs → `FoodCookingModel`, `FoodState`, `GrillHeatModel`, data and `HeatAndCookingTests` | UI/art/Android unless evidence points there | Focused EditMode first; affected PlayMode and broader suite only if cross-system impact warrants it. |

The first color-change route needs no skill or spec by default and should reach a relevant source snippet after a label search. Escalate context only when the first evidence exposes uncertainty or wider impact.

## Files changed and routing policy

- Updated `AGENTS.md`, `README.md`, all three `.agents/skills/*/SKILL.md`, and clarified the current serving interaction in `specs/acceptance/mvp-gate.md`.
- Added `docs/current-project-state.md` (short operational behavior), `docs/repo-map.md` (paths plus verified monolith symbols), and this comparison.
- Dated `docs/ai/current-state.md` and `docs/ai/mvp-status.md` remain evidence/history; they now point to the operational source. The audit received only a follow-up pointer; its original dated diagnosis remains intact, and its “pending” recommendations now link here.
- No specialized agents were created. `AGENTS.md` now gives only the general rule “testing is proportional to risk”; the five-level selection/fail-fast matrix lives in `.agents/skills/asadito-unity-verify/SKILL.md` to avoid duplicated policy.

This documents a routing design, not runtime token/latency telemetry. It reduces unnecessary pre-edit exploration by making first paths and expansion criteria explicit.
