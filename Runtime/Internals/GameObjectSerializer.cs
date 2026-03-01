// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using TestHelper.UI.Extensions;
using UnityEngine;

namespace GameplayMcp.Internals
{
    /// <summary>
    /// Serializes a <see cref="GameObject"/> to a JSON string.
    /// </summary>
    internal static class GameObjectSerializer
    {
        /// <summary>
        /// Serializes the given <paramref name="gameObject"/> to a JSON string.
        /// Includes name, hierarchy path, and all public properties and fields of each Component.
        /// </summary>
        /// <param name="gameObject">The target GameObject.</param>
        /// <returns>JSON string representing the GameObject.</returns>
        internal static string Serialize(GameObject gameObject)
        {
            var result = new Dictionary<string, object>
            {
                ["name"] = gameObject.name,
                ["path"] = gameObject.transform.GetPath(),
                ["components"] = BuildComponentList(gameObject),
            };

            return JsonSerializer.Serialize(result);
        }

        private static List<Dictionary<string, string>> BuildComponentList(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            var componentList = new List<Dictionary<string, string>>(components.Length);

            foreach (var component in components)
            {
                if (component == null)
                {
                    continue;
                }

                var componentData = new Dictionary<string, string>
                {
                    ["type"] = component.GetType().FullName,
                };

                var componentType = component.GetType();

                foreach (var property in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    // Skip indexers: they require parameters and cannot be retrieved without arguments
                    if (property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    try
                    {
                        var value = property.GetValue(component);
                        componentData[property.Name] = value?.ToString() ?? "null";
                    }
                    catch
                    {
                        // Skip properties that throw on access (e.g., obsolete or restricted Unity internals)
                    }
                }

                foreach (var field in componentType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        var value = field.GetValue(component);
                        componentData[field.Name] = value?.ToString() ?? "null";
                    }
                    catch
                    {
                        // Skip fields that throw on access
                    }
                }

                componentList.Add(componentData);
            }

            return componentList;
        }
    }
}
