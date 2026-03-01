// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;

namespace GameplayMcp.Internals
{
    [TestFixture]
    public class CommandLineArgsTest
    {
        [Test]
        public void GetListenPrefix_WithArgument_ReturnsSpecifiedPrefix()
        {
            var args = new[] { "-gameplayMcpListenPrefix", "http://+:9999/" };

            var actual = CommandLineArgs.GetListenPrefix(args);

            Assert.That(actual, Is.EqualTo("http://+:9999/"));
        }

        [Test]
        public void GetListenPrefix_WithoutArgument_ReturnsDefaultPrefix()
        {
            var actual = CommandLineArgs.GetListenPrefix(new string[0]);

            Assert.That(actual, Is.EqualTo(CommandLineArgs.DefaultListenPrefix));
        }

        [Test]
        public void GetListenPrefix_WithOtherArguments_ReturnsSpecifiedPrefix()
        {
            var args = new[] { "-batchmode", "-gameplayMcpListenPrefix", "http://+:8080/", "-nographics" };

            var actual = CommandLineArgs.GetListenPrefix(args);

            Assert.That(actual, Is.EqualTo("http://+:8080/"));
        }

        [Test]
        public void DictionaryFromCommandLineArgs_MultipleKeyValuePairs_ParsedCorrectly()
        {
            var args = new[] { "-flag1", "-key1", "value1", "-flag2" };
            var expected = new System.Collections.Generic.Dictionary<string, string>
            {
                { "-flag1", string.Empty },
                { "-key1", "value1" },
                { "-flag2", string.Empty },
            };

            var actual = CommandLineArgs.DictionaryFromCommandLineArgs(args);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
