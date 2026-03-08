# Gameplay MCP Server for Unity

[![Meta file check](https://github.com/nowsprinting/gameplay-mcp/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/gameplay-mcp/actions/workflows/metacheck.yml)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/nowsprinting/gameplay-mcp)

Model context protocol (MCP) server for gameplay. Provides tools that AI models play your game via the MCP by embedding in your runtime (player build).

## Key Features

- **Customizable for your game title** — Built on the [UI Test Helper](https://github.com/nowsprinting/test-helper.ui) package, so you can customize operators, reachability strategies, and interactable detection to match your game's UI.
- **Easy to extend** — The MCP server is built on [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk). Adding custom tools is as simple as placing `[McpServerToolType]` and `[McpServerTool]` attributes.
- **Works with IL2CPP builds** — You can run the MCP server on your player build, including IL2CPP.

## Limitations

- WebGL platform is not supported.

## Required Packages

- [MCP C# SDK](https://www.nuget.org/packages/ModelContextProtocol) v1.0.0 or later (NuGet)
- [UI Test Helper](https://github.com/nowsprinting/test-helper.ui) v1.2.2 or later (UPM)
- [Test Helper](https://github.com/nowsprinting/test-helper) v1.4.1 or later (UPM)

## Getting Started

### Start the MCP Server

Start the MCP server from your game title's code. A typical approach is to use `RuntimeInitializeOnLoad` for automatic startup, or toggle it on/off from a debug menu.

```csharp
var config = new McpConfig
{
    OperatorPool = new OperatorPool()
        .Register<UguiClickOperator>()
        .Register<UguiDragAndDropOperator>()
        .Register<UguiTextInputOperator>()
};
var server = new McpServer(config);
server.StartAsync().Forget();
```

`McpConfig` exposes additional settings beyond `OperatorPool`, including `GameObjectFinder`, `IsInteractable`, `ReachableStrategy`, and `ToolsNamespace`. Refer to the [UI Test Helper](https://github.com/nowsprinting/test-helper.ui) documentation for details on the UI-related configuration options.

> [!TIP]  
> You can override the listen prefix via the `-gameplayMcpListenPrefix` command-line argument. Note that if `ListenPrefix` is set in `McpConfig`, it takes precedence over the command-line argument.

To stop the server from a debug menu if you need:

```csharp
server.Dispose();
```

### MCP Settings in Coding Agent

Add the MCP server configuration to your coding agent.
e.g.,
```
{
  "mcpServers": {
    "gameplay": {
      "type": "http",
      "url": "http://localhost:8010/mcp"
    }
  }
}
```

## Built-in Tools

Most built-in tools are wrappers of [UI Test Helper](https://github.com/nowsprinting/test-helper.ui) APIs.

> [!NOTE]
> The tool name is prefixed with the namespace; the default namespace is `mygame`, resulting in `mygame.inspect_game_object`.

### inspect_game_object

Inspect a GameObject by name, path, text label, or texture name and returns properties as JSON. Waits for the GameObject to appear and become reachable within a timeout period.

- **path** — Hierarchy path separated by `/`. Supports glob wildcards (`?`, `*`, `**`).
- **name** — GameObject name.
- **text** — Text label on a Button component child. If specified, uses `ButtonMatcher`.
- **texture** — Texture/sprite name on a Button component. If specified, uses `ButtonMatcher`.
- **reachable** — If `true` (default), only reachable GameObjects are returned.

### list_available_actions

Returns a list of operable actions as a JSON array. Each entry contains a target GameObject and the operator class name that can act on it. For Button components, the text label and texture name are also included in the target.

- **reachable** — If `true` (default), only reachable GameObjects are included.

### operate

Finds a reachable GameObject and executes the specified operator on it.

- **operatorName** — Concrete operator class name (e.g., `"UguiClickOperator"`).
- **path** — Hierarchy path separated by `/`. Supports glob wildcards (`?`, `*`, `**`).
- **name** — GameObject name.
- **text** — Text label on a Button component child. If specified, uses `ButtonMatcher`.
- **texture** — Texture/sprite name on a Button component. If specified, uses `ButtonMatcher`.
- **operatorArgs** — Operator-specific arguments as a JSON string. The JSON keys must match the parameter names of the operator's `OperateAsync` method overloads. Known arguments for built-in operators:
  - `IClickAndHoldOperator`: `{"holdMillis": 1000}`
  - `IDragAndDropOperator` (by GameObject): `{"destination": {"name": "DropTarget"}, "dragSpeed": 50}`
  - `IDragAndDropOperator` (by screen point): `{"destination": [100.0, 200.0], "dragSpeed": 50}`
  - `IScrollWheelOperator`: `{"direction": [0.0, 1.0], "distance": 100, "scrollSpeed": 50}`
  - `ISwipeOperator`: `{"direction": [1.0, 0.0], "swipeSpeed": 100}`
  - `ITextInputOperator`: `{"text": "input text"}`
  - `IToggleOperator`: `{"isOn": true}`
  - Parameters with default values (e.g., `dragSpeed`, `scrollSpeed`, `swipeSpeed`) can be omitted.
  - When a parameter type is `GameObject`, specify it as `{"name": "...", "path": "...", "text": "...", "texture": "..."}` — the tool will find and verify reachability automatically.

### take_screenshot

Captures the current game screen and returns it as an image.

- **maxLongSide** — Maximum length of the long side in pixels. The image is scaled down if it exceeds this value. Defaults to `1568`.
- **format** — Image format: `"jpeg"` (default) or `"png"`.
- **quality** — JPEG encoding quality (1–100). Only used when `format` is `"jpeg"`. Defaults to `75`.

### get_scenes

Returns the currently loaded scenes as JSON. The active scene is marked with `active=true`.

No parameters.

## Adding Custom Tools

Any class annotated with `[McpServerToolType]` is discovered automatically at server startup — no registration required.

```csharp
// Assets/Scripts/Runtime/MyGameTools.cs (custom tools created by the game title)
// Just add [McpServerToolType] and it's registered automatically!
[McpServerToolType]
public class MyGameTools
{
    [McpServerTool(Name = "get_player_status", ReadOnly = true, Destructive = false)]
    [Description("Returns the player's current status as JSON.")]
    public async Task<string> GetPlayerStatus(CancellationToken ct = default)
    {
        await UniTask.SwitchToMainThread(ct);
        var player = GameObject.FindWithTag("Player");
        return JsonSerializer.Serialize(new { hp = player.GetComponent<Health>().Current });
    }
}
```

At minimum, it is recommended to implement a tool that returns the current game state, so the AI model can understand the game situation without relying solely on screenshots.

If a custom tool covers the same use case as a built-in tool, you can hide the built-in tool via `DisabledTools`:

```csharp
var config = new McpConfig();
config.DisabledTools.Add("mygame.inspect_game_object"); // use the full prefixed name
```

Hidden tools are excluded from `tools/list` responses. MCP clients typically only call tools they discover via `tools/list`, so this effectively disables them.

## Agent Skills

When providing instructions to an AI model, consider defining the following as skills:

- **Screen transition and navigation** — Even if the instructions describe high-level goals rather than step-by-step procedures, the AI needs to know the screen structure and which buttons navigate to which screens.
- **Game rules and knowledge** — If you want the AI to play autonomously, it will need knowledge of game rules, items, and mechanics.
- **Custom operator parameters** — If your game title uses custom operators with non-obvious parameters, document how to specify those parameters as part of the skill.
- **Troubleshooting** — Convert the troubleshooting information from the [UI Test Helper](https://github.com/nowsprinting/test-helper.ui) into a skill, so the AI can resolve issues such as failing to find an operation target.
- **Screenshot format recommendations** — The default format is JPEG with quality 75, which works for most cases. Consider adjusting in these situations:
  - **Increase JPEG quality** — If the game has fine visual details that are lost at quality 75.
  - **Use PNG** — PNG is preferred in the following cases:
    - When the AI needs to accurately read UI text.
    - For pixel art games where compression artifacts are visible.
    - When pixel-level accuracy is required for debugging purposes (e.g., visual regression testing).
