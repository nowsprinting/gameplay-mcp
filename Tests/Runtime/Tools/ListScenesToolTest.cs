// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;

namespace GameplayMcp.Tools
{
    [TestFixture]
    public class ListScenesToolTest
    {
        [Test]
        [CreateScene]
        public async Task ListScenes_DefaultScene_ReturnsJsonWithSceneName()
        {
            var actual = await ListScenesTool.ListScenes();

            var scenes = JsonSerializer.Deserialize<JsonElement[]>(actual);
            Assert.That(scenes, Is.Not.Empty);
            Assert.That(scenes[0].GetProperty("name").GetString(), Is.Not.Empty);
        }

        [Test]
        [CreateScene]
        public async Task ListScenes_DefaultScene_ActiveIsTrue()
        {
            var actual = await ListScenesTool.ListScenes();

            var scenes = JsonSerializer.Deserialize<JsonElement[]>(actual);
            Assert.That(scenes, Has.Some.Matches<JsonElement>(s => s.GetProperty("active").GetBoolean()));
        }
    }
}
