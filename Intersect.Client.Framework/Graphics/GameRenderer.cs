﻿using System;
using System.Collections.Generic;
using System.IO;

using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.GenericClasses;

using JetBrains.Annotations;

namespace Intersect.Client.Framework.Graphics
{
    public abstract class GameRenderer : IRenderer
    {

        public GameRenderer()
        {
            ScreenshotRequests = new List<Stream>();
        }

        [NotNull] public List<Stream> ScreenshotRequests { get; }

        public Resolution ActiveResolution => new Resolution(PreferredResolution, OverrideResolution);

        public bool HasOverrideResolution => OverrideResolution != Resolution.Empty;

        public Resolution OverrideResolution { get; set; }

        public Resolution PreferredResolution { get; set; }

        public abstract void Init();

        /// <summary>
        ///     Called before a frame is drawn, if the renderer must re-created or anything it does it here.
        /// </summary>
        /// <returns></returns>
        public abstract bool Begin();

        public abstract bool BeginScreenshot();

        /// <summary>
        ///     Called when the frame is done being drawn, generally used to finally display the content to the screen.
        /// </summary>
        public abstract void End(bool frame = true);

        public abstract void EndScreenshot();

        /// <summary>
        ///     Clears everything off the render target with a specified color.
        /// </summary>
        public abstract void Clear(Color color);

        /// <inheritdoc />
        public abstract void SetRenderTexture(IRenderTexture renderTexture);

        public abstract void SetView(FloatRect view);

        public abstract FloatRect GetView();

        public abstract IFont LoadFont(string filename);

        public abstract void DrawTexture(
            ITexture tex,
            float sx,
            float sy,
            float sw,
            float sh,
            float tx,
            float ty,
            float tw,
            float th,
            Color renderColor,
            IRenderTexture gameRenderTarget = null,
            GameBlendModes blendMode = GameBlendModes.None,
            IShader shader = null,
            float rotationDegrees = 0.0f,
            bool isUi = false,
            bool drawImmediate = false
        );

        public abstract int Fps { get; protected set; }

        public abstract int ScreenWidth { get; }

        public abstract int ScreenHeight { get; }

        public abstract bool DisplayModeChanged();

        public abstract IRenderTexture CreateRenderTexture(int width, int height);

        public abstract ITexture LoadTexture(TextureType textureType, string assetName);

        public abstract ITexture LoadTexture(string filename, ITexturePackFrame texturePackFrame = null);

        public abstract ITexture LoadTexture(
            [NotNull] string assetName,
            [NotNull] Func<Stream> createStream
        );

        public abstract ITexture GetWhiteTexture();

        public abstract Pointf MeasureText(string text, IFont gameFont, float fontScale);

        public abstract void DrawString(
            string text,
            IFont gameFont,
            float x,
            float y,
            float fontScale,
            Color fontColor,
            bool worldPos = true,
            IRenderTexture gameRenderTexture = null,
            Color borderColor = null
        );

        public abstract void DrawString(
            string text,
            IFont gameFont,
            float x,
            float y,
            float fontScale,
            Color fontColor,
            bool worldPos,
            IRenderTexture gameRenderTexture,
            FloatRect clipRect,
            Color borderColor = null
        );

        //Buffers
        public abstract GameTileBuffer CreateTileBuffer();

        public abstract void DrawTileBuffer(GameTileBuffer buffer);

        public abstract void Close();

        public abstract List<string> GetValidVideoModes();

        public abstract IShader LoadShader(string name);

        public void RequestScreenshot(string screenshotDir = "screenshots")
        {
            if (!Directory.Exists(screenshotDir))
            {
                Directory.CreateDirectory(screenshotDir ?? "");
            }

            var screenshotNumber = 0;
            string screenshotFile;
            do
            {
                screenshotFile = Path.Combine(
                    screenshotDir ?? "", $"{DateTime.Now:yyyyMMdd-HHmmssfff}{screenshotNumber}.png"
                );

                ++screenshotNumber;
            } while (File.Exists(screenshotFile) && screenshotNumber < 4);

            ScreenshotRequests.Add(File.OpenWrite(screenshotFile));
        }

    }

}
