# ASADITO — project router

## Context rules

- Read [`docs/current-project-state.md`](docs/current-project-state.md) only when current behavior/scope matters; use [`docs/repo-map.md`](docs/repo-map.md) when the first path is not obvious.
- `specs/` are **on demand**: open the one relevant rule/acceptance page only when intent is unclear or a change may affect product behavior. User decisions take precedence; verify implementation against code/data.
- For large files, especially `AsaditoGame.cs`, use `rg` for a label/symbol, then read a narrow range and expand only as needed.
- Keep responsibilities separate: this file routes; skills give domain procedures; state summarizes current behavior; repo map locates files; specs define detailed requirements.

## Route by risk and scope

| Task | Context | First paths | Check |
|---|---|---|---|
| **Simple**: one color/copy/size/position/reference/control or obvious local bug | No skill/spec; map only if needed | Search label/symbol; often `AsaditoGame.cs` | Scoped diff; relevant compile/preview only. No suite/build. |
| **Medium behavior**: food input, flip, serving, hit target, gameplay animation | `asadito-sdd`; add `asadito-visual-mvp` for art/motion and `asadito-unity-verify` for Unity/test evidence | `FoodPieceTouch.cs`, `ServingBoardTouch.cs`, relevant `AsaditoGame.cs`/model | Compile and affected test/Editor flow. |
| **Medium content/art**: add food, atlas, substantial visual change | `asadito-sdd` for data/rules; `asadito-visual-mvp` for art; Verify for import/runtime | JSON, model, assets and levels from repo map | Validator + relevant tests/import/preview. |
| **Complex**: cooking, cross-system bug, architecture, progression/save | Relevant skills + affected spec(s); expand search progressively | Models/data/tests and implicated `AsaditoGame.cs` call sites | Focused tests first; broaden suite/validator/Editor by risk. |
| **Android/release** | `asadito-unity-verify` as applicable | `docs/android-release.md`, build script, `ProjectSettings/` | Build/install/smoke only for requested release scope. |
| **Testing/CI** | `asadito-unity-verify` | Requested fixture, validator, or workflow | Run requested/affected check; broaden only when risk requires. |

Classify by risk, uncertainty and systems touched—not prompt length. Start small; expand with evidence. No project-specific agent chain exists or is needed; avoid routine delegation.

## Validation, Git and Unity

- Match checks to risk: visual/config edits get focused reference/diff/preview; gameplay gets relevant compilation/tests; cross-system/release gets broader acceptance checks. Never claim a check that did not run.
- If workspace/branch is ambiguous, verify Git root/status/branch. Finish with `git diff --check` and review the scoped diff. Commit/push only when explicitly requested or authorized; verify remote state first and never force-push.
- Use Engram selectively for durable decisions; no secrets, full source, logs or duplicate docs. If syncing the versioned local store, include manifest/chunks only, never its local database.
- Unity version is in `ProjectSettings/ProjectVersion.txt`; enabled scene is `Assets/Scenes/SampleScene.unity`. Do not change scenes/GameObjects/assets unless needed for scope. Keep evidence honest and update current state when operational facts change; dated QA records belong in `docs/ai/`.
