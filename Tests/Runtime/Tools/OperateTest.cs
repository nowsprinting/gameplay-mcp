// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameplayMcp.Tools
{
    [TestFixture]
    public class OperateTest
    {
        private const string ButtonName = "TestButton";

        private GameObject CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        private void CreateButton(GameObject canvas)
        {
            var buttonGo = new GameObject(ButtonName);
            buttonGo.transform.SetParent(canvas.transform, false);
            buttonGo.AddComponent<Image>(); // Required for GraphicRaycaster to detect this object
            buttonGo.AddComponent<Button>();
        }

        private void CreateEventSystem()
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        private (McpConfig config, SpyOperatorWithoutOverload spy) CreateConfigWithSpyWithoutOverload(
            bool canOperate = true)
        {
            var spy = new SpyOperatorWithoutOverload { CanOperateResult = canOperate };
            var pool = new OperatorPool().Register<SpyOperatorWithoutOverload>();
            // Pre-populate the pool so Rent returns our spy instance
            pool.Return(spy);
            return (new McpConfig { OperatorPool = pool }, spy);
        }

        private (McpConfig config, SpyOperatorWithOverload spy) CreateConfigWithSpyWithOverload()
        {
            var spy = new SpyOperatorWithOverload();
            var pool = new OperatorPool().Register<SpyOperatorWithOverload>();
            // Pre-populate the pool so Rent returns our spy instance
            pool.Return(spy);
            return (new McpConfig { OperatorPool = pool }, spy);
        }

        [Test]
        [CreateScene]
        public async Task OperateTool_WithoutOperatorArgs_CallsBaseOperateAsync()
        {
            CreateButton(CreateCanvas());
            CreateEventSystem();
            var (config, spy) = CreateConfigWithSpyWithoutOverload();
            var sut = new Operate(config);

            await sut.OperateTool(
                operatorName: nameof(SpyOperatorWithoutOverload),
                name: ButtonName);

            Assert.That(spy.WasOperateCalled, Is.True);
        }

        [Test]
        [CreateScene]
        public async Task OperateTool_WithOperatorArgs_CallsMatchingOverload()
        {
            CreateButton(CreateCanvas());
            CreateEventSystem();
            var (config, spy) = CreateConfigWithSpyWithOverload();
            var sut = new Operate(config);

            await sut.OperateTool(
                operatorName: nameof(SpyOperatorWithOverload),
                name: ButtonName,
                operatorArgs: "{\"text\": \"hello\"}");

            Assert.That(spy.WasOverloadOperateCalled, Is.True);
        }

        [Test]
        [CreateScene]
        public async Task OperateTool_WithNullOperatorArgs_CallsBaseOperateAsync()
        {
            CreateButton(CreateCanvas());
            CreateEventSystem();
            var (config, spy) = CreateConfigWithSpyWithOverload();
            var sut = new Operate(config);

            await sut.OperateTool(
                operatorName: nameof(SpyOperatorWithOverload),
                name: ButtonName,
                operatorArgs: null);

            Assert.That(spy.WasBaseOperateCalled, Is.True);
        }

        [Test]
        [CreateScene]
        public async Task OperateTool_WithUnregisteredOperator_ReturnsError()
        {
            var sut = new Operate(new McpConfig());

            var actual = await sut.OperateTool(operatorName: "NonExistentOperator_12345");

            Assert.That(actual, Does.Contain("NonExistentOperator_12345"));
        }

        [Test]
        [CreateScene]
        public async Task OperateTool_CanOperateFalse_ReturnsError()
        {
            CreateButton(CreateCanvas());
            CreateEventSystem();
            var (config, _) = CreateConfigWithSpyWithoutOverload(canOperate: false);
            var sut = new Operate(config);

            var actual = await sut.OperateTool(
                operatorName: nameof(SpyOperatorWithoutOverload),
                name: ButtonName);

            Assert.That(actual, Does.Contain(nameof(SpyOperatorWithoutOverload)));
        }

        [Test]
        [CreateScene]
        public async Task OperateTool_WithMismatchedArgs_ReturnsErrorWithParamInfo()
        {
            CreateButton(CreateCanvas());
            CreateEventSystem();
            var (config, _) = CreateConfigWithSpyWithoutOverload();
            var sut = new Operate(config);

            var actual = await sut.OperateTool(
                operatorName: nameof(SpyOperatorWithoutOverload),
                name: ButtonName,
                operatorArgs: "{\"nonExistentParam\": 42}");

            Assert.That(actual, Does.Contain("nonExistentParam").Or.Contain("OperateAsync").Or.Contain("parameter"));
        }
    }
}
