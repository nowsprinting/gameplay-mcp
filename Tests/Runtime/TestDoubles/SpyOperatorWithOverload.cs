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
    /// Spy operator that has both the base OperateAsync overload and an additional overload with a string parameter.
    /// Implements ITextInputOperator to simulate operators with extra arguments.
    /// </summary>
    internal class SpyOperatorWithOverload : ITextInputOperator
    {
        /// <summary>
        /// Whether the base OperateAsync was called.
        /// </summary>
        internal bool WasBaseOperateCalled { get; private set; }

        /// <summary>
        /// Whether the overloaded OperateAsync(text) was called.
        /// </summary>
        internal bool WasOverloadOperateCalled { get; private set; }

        /// <summary>
        /// The text argument received by the overloaded OperateAsync.
        /// </summary>
        internal string LastReceivedText { get; private set; }

        /// <inheritdoc/>
        public ILogger Logger { set { } }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { set { } }

        /// <inheritdoc/>
        public IVisualizer Visualizer { set { } }

        /// <inheritdoc/>
        public bool CanOperate(GameObject gameObject) => true;

        /// <inheritdoc/>
        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            WasBaseOperateCalled = true;
            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public UniTask OperateAsync(GameObject gameObject, string text,
            CancellationToken cancellationToken = default)
        {
            WasOverloadOperateCalled = true;
            LastReceivedText = text;
            return UniTask.CompletedTask;
        }
    }
}
