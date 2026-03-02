// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;

namespace GameplayMcp.Tools
{
    [TestFixture]
    public class GetScenesTest
    {
        [Test]
        [CreateScene]
        public async Task GetScenesTool_DefaultScene_ReturnsJsonWithSceneName()
        {
            var sut = new GetScenes();

            var actual = await sut.GetScenesTool();

            var scenes = JsonSerializer.Deserialize<JsonElement[]>(actual);
            Assert.That(scenes, Is.Not.Empty);
            Assert.That(scenes[0].GetProperty("name").GetString(), Is.Not.Empty);
        }

        [Test]
        [CreateScene]
        public async Task GetScenesTool_DefaultScene_ActiveIsTrue()
        {
            var sut = new GetScenes();

            var actual = await sut.GetScenesTool();

            var scenes = JsonSerializer.Deserialize<JsonElement[]>(actual);
            Assert.That(scenes[0].GetProperty("active").GetBoolean(), Is.True);
        }
    }
}
