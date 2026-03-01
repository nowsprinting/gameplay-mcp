// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.UI;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using UnityEngine;

namespace GameplayMcp
{
    /// <summary>
    /// Configuration for <see cref="McpServer"/>.
    /// </summary>
    public class McpConfig
    {
        private string _listenPrefix;

        /// <summary>
        /// HTTP listener prefix.
        /// <p/>
        /// Defaults to the value of the <c>-gameplayMcpListenPrefix</c> command-line argument,
        /// or <c>"http://+:8010/"</c> if the argument is not specified.
        /// <p/>
        /// Use wildcard prefix so HttpListener binds to both IPv4 and IPv6 interfaces.
        /// "localhost" binds IPv6-only on IL2CPP standalone builds, causing connection failures
        /// when MCP clients connect via IPv4 (127.0.0.1).
        /// </summary>
        public string ListenPrefix
        {
            get => _listenPrefix ??= Internals.CommandLineArgs.GetListenPrefix();
            set => _listenPrefix = value;
        }

        /// <summary>
        /// <see cref="TestHelper.UI.GameObjectFinder"/> used to locate GameObjects.
        /// </summary>
        public GameObjectFinder GameObjectFinder { get; set; } = new GameObjectFinder();

        /// <summary>
        /// Function returns the <c>Component</c> is interactable or not.
        /// Used by <see cref="InteractableComponentsFinder"/>.
        /// </summary>
        public Func<Component, bool> IsInteractable { get; set; } = DefaultComponentInteractableStrategy.IsInteractable;

        private InteractableComponentsFinder _interactableComponentsFinder;

        /// <summary>
        /// <see cref="TestHelper.UI.InteractableComponentsFinder"/> used to collect interactable components.
        /// </summary>
        public InteractableComponentsFinder InteractableComponentsFinder
        {
            get
            {
                _interactableComponentsFinder ??= new InteractableComponentsFinder(IsInteractable, OperatorPool);
                return _interactableComponentsFinder;
            }
        }

        /// <summary>
        /// <see cref="TestHelper.UI.OperatorPool"/> used to manage UI operators.
        /// <p/>
        /// The only operator registered by default is <see cref="TestHelper.UI.Operators.UguiClickOperator"/>.
        /// </summary>
        public OperatorPool OperatorPool { get; set; } = new OperatorPool().Register<UguiClickOperator>();

        /// <summary>
        /// Whether to register the find_gameobject tool.
        /// </summary>
        public bool EnableFindGameObjectTool { get; set; } = true;
    }
}
