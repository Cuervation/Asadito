# MCP for Unity setup

- Unity package: `com.coplaydev.unity-mcp` 10.2.0 (stable)
- Upstream: https://github.com/CoplayDev/unity-mcp, tag `v10.2.0`
- Installed as a UPM embedded package in `Packages/com.coplaydev.unity-mcp` so the upstream source is stored with the Unity project and does not require the macOS Git Command Line Tools.
- Codex connects over local HTTP at `http://127.0.0.1:8080/mcp`.
- `uv` and Python 3.12 are installed under the current macOS user. The server wrapper pins `cryptography` to 45.0.6 because the current latest release tries a native build when Command Line Tools are absent.
# Unity MCP compatibility note

On Unity 6000.6.3f1 for macOS, MCP for Unity 10.2.0's reflected
`EditorUtility.InstanceIDToObject(int)` throws `NotImplementedException`. The embedded
package includes a fallback scan of loaded Unity objects in
`Runtime/Helpers/UnityObjectIdCompat.cs`, matching the session-scoped EntityId handle.
This keeps MCP hierarchy and GameObject resource reads working in this Editor build.
