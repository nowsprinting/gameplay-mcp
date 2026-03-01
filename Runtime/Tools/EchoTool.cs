// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.ComponentModel;
using ModelContextProtocol.Server;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that echoes input messages back unchanged.
    /// </summary>
    public static class EchoTool
    {
        /// <summary>
        /// Echoes the input message back as-is.
        /// </summary>
        /// <param name="message">The message to echo.</param>
        /// <returns>The same message that was received.</returns>
        [McpServerTool(Name = "echo", ReadOnly = true, Destructive = false)]
        [Description("Echoes the input message back as-is.")]
        public static string Echo(string message)
        {
            return message;
        }
    }
}
