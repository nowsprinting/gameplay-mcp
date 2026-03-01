// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace GameplayMcp
{
    /// <summary>
    /// Entry point for the Gameplay MCP server. Starts the server when Play mode begins.
    /// </summary>
    public static class McpServerBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            try
            {
                using var server = new McpServer();
                var ct = Application.exitCancellationToken;

                // Fire-and-forget: server runs for the lifetime of Play mode
                _ = server.StartAsync(ct);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
