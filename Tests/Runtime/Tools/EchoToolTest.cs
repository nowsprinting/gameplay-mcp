// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// Tests for <see cref="EchoTool"/>.
    /// </summary>
    [TestFixture]
    public class EchoToolTest
    {
        /// <summary>
        /// Verifies that a non-empty string is returned unchanged.
        /// </summary>
        [Test]
        public void Echo_NonEmptyString_ReturnsSameString()
        {
            var result = EchoTool.Echo("Hello, MCP!");

            Assert.That(result, Is.EqualTo("Hello, MCP!"));
        }

        /// <summary>
        /// Verifies that an empty string is returned as an empty string (boundary value).
        /// </summary>
        [Test]
        public void Echo_EmptyString_ReturnsEmptyString()
        {
            var result = EchoTool.Echo(string.Empty);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// Verifies that a string containing Japanese, emoji, and newlines is preserved intact.
        /// </summary>
        [Test]
        public void Echo_StringWithSpecialCharacters_ReturnsSameString()
        {
            const string Input = "日本語テスト 🎮\n改行あり";

            var result = EchoTool.Echo(Input);

            Assert.That(result, Is.EqualTo(Input));
        }
    }
}
