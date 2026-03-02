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
            await UniTask.SwitchToMainThread(cancellationToken);

            try
            {
                var pairs = _config.InteractableComponentsFinder.FindInteractableComponentsAndOperators().ToList();

                IEnumerable<(UnityEngine.MonoBehaviour, IOperator)> filteredPairs = pairs;
                if (reachable)
                {
                    filteredPairs = pairs.Where(pair =>
                        _config.ReachableStrategy.IsReachable(pair.Item1.gameObject, out _));
                }

                var entries = filteredPairs
                    .GroupBy(pair => pair.Item1.gameObject)
                    .Select(group => BuildEntry(group.Key, group.Select(p => p.Item2)))
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

        private static Dictionary<string, object> BuildEntry(GameObject go, IEnumerable<IOperator> operators)
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
                ["operators"] = operators.Select(op => op.GetType().Name).Distinct().ToArray(),
            };
        }
    }
}
