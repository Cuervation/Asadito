# Asadito contributor contract

## Product and validation

- Treat `specs/` as the source of truth for scope, systems, and acceptance criteria. Keep the SDD concise and update it when a product decision changes.
- The active milestone is Visual Slice: the First Playable level-1 loop has passed automated PlayMode in Unity. Do not move to MVP Content until the visual-slice art and motion gate is reviewed in Editor; broader L2–L5 end-to-end, normal pacing, and device QA remain later MVP gates.
- Keep the MVP visual first: prioritize the grill, food states, and readable animation/feedback. Avoid adding content outside the five-level MVP.
- Unity MCP, EditMode tests, and an actual Editor playtest provide validation. Report clearly when a check could not run; old build logs do not validate changed code.

## Git and source of truth

- The only official remote is `https://github.com/Cuervation/Asadito.git`, branch `main`.
- Git stores project code, specs, docs, tests, Unity settings/packages, and versionable art. Do not commit Unity generated folders, local databases, credentials, tokens, or private keys.
- Make small commits at coherent milestones or feature boundaries, then push validated work to `main`. Inspect remote state and integrate it before pushing. Never force-push.

## Engram memory

- Use the official `Gentleman-Programming/engram` installation configured for Codex. At task start, retrieve only memories relevant to the task; consult specs and current-state docs first for exact current project behavior.
- Save durable product, architecture, Unity, art, debugging, or milestone decisions. Memory records context and rationale, not secrets, full code, logs, or copies of repository docs.
- Use Git Sync at meaningful checkpoints. Commit `.engram/manifest.json` and `.engram/chunks/`; keep the local Engram database ignored.
- For delegated work, pass only relevant memory and spec excerpts. Choose the least costly model/reasoning that can do the work reliably.

## Unity project

- Current Editor version is recorded in `ProjectSettings/ProjectVersion.txt`; keep project configuration reproducible in Git.
- Do not create or modify gameplay scenes, GameObjects, or assets unless required by the active scoped task.
- Record current status, blockers, and next actions in `docs/ai/current-state.md` and `docs/ai/mvp-status.md` when they change.
