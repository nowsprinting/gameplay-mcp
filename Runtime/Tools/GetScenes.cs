// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using UnityEngine.SceneManagement;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that returns the currently loaded scenes as JSON.
    /// </summary>
    public class GetScenes
    {
        /// <summary>
        /// Returns the currently loaded scenes as JSON. The active scene is marked with active=true.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON array of scene objects with name and active fields, or error message on failure.</returns>
        [McpServerTool(Name = "get_scenes", ReadOnly = true, Destructive = false)]
        [Description("Returns the currently loaded scenes as JSON. The active scene is marked with active=true.")]
        public async Task<string> GetScenesTool(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
