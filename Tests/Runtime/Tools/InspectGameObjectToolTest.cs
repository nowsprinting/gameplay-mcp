// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayMcp.Tools
{
    [TestFixture]
    public class InspectGameObjectToolTest
    {
        [Test]
        [CreateScene]
        public async Task InspectGameObject_ByName_ReturnsJsonContainingName()
        {
            var go = new GameObject("TargetObject");
            go.AddComponent<Canvas>(); // needs Canvas to be reachable via raycasting, but for name search we skip reachable

            var actual = await InspectGameObjectTool.InspectGameObject(name: "TargetObject", reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("TargetObject"));
        }

        [Test]
        [CreateScene]
        public async Task InspectGameObject_ByPath_ReturnsJsonContainingPath()
        {
            var parent = new GameObject("ParentObj");
            var child = new GameObject("ChildObj");
            child.transform.SetParent(parent.transform);

            var actual = await InspectGameObjectTool.InspectGameObject(path: "/ParentObj/ChildObj", reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("/ParentObj/ChildObj"));
        }

        [Test]
        [CreateScene]
        public async Task InspectGameObject_WithText_ReturnsJsonContainingButtonComponent()
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

            var actual = await InspectGameObjectTool.InspectGameObject(text: "Start", reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("Button"));
        }

        [Test]
        [CreateScene]
        public async Task InspectGameObject_NotFound_ReturnsExceptionMessage()
        {
            var actual = await InspectGameObjectTool.InspectGameObject(name: "NonExistentGameObject_12345", reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("Exception").Or.Contain("not found").Or.Contain("TimeoutException"));
        }
    }
}
