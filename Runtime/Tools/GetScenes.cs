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
    [McpServerToolType]
    public static class GetScenes
    {
        /// <summary>
        /// Returns the currently loaded scenes as JSON. The active scene is marked with active=true.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON array of scene objects with name and active fields, or error message on failure.</returns>
        [McpServerTool(Name = "get_scenes", ReadOnly = true, Destructive = false)]
        [Description("Returns the currently loaded scenes as JSON. The active scene is marked with active=true.")]
        public static async Task<string> GetScenesTool(CancellationToken cancellationToken = default)
        {
            await UniTask.SwitchToMainThread(cancellationToken);
            try
            {
                var activeScene = SceneManager.GetActiveScene();
                var scenes = new List<object>();
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    scenes.Add(new { name = scene.name, active = scene == activeScene });
                }
                return JsonSerializer.Serialize(scenes);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
