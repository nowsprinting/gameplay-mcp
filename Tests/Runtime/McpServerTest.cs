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
        private const string ServerListenPrefix = "http://+:8011/";
        private const string ServerEndpoint = "http://localhost:8011/mcp";
        private const int RetryCount = 10;
        private const int RetryDelayMilliseconds = 500;

        private McpServer _server;
        private McpClient _client;

        [TearDown]
        public async Task TearDown()
        {
            await StopConnectionAsync();
        }

        [Test]
        public async Task ListToolsAsync_ConnectToServer_ContainsGetScenesTool()
        {
            await StartConnectionAsync();

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "mygame.get_scenes"));
        }

        [Test]
        public async Task ListToolsAsync_DisableGetScenesTool_NotContainsGetScenesTool()
        {
            var config = new McpConfig();
            config.DisabledTools.Add("mygame.get_scenes");
            await StartConnectionAsync(config);

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.None.Matches<McpClientTool>(t => t.Name == "mygame.get_scenes"));
        }

        [Test]
        public async Task ListToolsAsync_ConnectToServer_ContainsFindGameObjectTool()
        {
            await StartConnectionAsync();

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "mygame.find_gameobject"));
        }

        [Test]
        public async Task ListToolsAsync_ConnectToServer_ContainsTakeScreenshotTool()
        {
            await StartConnectionAsync();

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "mygame.take_screenshot"));
        }

        [Test]
        public async Task CallToolAsync_GetScenes_ReturnsJsonWithActiveScene()
        {
            await StartConnectionAsync();

            var result = await _client.CallToolAsync("mygame.get_scenes", new Dictionary<string, object>());

            var json = result.Content.OfType<TextContentBlock>().First().Text;
            var scenes = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(json);
            Assert.That(scenes, Has.Some.Matches<System.Text.Json.JsonElement>(s => s.GetProperty("active").GetBoolean()));
        }

        [Test]
        public async Task ListToolsAsync_CustomToolsNamespace_ToolNamesHaveCustomPrefix()
        {
            var config = new McpConfig { ToolsNamespace = "custom" };
            await StartConnectionAsync(config);

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "custom.get_scenes"));
        }

        [Test]
        public async Task ListToolsAsync_EmptyToolsNamespace_ToolNamesHaveNoPrefix()
        {
            var config = new McpConfig { ToolsNamespace = "" };
            await StartConnectionAsync(config);

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "get_scenes"));
        }

        [Test]
        public async Task ListToolsAsync_NullToolsNamespace_ToolNamesHaveNoPrefix()
        {
            var config = new McpConfig { ToolsNamespace = null };
            await StartConnectionAsync(config);

            var actual = await _client.ListToolsAsync();

            Assert.That(actual, Has.Some.Matches<McpClientTool>(t => t.Name == "get_scenes"));
        }

        private async Task StartConnectionAsync(McpConfig config = null)
        {
            config ??= new McpConfig();
            config.ListenPrefix = ServerListenPrefix;
            _server = new McpServer(config);
            _server.StartAsync().Forget();
            _client = await ConnectAsync();
        }

        private async Task StopConnectionAsync()
        {
            if (_client != null)
            {
                await _client.DisposeAsync();
                _client = null;
            }

            _server?.Dispose();
            _server = null;
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
