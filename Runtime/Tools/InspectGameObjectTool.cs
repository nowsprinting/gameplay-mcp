// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameplayMcp.Internals;
using ModelContextProtocol.Server;
using TestHelper.UI.GameObjectMatchers;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that inspects a GameObject by name, path, text, or texture and returns its component information as JSON.
    /// Waits for the GameObject to appear and become reachable within a timeout period.
    /// </summary>
    [McpServerToolType]
    public static class InspectGameObjectTool
    {
        /// <summary>
        /// Inspects a GameObject by name, path, text label, or texture name and returns its component information as JSON.
        /// Waits for the GameObject to appear and become reachable within a timeout period.
        /// </summary>
        /// <param name="path">Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).</param>
        /// <param name="name">GameObject name.</param>
        /// <param name="text">Text label on a Button component child. If specified, uses ButtonMatcher.</param>
        /// <param name="texture">Texture/sprite name on a Button component. If specified, uses ButtonMatcher.</param>
        /// <param name="reachable">If true, only reachable GameObjects are returned.</param>
        /// <param name="config">Configuration injected via DI.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON string with the found GameObject's name, path, and component details, or an exception message if not found.</returns>
        [McpServerTool(Name = "inspect_game_object", ReadOnly = true, Destructive = false)]
        [Description("Inspect a GameObject by name, path, text label, or texture name and returns properties as JSON. Waits for the GameObject to appear and become reachable within a timeout period.")]
        public static async Task<string> InspectGameObject(
            [Description("Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).")]
            string path = null,
            [Description("GameObject name.")] string name = null,
            [Description("Text label on a Button component child. If specified, uses ButtonMatcher.")]
            string text = null,
            [Description("Texture/sprite name on a Button component. If specified, uses ButtonMatcher.")]
            string texture = null,
            [Description("If true (default), only reachable GameObjects are returned.")]
            bool reachable = true,
            McpConfig config = null, // Injected via IServiceProvider; default null is never used at runtime
            CancellationToken cancellationToken = default)
        {
            await UniTask.SwitchToMainThread(cancellationToken);

            try
            {
                // Use ButtonMatcher when text or texture is given; ComponentMatcher otherwise.
                // ButtonMatcher requires a Button component, while ComponentMatcher matches any Component (typeof(Component)).
                var matcher = (text != null || texture != null)
                    ? (IGameObjectMatcher)new ButtonMatcher(name: name, path: path, text: text, texture: texture)
                    : new ComponentMatcher(name: name, path: path);

                var result = await config.GameObjectFinder.FindByMatcherAsync(
                    matcher,
                    reachable: reachable,
                    cancellationToken: cancellationToken);

                return GameObjectSerializer.Serialize(result.GameObject);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
