// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that returns a list of operable GameObjects and their operators as JSON.
    /// </summary>
    public class GetAvailableTargetOperators
    {
        private readonly McpConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAvailableTargetOperators"/> class.
        /// </summary>
        /// <param name="config">Configuration for the MCP server.</param>
        public GetAvailableTargetOperators(McpConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Returns a list of operable GameObjects and their available operators as JSON.
        /// </summary>
        /// <param name="reachable">If true (default), only reachable GameObjects are included.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON string with operable targets and their operators, or a message if none are found.</returns>
        [McpServerTool(Name = "get_available_target_operators", ReadOnly = true, Destructive = false)]
        [Description("Returns a list of operable GameObjects and their available operators as JSON.")]
        public async Task<string> GetAvailableTargetOperatorsTool(
            [Description("If true (default), only reachable GameObjects are included.")]
            bool reachable = true,
            CancellationToken cancellationToken = default)
        {
            return "";
        }
    }
}
