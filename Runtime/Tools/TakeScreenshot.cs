// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TestHelper.RuntimeInternals;
using UnityEngine;

namespace GameplayMcp.Tools
{
    /// <summary>
    /// MCP tool that captures the current game screen and returns it as an image.
    /// </summary>
    [McpServerToolType]
    public static class TakeScreenshot
    {
        /// <summary>
        /// Captures the current game screen and returns it as an image.
        /// </summary>
        /// <param name="format">Image format: "jpeg" (default) or "png".</param>
        /// <param name="maxPixels">Maximum length of the long side in pixels. The image is scaled down if it exceeds this value. Defaults to 1568.</param>
        /// <param name="quality">JPEG encoding quality (1-100). Only used when format is "jpeg". Defaults to 75.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ImageContentBlock on success, TextContentBlock with error message on failure.</returns>
        [McpServerTool(Name = "take_screenshot", ReadOnly = true, Destructive = false)]
        [Description("Captures the current game screen and returns it as an image.")]
        public static async Task<ContentBlock> TakeScreenshotTool(
            [Description("Image format: \"jpeg\" (default) or \"png\".")]
            string format = "jpeg",
            [Description("Maximum length of the long side in pixels. The image is scaled down if it exceeds this value. Defaults to 1568.")]
            int maxPixels = 1568,
            [Description("JPEG encoding quality (1-100). Only used when format is \"jpeg\". Defaults to 75.")]
            int quality = 75,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await UniTask.SwitchToMainThread(cancellationToken);

                var width = Screen.width;
                var height = Screen.height;
                var longSide = Math.Max(width, height);
                var scale = longSide > maxPixels ? (float)maxPixels / longSide : 1.0f;

                byte[] bytes;
                string mimeType;
                if (string.Equals(format, "png", StringComparison.OrdinalIgnoreCase))
                {
                    bytes = await ScreenshotHelper.TakeScreenshotAsPngBytesAsync(scale);
                    mimeType = "image/png";
                }
                else
                {
                    bytes = await ScreenshotHelper.TakeScreenshotAsJpegBytesAsync(scale, quality);
                    mimeType = "image/jpeg";
                }

                return ImageContentBlock.FromBytes(bytes, mimeType);
            }
            catch (Exception e)
            {
                return new TextContentBlock { Text = e.ToString() };
            }
        }
    }
}
