// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that returns a list of operable actions (target and operator pairs) as JSON.
    /// </summary>
    [McpServerToolType]
    public static class ListAvailableActionsTool
    {
        /// <summary>
        /// Returns a list of operable actions as a JSON array. Each entry contains a target GameObject and the operator that can act on it.
        /// </summary>
        /// <param name="reachable">If true (default), only reachable GameObjects are included.</param>
        /// <param name="config">Configuration injected via DI.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON array of action entries, or a message if none are found.</returns>
        [McpServerTool(Name = "list_available_actions", ReadOnly = true, Destructive = false)]
        [Description("Returns a list of operable actions as a JSON array. Each entry contains a target GameObject and the operator that can act on it.")]
        public static async Task<string> ListAvailableActions(
            [Description("If true (default), only reachable GameObjects are included.")]
            bool reachable = true,
            McpConfig config = null, // Injected via IServiceProvider; default null is never used at runtime
            CancellationToken cancellationToken = default)
        {
            await UniTask.SwitchToMainThread(cancellationToken);

            try
            {
                var pairs = config.InteractableComponentsFinder.FindInteractableComponentsAndOperators().ToList();

                IEnumerable<(MonoBehaviour, IOperator)> filteredPairs = pairs;
                if (reachable)
                {
                    filteredPairs = pairs.Where(pair =>
                        config.ReachableStrategy.IsReachable(pair.Item1.gameObject, out _));
                }

                var entries = filteredPairs
                    .Select(pair => BuildEntry(pair.Item1.gameObject, pair.Item2))
                    .ToList();

                if (entries.Count == 0)
                {
                    return "No operable GameObjects found on the current scene.";
                }

                return JsonSerializer.Serialize(entries);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static Dictionary<string, object> BuildEntry(GameObject go, IOperator op)
        {
            var target = new Dictionary<string, object>
            {
                ["name"] = go.name,
                ["path"] = go.transform.GetPath(),
            };

            var button = go.GetComponent<Button>();
            if (button != null)
            {
                var text = go.GetComponentInChildren<Text>()?.text;
                if (text != null)
                {
                    target["text"] = text;
                }

                var texture = go.GetComponent<Image>()?.sprite?.name;
                if (texture != null)
                {
                    target["texture"] = texture;
                }
            }

            return new Dictionary<string, object>
            {
                ["target"] = target,
                ["operator"] = op.GetType().Name,
            };
        }
    }
}
