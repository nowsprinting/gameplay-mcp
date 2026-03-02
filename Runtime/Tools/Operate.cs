// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that finds a reachable GameObject and executes the specified operator on it.
    /// </summary>
    public class Operate
    {
        private readonly McpConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Operate"/> class.
        /// </summary>
        /// <param name="config">Configuration for the MCP server.</param>
        public Operate(McpConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Finds a reachable GameObject and executes the specified operator on it.
        /// </summary>
        /// <param name="operatorName">Concrete operator class name (e.g., "UguiClickOperator").</param>
        /// <param name="path">Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).</param>
        /// <param name="name">GameObject name.</param>
        /// <param name="text">Text label on a Button component child. If specified, uses ButtonMatcher.</param>
        /// <param name="texture">Texture/sprite name on a Button component. If specified, uses ButtonMatcher.</param>
        /// <param name="operatorArgs">Operator-specific extra arguments as a JSON string. The JSON keys must match the parameter names of the operator's OperateAsync method overloads. Known built-in operator arguments: ITextInputOperator: {"text": "input text"}, IClickAndHoldOperator: {"holdMillis": 1000}, IToggleOperator: {"isOn": true}, IDragAndDropOperator (by GameObject): {"destination": {"name": "DropTarget"}, "dragSpeed": 50}, IDragAndDropOperator (by screen point): {"destination": [100.0, 200.0], "dragSpeed": 50}, IScrollWheelOperator: {"direction": [0.0, 1.0], "distance": 100, "scrollSpeed": 50}, ISwipeOperator: {"direction": [1.0, 0.0], "swipeSpeed": 100}. Not required for IClickOperator, IDoubleClickOperator, IRightClickOperator, IHoverOperator, IFlickOperator. Parameters with default values in the operator (e.g., dragSpeed, scrollSpeed, swipeSpeed) can be omitted. When a parameter type is GameObject, specify as {"name": "...", "path": "...", "text": "...", "texture": "..."} (same keys as the tool's target parameters); the tool will find the GameObject and verify reachability.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success message, or an error message if the operation fails.</returns>
        [McpServerTool(Name = "operate", ReadOnly = false, Destructive = false)]
        [Description("Finds a reachable GameObject and executes the specified operator on it.")]
        public async Task<string> OperateTool(
            [Description("Concrete operator class name (e.g., \"UguiClickOperator\").")]
            string operatorName,
            [Description("Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).")]
            string path = null,
            [Description("GameObject name.")]
            string name = null,
            [Description("Text label on a Button component child. If specified, uses ButtonMatcher.")]
            string text = null,
            [Description("Texture/sprite name on a Button component. If specified, uses ButtonMatcher.")]
            string texture = null,
            [Description(
                "Operator-specific extra arguments as a JSON string. The JSON keys must match the parameter names " +
                "of the operator's OperateAsync method overloads. " +
                "Known built-in operator arguments: " +
                "ITextInputOperator: {\"text\": \"input text\"}, " +
                "IClickAndHoldOperator: {\"holdMillis\": 1000}, " +
                "IToggleOperator: {\"isOn\": true}, " +
                "IDragAndDropOperator (by GameObject): {\"destination\": {\"name\": \"DropTarget\"}, \"dragSpeed\": 50}, " +
                "IDragAndDropOperator (by screen point): {\"destination\": [100.0, 200.0], \"dragSpeed\": 50}, " +
                "IScrollWheelOperator: {\"direction\": [0.0, 1.0], \"distance\": 100, \"scrollSpeed\": 50}, " +
                "ISwipeOperator: {\"direction\": [1.0, 0.0], \"swipeSpeed\": 100}. " +
                "Not required for IClickOperator, IDoubleClickOperator, IRightClickOperator, IHoverOperator, IFlickOperator. " +
                "Parameters with default values in the operator (e.g., dragSpeed, scrollSpeed, swipeSpeed) can be omitted. " +
                "When a parameter type is GameObject, specify as {\"name\": \"...\", \"path\": \"...\", \"text\": \"...\", \"texture\": \"...\"} " +
                "(same keys as the tool's target parameters); the tool will find the GameObject and verify reachability.")]
            string operatorArgs = null,
            CancellationToken cancellationToken = default)
        {
            await UniTask.SwitchToMainThread(cancellationToken);
            throw new NotImplementedException();
        }
    }
}
