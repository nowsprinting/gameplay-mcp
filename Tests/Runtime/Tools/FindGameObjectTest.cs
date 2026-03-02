// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayMcp.Tools
{
    [TestFixture]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    public class FindGameObjectTest
    {
        private McpServer _server;

        [TearDown]
        public void TearDown()
        {
            _server?.Stop();
            _server?.Dispose();
            _server = null;
        }

        [Test]
        [CreateScene]
        public async Task FindGameObjectTool_ByName_ReturnsJsonContainingName()
        {
            var go = new GameObject("TargetObject");
            go.AddComponent<Canvas>(); // needs Canvas to be reachable via raycasting, but for name search we skip reachable
            _server = new McpServer(new McpConfig());

            var actual = await FindGameObject.FindGameObjectTool(name: "TargetObject", reachable: false);

            Assert.That(actual, Does.Contain("TargetObject"));
        }

        [Test]
        [CreateScene]
        public async Task FindGameObjectTool_ByPath_ReturnsJsonContainingPath()
        {
            var parent = new GameObject("ParentObj");
            var child = new GameObject("ChildObj");
            child.transform.SetParent(parent.transform);
            _server = new McpServer(new McpConfig());

            var actual = await FindGameObject.FindGameObjectTool(path: "/ParentObj/ChildObj", reachable: false);

            Assert.That(actual, Does.Contain("/ParentObj/ChildObj"));
        }

        [Test]
        [CreateScene]
        public async Task FindGameObjectTool_WithText_ReturnsJsonContainingButtonComponent()
        {
            // Create a Button with a Text child
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            var buttonGo = new GameObject("StartButton");
            buttonGo.transform.SetParent(canvasGo.transform);
            buttonGo.AddComponent<Button>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform);
            var textComp = textGo.AddComponent<Text>();
            textComp.text = "Start";
            _server = new McpServer(new McpConfig());

            var actual = await FindGameObject.FindGameObjectTool(text: "Start", reachable: false);

            Assert.That(actual, Does.Contain("Button"));
        }

        [Test]
        [CreateScene]
        public async Task FindGameObjectTool_NotFound_ReturnsExceptionMessage()
        {
            _server = new McpServer(new McpConfig());

            var actual = await FindGameObject.FindGameObjectTool(name: "NonExistentGameObject_12345", reachable: false);

            Assert.That(actual, Does.Contain("Exception").Or.Contain("not found").Or.Contain("TimeoutException"));
        }
    }
}
