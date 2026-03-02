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
    public class GetAvailableTargetOperatorsTest
    {
        [Test]
        [CreateScene]
        public async Task GetAvailableTargetOperatorsTool_WithInteractableButton_ReturnsJsonContainingTargetAndOperators()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<GraphicRaycaster>();
            var buttonGo = new GameObject("StartButton");
            buttonGo.transform.SetParent(canvasGo.transform);
            buttonGo.AddComponent<Button>();
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var actual = await GetAvailableTargetOperators.GetAvailableTargetOperatorsTool(reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("StartButton").And.Contain("operators"));
        }

        [Test]
        [CreateScene]
        public async Task GetAvailableTargetOperatorsTool_WithButtonHavingText_ReturnsJsonContainingText()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<GraphicRaycaster>();
            var buttonGo = new GameObject("StartButton");
            buttonGo.transform.SetParent(canvasGo.transform);
            buttonGo.AddComponent<Button>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform);
            var textComp = textGo.AddComponent<Text>();
            textComp.text = "Start";
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var actual = await GetAvailableTargetOperators.GetAvailableTargetOperatorsTool(reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("Start"));
        }

        [Test]
        [CreateScene]
        public async Task GetAvailableTargetOperatorsTool_NoInteractableComponents_ReturnsNoOperableMessage()
        {
            var actual = await GetAvailableTargetOperators.GetAvailableTargetOperatorsTool(reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Not.StartWith("["));
        }

        [Test]
        [CreateScene]
        public async Task GetAvailableTargetOperatorsTool_ReachableFalse_ReturnsAllInteractable()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<GraphicRaycaster>();
            var buttonGo = new GameObject("HiddenButton");
            buttonGo.transform.SetParent(canvasGo.transform);
            buttonGo.AddComponent<Button>();
            // Place another UI panel on top to block reachability
            var overlayGo = new GameObject("Overlay");
            overlayGo.transform.SetParent(canvasGo.transform);
            var overlayImage = overlayGo.AddComponent<Image>();
            overlayImage.color = Color.black;
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var actual = await GetAvailableTargetOperators.GetAvailableTargetOperatorsTool(reachable: false, config: new McpConfig());

            Assert.That(actual, Does.Contain("HiddenButton"));
        }
    }
}
