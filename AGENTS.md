# ASADITO — project router

## Context rules

- Read [`docs/current-project-state.md`](docs/current-project-state.md) only when current behavior/scope matters; use [`docs/repo-map.md`](docs/repo-map.md) when the first path is not obvious.
- `specs/` are **on demand**: open the one relevant rule/acceptance page only when intent is unclear or a change may affect product behavior. User decisions take precedence; verify implementation against code/data.
- For large files, especially `AsaditoGame.cs`, use `rg` for a label/symbol, then read a narrow range and expand only as needed.
- Keep responsibilities separate: this file routes; skills give domain procedures; state summarizes current behavior; repo map locates files; specs define detailed requirements.

## Route by risk and scope

| Task | Context | First paths |
|---|---|---|
| **Simple UI/local bug** | No skill/spec; map only if needed | Search label/symbol; often `AsaditoGame.cs` |
| **Gameplay interaction** | `asadito-sdd`; Visual for art/motion; Verify for Unity evidence | `FoodPieceTouch.cs`, `ServingBoardTouch.cs`, relevant `AsaditoGame.cs`/model |
| **Content/art** | SDD for data/rules; Visual for art; Verify for import/runtime | JSON, model, assets and levels via repo map |
| **Complex cooking/progression/save** | Relevant skills + affected spec(s) | Models/data/tests and implicated `AsaditoGame.cs` call sites |
| **Android/release or testing/CI** | `asadito-unity-verify` | `docs/android-release.md`, `ProjectSettings/`, requested test/validator/workflow |

Classify by risk, uncertainty and systems touched—not prompt length. Start small; expand with evidence. No project-specific agent chain exists or is needed; avoid routine delegation.

## Validation, Git and Unity

- Testing is proportional to risk: do not run the full suite or build by default. Use `asadito-unity-verify` for test selection, escalation and stop criteria; never claim a check that did not run.
- If workspace/branch is ambiguous, verify Git root/status/branch. Check whitespace and review the scoped diff; leave unrelated worktree changes out of validation/commit. Commit/push only when explicitly requested or authorized; verify remote state first and never force-push.
- Use Engram selectively for durable decisions; no secrets, full source, logs or duplicate docs. If syncing the versioned local store, include manifest/chunks only, never its local database.
- Unity version is in `ProjectSettings/ProjectVersion.txt`; enabled scene is `Assets/Scenes/SampleScene.unity`. Do not change scenes/GameObjects/assets unless needed for scope. Keep evidence honest and update current state when operational facts change; dated QA records belong in `docs/ai/`.
