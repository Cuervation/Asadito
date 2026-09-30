---
name: asadito-unity-verify
description: "Use when a change needs Unity compilation, asset/scene import, Editor inspection, PlayMode, or Android build evidence."
---

# Asadito Unity Verify

- Route to affected scene/scripts/tests using [`docs/repo-map.md`](../../../docs/repo-map.md). Use Unity MCP only if its tools are available in this session.
- Run the smallest useful check: compile/import for C# or asset wiring; focused EditMode for model/data rules; PlayMode/Editor interaction only for changed runtime acceptance; Android build/install only for release/build tasks.
- Inspect Console only after running relevant Unity work. If MCP is unavailable, state exactly what could not be checked; file inspection or old logs do not prove current compilation/PlayMode.
- The Editor may stop advancing in background runs. If a run reports `editor_unfocused`/timeout, do not count it as a pass; focus the existing Editor and retry only when feasible.
- For bridge setup or the known Editor compatibility fallback, consult `UNITY_MCP_SETUP.md` only when relevant.
- For release scope, follow `docs/android-release.md` and `specs/acceptance/mvp-gate.md`; keep package/SDK/build claims tied to actual artifacts/results.

Current project state and dated QA limitations: [`docs/current-project-state.md`](../../../docs/current-project-state.md), [`docs/ai/mvp-status.md`](../../../docs/ai/mvp-status.md).
