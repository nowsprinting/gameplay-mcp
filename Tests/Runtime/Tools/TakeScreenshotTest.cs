// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;

namespace GameplayMcp.Tools
{
    [TestFixture]
    [IgnoreBatchMode("GameView is required")]
    public class TakeScreenshotTest
    {
        [Test]
        [CreateScene]
        [FocusGameView]
        public async Task TakeScreenshotTool_DefaultParameters_ReturnsImageContentBlock()
        {
            var sut = new TakeScreenshot();

            var actual = await sut.TakeScreenshotTool();

            Assert.That(actual, Is.TypeOf<ImageContentBlock>());
        }

        [Test]
        [CreateScene]
        [FocusGameView]
        public async Task TakeScreenshotTool_FormatPng_ReturnsPngMimeType()
        {
            var sut = new TakeScreenshot();

            var actual = await sut.TakeScreenshotTool(format: "png");

            Assert.That(((ImageContentBlock)actual).MimeType, Is.EqualTo("image/png"));
        }

        [Test]
        [CreateScene]
        [FocusGameView]
        public async Task TakeScreenshotTool_FormatJpeg_ReturnsJpegMimeType()
        {
            var sut = new TakeScreenshot();

            var actual = await sut.TakeScreenshotTool(format: "jpeg");

            Assert.That(((ImageContentBlock)actual).MimeType, Is.EqualTo("image/jpeg"));
        }

        [Test]
        [CreateScene]
        [GameViewResolution(GameViewResolution.FullHD)]
        public async Task TakeScreenshotTool_LongSideExceedsMax_ReturnsDownscaledImage()
        {
            var sut = new TakeScreenshot();

            var actual = await sut.TakeScreenshotTool(format: "png", maxLongSide: 800);

            var imageBlock = (ImageContentBlock)actual;
            var tex = new Texture2D(1, 1);
            tex.LoadImage(imageBlock.DecodedData.ToArray());
            Assert.That(Math.Max(tex.width, tex.height), Is.LessThanOrEqualTo(800));
            UnityEngine.Object.Destroy(tex);
        }

        [Test]
        [CreateScene]
        [GameViewResolution(GameViewResolution.VGA)]
        public async Task TakeScreenshotTool_LongSideBelowMax_ReturnsOriginalSizeImage()
        {
            var sut = new TakeScreenshot();

            var actual = await sut.TakeScreenshotTool(format: "png", maxLongSide: 1568);

            var imageBlock = (ImageContentBlock)actual;
            var tex = new Texture2D(1, 1);
            tex.LoadImage(imageBlock.DecodedData.ToArray());
            Assert.That(Math.Max(tex.width, tex.height), Is.EqualTo(640));
            UnityEngine.Object.Destroy(tex);
        }
    }
}
