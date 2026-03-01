// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using NUnit.Framework;

namespace GameplayMcp
{
    /// <summary>
    /// Integration tests that connect to <see cref="McpServer"/> via the MCP client SDK.
    /// </summary>
    [TestFixture]
    [Timeout(5000)]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    public class McpServerTest
    {
        private const string ServerEndpoint = "http://localhost:8010/mcp";
        private const int RetryCount = 10;
        private const int RetryDelayMilliseconds = 500;

        private McpServer _server;
        private McpClient _client;

        [SetUp]
        public async Task SetUp()
        {
            _server = new McpServer(new McpConfig());
            _server.StartAsync().Forget();

            _client = await ConnectAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_client != null)
            {
                await _client.DisposeAsync();
                _client = null;
            }

            _server?.Stop();
            _server?.Dispose();
            _server = null;
        }

        [Test]
        public async Task ListToolsAsync_ConnectToServer_ContainsEchoTool()
        {
            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "echo"));
        }

        [Test]
        public async Task ListToolsAsync_ConnectToServer_ContainsFindGameObjectTool()
        {
            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "find_gameobject"));
        }

        [Test]
        public async Task ListToolsAsync_DisableFindGameObjectTool_NotContainsFindGameObjectTool()
        {
            // Restart server with EnableFindGameObjectTool = false
            await _client.DisposeAsync();
            _client = null;
            _server.Stop();
            _server.Dispose();

            _server = new McpServer(new McpConfig { EnableFindGameObjectTool = false });
            _server.StartAsync().Forget();
            _client = await ConnectAsync();

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.None.Matches<McpClientTool>(t => t.Name == "find_gameobject"));
        }

        [Test]
        public async Task CallToolAsync_EchoWithMessage_ReturnsTextContentWithSameMessage()
        {
            const string Expected = "Hello, MCP!";

            var result = await _client.CallToolAsync("echo", new Dictionary<string, object> { ["message"] = Expected });

            var actual = result.Content.OfType<TextContentBlock>().First().Text;
            Assert.That(actual, Is.EqualTo(Expected));
        }

        [Test]
        public async Task CallToolAsync_EchoWithEmptyString_ReturnsTextContentWithEmptyString()
        {
            var result = await _client.CallToolAsync("echo", new Dictionary<string, object> { ["message"] = "" });

            var actual = result.Content.OfType<TextContentBlock>().First().Text;
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public async Task CallToolAsync_EchoWithSpecialCharacters_ReturnsTextContentWithSameMessage()
        {
            const string Expected = "日本語テスト 🎮\n改行あり";

            var result = await _client.CallToolAsync("echo", new Dictionary<string, object> { ["message"] = Expected });

            var actual = result.Content.OfType<TextContentBlock>().First().Text;
            Assert.That(actual, Is.EqualTo(Expected));
        }

        private static async Task<McpClient> ConnectAsync()
        {
            Exception lastException = null;
            for (var i = 0; i < RetryCount; i++)
            {
                try
                {
                    var options = new HttpClientTransportOptions
                    {
                        Endpoint = new Uri(ServerEndpoint),
                        TransportMode = HttpTransportMode.StreamableHttp,
                    };
                    var transport = new HttpClientTransport(options);
                    return await McpClient.CreateAsync(transport);
                }
                catch (Exception e)
                {
                    lastException = e;
                    await Task.Delay(RetryDelayMilliseconds);
                }
            }

            throw lastException ?? new InvalidOperationException("Failed to connect to MCP server.");
        }
    }
}
