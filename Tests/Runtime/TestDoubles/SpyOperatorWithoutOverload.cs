// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// Spy operator that has only the base OperateAsync overload.
    /// Implements IClickOperator to simulate simple click operators.
    /// </summary>
    internal class SpyOperatorWithoutOverload : IClickOperator
    {
        /// <summary>
        /// Whether CanOperate returns true or false.
        /// </summary>
        internal bool CanOperateResult { get; set; } = true;

        /// <summary>
        /// Whether OperateAsync was called.
        /// </summary>
        internal bool WasOperateCalled { get; private set; }

        /// <summary>
        /// The GameObject that was passed to OperateAsync.
        /// </summary>
        internal GameObject LastOperatedGameObject { get; private set; }

        /// <inheritdoc/>
        public ILogger Logger { set { } }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { set { } }

        /// <inheritdoc/>
        public IVisualizer Visualizer { set { } }

        /// <inheritdoc/>
        public bool CanOperate(GameObject gameObject) => CanOperateResult;

        /// <inheritdoc/>
        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            WasOperateCalled = true;
            LastOperatedGameObject = gameObject;
            return UniTask.CompletedTask;
        }
    }
}
