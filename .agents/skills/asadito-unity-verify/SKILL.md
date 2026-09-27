---
name: asadito-unity-verify
description: "Verify Asadito in Unity Editor and through Unity MCP when available, including compilation, scene, hierarchy, Console and Play Mode."
---

# Asadito Unity Verify

Use Unity MCP when its tools are actually exposed in the active Codex session. Check Editor/instance, active scene, hierarchy and Console; verify Play Mode only for the acceptance items affected by the change.

If MCP tools are unavailable, check the project's Unity Editor log and report that live MCP inspection could not be completed. Do not claim compilation or Play Mode success from file inspection alone.

For unattended Play Mode checks, Unity may stop advancing when the Editor is backgrounded. Temporarily set `Application.runInBackground = true` in Play Mode, then stop Play Mode to restore the saved project state.

This project embeds MCP for Unity 10.2.0 at `http://127.0.0.1:8080/mcp`. Unity 6000.6.3f1 needs the local `UnityObjectIdCompat` fallback documented in `UNITY_MCP_SETUP.md`; preserve it unless the package/Editor compatibility issue is re-tested.

Canonical gate: `specs/acceptance/mvp-gate.md`. Group related edits, then perform one relevant compile/scene/Play Mode pass and check new Console errors.
