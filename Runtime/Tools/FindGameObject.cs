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
    /// MCP tool that finds a GameObject by name, path, text, or texture and returns its component information as JSON.
    /// </summary>
    [McpServerToolType]
    public class FindGameObject
    {
        /// <summary>
        /// Finds a GameObject by name, path, text label, or texture name and returns its component information as JSON.
        /// </summary>
        /// <param name="path">Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).</param>
        /// <param name="name">GameObject name.</param>
        /// <param name="text">Text label on a Button component child. If specified, uses ButtonMatcher.</param>
        /// <param name="texture">Texture/sprite name on a Button component. If specified, uses ButtonMatcher.</param>
        /// <param name="reachable">If true, only reachable GameObjects are returned.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON string with the found GameObject's name, path, and component details, or an exception message if not found.</returns>
        [McpServerTool(Name = "find_gameobject", ReadOnly = true, Destructive = false)]
        [Description("Finds a GameObject by name, path, text label, or texture and returns its component properties as JSON.")]
        public static async Task<string> FindGameObjectTool(
            [Description("Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).")]
            string path = null,
            [Description("GameObject name.")]
            string name = null,
            [Description("Text label on a Button component child. If specified, uses ButtonMatcher.")]
            string text = null,
            [Description("Texture/sprite name on a Button component. If specified, uses ButtonMatcher.")]
            string texture = null,
            [Description("If true (default), only reachable GameObjects are returned.")]
            bool reachable = true,
            CancellationToken cancellationToken = default)
        {
            await UniTask.SwitchToMainThread(cancellationToken);

            try
            {
                var config = McpServer.Instance.Config;

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
