// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Operators;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that finds a reachable GameObject and executes the specified operator on it.
    /// </summary>
    [McpServerToolType]
    public static class InvokeActionTool
    {
        // Excluded by parameter name, not type, so that additional GameObject parameters
        // (e.g., IDragAndDropOperator.destination) are recognized as extra parameters.
        private static readonly HashSet<string> FixedParamNames = new HashSet<string>
        {
            "gameObject", "raycastResult", "cancellationToken"
        };

        /// <summary>
        /// Finds a reachable GameObject and executes the specified operator on it.
        /// </summary>
        /// <param name="operatorName">Concrete operator class name (e.g., "UguiClickOperator").</param>
        /// <param name="path">Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).</param>
        /// <param name="name">GameObject name.</param>
        /// <param name="text">Text label on a Button component child. If specified, uses ButtonMatcher.</param>
        /// <param name="texture">Texture/sprite name on a Button component. If specified, uses ButtonMatcher.</param>
        /// <param name="operatorArgs">Operator-specific extra arguments as a JSON string. The JSON keys must match the parameter names of the operator's OperateAsync method overloads. Known built-in operator arguments: ITextInputOperator: {"text": "input text"}, IClickAndHoldOperator: {"holdMillis": 1000}, IToggleOperator: {"isOn": true}, IDragAndDropOperator (by GameObject): {"destination": {"name": "DropTarget"}, "dragSpeed": 50}, IDragAndDropOperator (by screen point): {"destination": [100.0, 200.0], "dragSpeed": 50}, IScrollWheelOperator: {"direction": [0.0, 1.0], "distance": 100, "scrollSpeed": 50}, ISwipeOperator: {"direction": [1.0, 0.0], "swipeSpeed": 100}. Not required for IClickOperator, IDoubleClickOperator, IRightClickOperator, IHoverOperator, IFlickOperator. Parameters with default values in the operator (e.g., dragSpeed, scrollSpeed, swipeSpeed) can be omitted. When a parameter type is GameObject, specify as {"name": "...", "path": "...", "text": "...", "texture": "..."} (same keys as the tool's target parameters); the tool will find the GameObject and verify reachability.</param>
        /// <param name="config">Configuration injected via DI.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success message, or an error message if the operation fails.</returns>
        [McpServerTool(Name = "invoke_action", ReadOnly = false, Destructive = false)]
        [Description("Finds a reachable GameObject and executes the specified operator on it.")]
        public static async Task<string> InvokeAction(
            [Description("Concrete operator class name (e.g., \"UguiClickOperator\").")]
            string operatorName,
            [Description("Hierarchy path separated by '/'. Supports glob wildcards (?, *, **).")]
            string path = null,
            [Description("GameObject name.")] string name = null,
            [Description("Text label on a Button component child. If specified, uses ButtonMatcher.")]
            string text = null,
            [Description("Texture/sprite name on a Button component. If specified, uses ButtonMatcher.")]
            string texture = null,
            [Description(
                "Operator-specific extra arguments as a JSON string. The JSON keys must match the parameter names " +
                "of the operator's OperateAsync method overloads. " +
                "Known built-in operator arguments: " +
                "ITextInputOperator: {\"text\": \"input text\"}, " +
                "IClickAndHoldOperator: {\"holdMillis\": 1000}, " +
                "IToggleOperator: {\"isOn\": true}, " +
                "IDragAndDropOperator (by GameObject): {\"destination\": {\"name\": \"DropTarget\"}, \"dragSpeed\": 50}, " +
                "IDragAndDropOperator (by screen point): {\"destination\": [100.0, 200.0], \"dragSpeed\": 50}, " +
                "IScrollWheelOperator: {\"direction\": [0.0, 1.0], \"distance\": 100, \"scrollSpeed\": 50}, " +
                "ISwipeOperator: {\"direction\": [1.0, 0.0], \"swipeSpeed\": 100}. " +
                "Not required for IClickOperator, IDoubleClickOperator, IRightClickOperator, IHoverOperator, IFlickOperator. " +
                "Parameters with default values in the operator (e.g., dragSpeed, scrollSpeed, swipeSpeed) can be omitted. " +
                "When a parameter type is GameObject, specify as {\"name\": \"...\", \"path\": \"...\", \"text\": \"...\", \"texture\": \"...\"} " +
                "(same keys as the tool's target parameters); the tool will find the GameObject and verify reachability.")]
            string operatorArgs = null,
            McpConfig config = null, // Injected via IServiceProvider; default null is never used at runtime
            CancellationToken cancellationToken = default)
        {
            await UniTask.SwitchToMainThread(cancellationToken);
            IOperator targetOperator = null;
            try
            {
                // Step 1: Resolve operator type by scanning all loaded assemblies
                var operatorType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch (ReflectionTypeLoadException) { return Array.Empty<Type>(); }
                    })
                    .FirstOrDefault(t =>
                        t.Name == operatorName &&
                        !t.IsInterface &&
                        !t.IsAbstract &&
                        typeof(IOperator).IsAssignableFrom(t));

                if (operatorType == null)
                {
                    return $"Operator type '{operatorName}' not found.";
                }

                // Step 2: Rent the operator from the pool
                targetOperator = config.OperatorPool.Rent(operatorType);

                // Step 3: Find the target GameObject (reachable forced to true)
                // Use ButtonMatcher when text or texture is given; ComponentMatcher otherwise.
                // ButtonMatcher requires a Button component, while ComponentMatcher matches any Component.
                var matcher = (text != null || texture != null)
                    ? (IGameObjectMatcher)new ButtonMatcher(name: name, path: path, text: text, texture: texture)
                    : new ComponentMatcher(name: name, path: path);

                var findResult = await config.GameObjectFinder.FindByMatcherAsync(
                    matcher,
                    reachable: true,
                    cancellationToken: cancellationToken);

                // Step 4: Check if the operator can operate on the found GameObject
                if (!targetOperator.CanOperate(findResult.GameObject))
                {
                    return $"Operator '{operatorName}' cannot operate on '{findResult.GameObject.name}'.";
                }

                // Step 5: Select and invoke the appropriate OperateAsync overload via reflection
                var overloads = operatorType.GetMethods()
                    .Where(m => m.Name == "OperateAsync")
                    .ToArray();

                Dictionary<string, JsonElement> jsonArgs = null;
                if (!string.IsNullOrEmpty(operatorArgs))
                {
                    jsonArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(operatorArgs);
                }

                var selectedOverload = SelectOverload(overloads, jsonArgs);
                if (selectedOverload == null)
                {
                    var paramInfo = BuildOverloadParamInfo(overloads);
                    return
                        $"No matching OperateAsync overload for '{operatorName}' with arguments '{operatorArgs}'. Available overload parameters:{paramInfo}";
                }

                var invokeArgs = await BuildArgsAsync(selectedOverload, findResult.GameObject, findResult.RaycastResult,
                    jsonArgs, config, cancellationToken);
                await (UniTask)selectedOverload.Invoke(targetOperator, invokeArgs);

                return $"Invoked '{operatorName}' on '{findResult.GameObject.name}'.";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            finally
            {
                if (targetOperator != null)
                {
                    config.OperatorPool.Return(targetOperator);
                }
            }
        }

        private static MethodInfo SelectOverload(MethodInfo[] overloads, Dictionary<string, JsonElement> jsonArgs)
        {
            if (jsonArgs == null || jsonArgs.Count == 0)
            {
                // Prefer the overload with the fewest extra parameters where all are optional (default: base overload)
                return overloads
                    .OrderBy(m => ExtraParams(m).Count())
                    .FirstOrDefault(m => ExtraParams(m).All(p => p.HasDefaultValue));
            }

            var jsonKeys = new HashSet<string>(jsonArgs.Keys);

            return overloads.FirstOrDefault(m =>
            {
                var extraParams = ExtraParams(m).ToArray();
                var extraParamNames = new HashSet<string>(extraParams.Select(p => p.Name));
                var requiredParamNames =
                    new HashSet<string>(extraParams.Where(p => !p.HasDefaultValue).Select(p => p.Name));

                // All JSON keys must exist in the overload's extra parameter names,
                // and all required extra parameters must be present in the JSON.
                return jsonKeys.IsSubsetOf(extraParamNames) && requiredParamNames.IsSubsetOf(jsonKeys);
            });
        }

        private static IEnumerable<ParameterInfo> ExtraParams(MethodInfo method)
        {
            return method.GetParameters().Where(p => !FixedParamNames.Contains(p.Name));
        }

        private static async UniTask<object[]> BuildArgsAsync(MethodInfo method, GameObject gameObject,
            RaycastResult raycastResult, Dictionary<string, JsonElement> jsonArgs, McpConfig config,
            CancellationToken ct)
        {
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];

                if (param.Name == "gameObject")
                {
                    args[i] = gameObject;
                }
                else if (param.Name == "raycastResult")
                {
                    args[i] = raycastResult;
                }
                else if (param.Name == "cancellationToken")
                {
                    args[i] = ct;
                }
                else if (jsonArgs != null && jsonArgs.TryGetValue(param.Name, out var jsonValue))
                {
                    args[i] = await ConvertJsonValueAsync(jsonValue, param.ParameterType, config, ct);
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
            }

            return args;
        }

        private static async UniTask<object> ConvertJsonValueAsync(JsonElement jsonValue, Type targetType,
            McpConfig config, CancellationToken ct)
        {
            if (targetType == typeof(int)) return jsonValue.GetInt32();
            if (targetType == typeof(float)) return (float)jsonValue.GetDouble();
            if (targetType == typeof(double)) return jsonValue.GetDouble();
            if (targetType == typeof(bool)) return jsonValue.GetBoolean();
            if (targetType == typeof(string)) return jsonValue.GetString();

            if (targetType == typeof(Vector2))
            {
                var arr = jsonValue.EnumerateArray().ToArray();
                return new Vector2((float)arr[0].GetDouble(), (float)arr[1].GetDouble());
            }

            if (targetType == typeof(GameObject))
            {
                // Parse {"name": "...", "path": "...", "text": "...", "texture": "..."} same as tool's own target params
                string goName = null, goPath = null, goText = null, goTexture = null;
                foreach (var prop in jsonValue.EnumerateObject())
                {
                    switch (prop.Name)
                    {
                        case "name": goName = prop.Value.GetString(); break;
                        case "path": goPath = prop.Value.GetString(); break;
                        case "text": goText = prop.Value.GetString(); break;
                        case "texture": goTexture = prop.Value.GetString(); break;
                    }
                }

                var matcher = (goText != null || goTexture != null)
                    ? (IGameObjectMatcher)new ButtonMatcher(name: goName, path: goPath, text: goText,
                        texture: goTexture)
                    : new ComponentMatcher(name: goName, path: goPath);
                var result =
                    await config.GameObjectFinder.FindByMatcherAsync(matcher, reachable: true, cancellationToken: ct);
                return result.GameObject;
            }

            throw new InvalidOperationException($"Cannot convert JSON value to type '{targetType.Name}'.");
        }

        private static string BuildOverloadParamInfo(MethodInfo[] overloads)
        {
            var sb = new StringBuilder();
            foreach (var method in overloads)
            {
                var extraParams = ExtraParams(method).ToArray();
                if (!extraParams.Any()) continue;
                sb.AppendLine();
                sb.Append("  OperateAsync(");
                sb.Append(string.Join(", ", extraParams.Select(p =>
                    p.HasDefaultValue
                        ? $"{p.ParameterType.Name} {p.Name} = {p.DefaultValue ?? "null"}"
                        : $"{p.ParameterType.Name} {p.Name}")));
                sb.Append(")");
            }

            return sb.Length > 0 ? sb.ToString() : " (base overload only, no extra parameters)";
        }
    }
}
