// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameplayMcp.Tools;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UnityEngine;

namespace GameplayMcp
{
    /// <summary>
    /// MCP server using Streamable HTTP transport over HttpListener.
    /// Listens on the prefix specified by <see cref="McpConfig.ListenPrefix"/> and handles POST, GET, and DELETE requests to /mcp.
    /// </summary>
    public sealed class McpServer : IDisposable
    {
        private const string McpPath = "/mcp";

        private readonly McpConfig _config;
        private readonly HttpListener _listener;
        private CancellationTokenSource _cts;

        private readonly ConcurrentDictionary<string, (StreamableHttpServerTransport Transport,
            ModelContextProtocol.Server.McpServer Server)> _sessions;

        private readonly McpServerOptions _serverOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="McpServer"/> class.
        /// </summary>
        /// <param name="config">Configuration for the MCP server.</param>
        public McpServer(McpConfig config)
        {
            _config = config;
            _listener = new HttpListener();
            _listener.Prefixes.Add(_config.ListenPrefix);
            _sessions =
                new ConcurrentDictionary<string, (StreamableHttpServerTransport, ModelContextProtocol.Server.McpServer
                    )>();

            var tools = new McpServerPrimitiveCollection<McpServerTool>();
            tools.Add(McpServerTool.Create(typeof(EchoTool).GetMethod(nameof(EchoTool.Echo))));

            if (_config.EnableFindGameObjectTool)
            {
                var findGameObject = new FindGameObject(_config);
                tools.Add(McpServerTool.Create(
                    typeof(FindGameObject).GetMethod(nameof(FindGameObject.FindGameObjectTool)),
                    findGameObject));
            }

            _serverOptions = new McpServerOptions
            {
                ServerInfo = new Implementation { Name = Application.productName, Version = Application.version },
                Capabilities = new ServerCapabilities { Tools = new ToolsCapability() },
                ToolCollection = tools,
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _listener.Close();
            foreach (var session in _sessions.Values)
            {
                session.Transport.DisposeAsync().GetAwaiter().GetResult();
                ((IAsyncDisposable)session.Server).DisposeAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Starts listening for MCP client requests on <see cref="McpConfig.ListenPrefix"/>.
        /// </summary>
        public async UniTask StartAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _listener.Start();
            Debug.Log($"Gameplay MCP Server started. Listening on {_config.ListenPrefix}");

            // Stop the listener when cancellation is requested
            var ct = _cts.Token;
            ct.Register(() => _listener.Stop());

            while (true)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (HttpListenerException)
                {
                    break;
                }

                // Fire-and-forget: process request without blocking the accept loop
                HandleRequestAsync(context, ct).Forget();
            }

            Debug.Log("Gameplay MCP Server stopped.");
        }

        /// <summary>
        /// Stop listening for requests and dispose all active sessions.
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTask HandleRequestAsync(HttpListenerContext context, CancellationToken ct)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.Url.AbsolutePath != McpPath)
                {
                    response.StatusCode = 404;
                    return;
                }

                switch (request.HttpMethod.ToUpperInvariant())
                {
                    case "POST":
                        await HandlePostAsync(request, response, ct);
                        break;
                    case "GET":
                        await HandleGetAsync(request, response, ct);
                        break;
                    case "DELETE":
                        await HandleDeleteAsync(request, response, ct);
                        break;
                    default:
                        response.StatusCode = 405;
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown; no action needed.
                // Not re-thrown because HandleRequestAsync is called via .Forget(), so there is no caller to receive it.
                // Swallowing here allows the finally block to close the response cleanly.
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                try
                {
                    response.StatusCode = 500;
                }
                catch
                {
                    // Headers may already be sent; ignore secondary error
                }
            }
            finally
            {
                response.Close();
            }
        }

        private async UniTask HandlePostAsync(HttpListenerRequest request, HttpListenerResponse response,
            CancellationToken ct)
        {
            // Deserialize the request body as a JSON-RPC message
            using var reader = new StreamReader(request.InputStream);
            var body = await reader.ReadToEndAsync();
            var message = JsonSerializer.Deserialize<JsonRpcMessage>(body, McpJsonUtilities.DefaultOptions);

            if (message == null)
            {
                response.StatusCode = 400;
                return;
            }

            // Route to the existing session or create a new one
            StreamableHttpServerTransport transport;
            var sessionId = request.Headers["Mcp-Session-Id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                // SessionId must be set explicitly via object initializer;
                // StreamableHttpServerTransport does not auto-generate it (that is done by StreamableHttpHandler in ASP.NET Core)
                transport = new StreamableHttpServerTransport { SessionId = Guid.NewGuid().ToString("N") };
                var server = ModelContextProtocol.Server.McpServer.Create(transport, _serverOptions);
                _sessions.TryAdd(transport.SessionId, (transport, server));
                _ = server.RunAsync(ct);
            }
            else if (_sessions.TryGetValue(sessionId, out var existing))
            {
                transport = existing.Transport;
            }
            else
            {
                response.StatusCode = 404;
                return;
            }

            // Buffer in MemoryStream first so we can set response headers before writing the body.
            // HttpListenerResponse sends headers automatically on the first write to OutputStream,
            // but Mcp-Session-Id must be set before writing begins.
            using var memoryStream = new MemoryStream();
            var hasResponse = await transport.HandlePostRequestAsync(message, memoryStream, ct);

            response.Headers["Mcp-Session-Id"] = transport.SessionId;

            if (hasResponse)
            {
                response.ContentType = "text/event-stream";
                response.Headers["Cache-Control"] = "no-cache";
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(response.OutputStream, cancellationToken: ct);
            }
            else
            {
                response.StatusCode = 202;
            }
        }

        private async UniTask HandleGetAsync(HttpListenerRequest request, HttpListenerResponse response,
            CancellationToken ct)
        {
            var sessionId = request.Headers["Mcp-Session-Id"];
            if (!_sessions.TryGetValue(sessionId ?? string.Empty, out var session))
            {
                response.StatusCode = 404;
                return;
            }

            response.ContentType = "text/event-stream";
            response.Headers["Cache-Control"] = "no-cache";

            // Long-running: streams unsolicited server-to-client messages until cancelled
            await session.Transport.HandleGetRequestAsync(response.OutputStream, ct);
        }

        private Task HandleDeleteAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken ct)
        {
            var sessionId = request.Headers["Mcp-Session-Id"];
            if (!string.IsNullOrEmpty(sessionId) && _sessions.TryRemove(sessionId, out var session))
            {
                session.Transport.DisposeAsync().GetAwaiter().GetResult();
                ((IAsyncDisposable)session.Server).DisposeAsync().GetAwaiter().GetResult();
            }

            response.StatusCode = 200;
            return Task.CompletedTask;
        }
    }
}
