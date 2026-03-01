// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that finds a GameObject by name, path, text, or texture and returns its component information as JSON.
    /// </summary>
    public class FindGameObject
    {
        private readonly McpConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindGameObject"/> class.
        /// </summary>
        /// <param name="config">Configuration for the MCP server.</param>
        public FindGameObject(McpConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Finds a GameObject by name, path, text label, or texture name and returns its component information as JSON.
        /// </summary>
        /// <param name="path">Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).</param>
        /// <param name="name">GameObject name.</param>
        /// <param name="text">Text label on a Button component child.</param>
        /// <param name="texture">Texture/sprite name on a Button component.</param>
        /// <param name="reachable">If true, only reachable GameObjects are returned.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON string with the found GameObject's name, path, and component details, or an exception message if not found.</returns>
        [McpServerTool(Name = "find_gameobject", ReadOnly = true, Destructive = false)]
        [Description("Finds a GameObject by name, path, text label, or texture and returns its component properties as JSON.")]
        public async Task<string> FindGameObjectTool(
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
            throw new System.NotImplementedException();
        }
    }
}
