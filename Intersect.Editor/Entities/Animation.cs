﻿using Intersect.Editor.Content;
using Intersect.Framework.Core;
using Intersect.GameObjects;
using Intersect.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace Intersect.Editor.Entities;


public partial class Animation
{

    private bool mInfiniteLoop;

    private int mLowerFrame;

    private int mLowerLoop;

    private long mLowerTimer;

    private int mRenderDir;

    private float mRenderX;

    private float mRenderY;

    private bool mShowLower = true;

    private bool mShowUpper = true;

    private int mUpperFrame;

    private int mUpperLoop;

    private long mUpperTimer;

    public AnimationBase MyBase;

    public Animation(AnimationBase animBase, bool loopForever)
    {
        MyBase = animBase;
        mLowerLoop = animBase.Lower.LoopCount;
        mUpperLoop = animBase.Upper.LoopCount;
        mLowerTimer = Timing.Global.MillisecondsUtc + animBase.Lower.FrameSpeed;
        mUpperTimer = Timing.Global.MillisecondsUtc + animBase.Upper.FrameSpeed;
        mInfiniteLoop = loopForever;
    }

    public void Draw(RenderTarget2D target, bool upper = false, bool alternate = false)
    {
        if (!upper && alternate != MyBase.Lower.AlternateRenderLayer)
        {
            return;
        }

        if (upper && alternate != MyBase.Upper.AlternateRenderLayer)
        {
            return;
        }

        if (!upper)
        {
            //Draw Lower
            var tex = GameContentManager.GetTexture(GameContentManager.TextureType.Animation, MyBase.Lower.Sprite);
            if (mShowLower)
            {
                if (mLowerFrame >= MyBase.Lower.FrameCount)
                {
                    return;
                }

                if (tex != null)
                {
                    if (MyBase.Lower.XFrames > 0 && MyBase.Lower.YFrames > 0)
                    {
                        var frameWidth = (int) tex.Width / MyBase.Lower.XFrames;
                        var frameHeight = (int) tex.Height / MyBase.Lower.YFrames;
                        Core.Graphics.DrawTexture(
                            tex,
                            new RectangleF(
                                mLowerFrame % MyBase.Lower.XFrames * frameWidth,
                                (float) Math.Floor((double) mLowerFrame / MyBase.Lower.XFrames) * frameHeight,
                                frameWidth, frameHeight
                            ),
                            new RectangleF(
                                mRenderX - frameWidth / 2, mRenderY - frameHeight / 2, frameWidth, frameHeight
                            ), System.Drawing.Color.White, target, BlendState.NonPremultiplied
                        );
                    }
                }

                Core.Graphics.AddLight(
                    Options.Instance.Map.MapWidth * Options.Instance.Map.TileWidth -
                    Core.Graphics.CurrentView.Left +
                    (int) mRenderX +
                    MyBase.Lower.Lights[mLowerFrame].OffsetX,
                    Options.Instance.Map.MapHeight * Options.Instance.Map.TileHeight -
                    Core.Graphics.CurrentView.Top +
                    (int) mRenderY +
                    MyBase.Lower.Lights[mLowerFrame].OffsetY, MyBase.Lower.Lights[mLowerFrame]
                );
            }
        }
        else
        {
            //Draw Upper
            var tex = GameContentManager.GetTexture(GameContentManager.TextureType.Animation, MyBase.Upper.Sprite);
            if (mShowUpper)
            {
                if (mUpperFrame >= MyBase.Upper.FrameCount)
                {
                    return;
                }

                if (tex != null)
                {
                    if (MyBase.Upper.XFrames > 0 && MyBase.Upper.YFrames > 0)
                    {
                        var frameWidth = (int) tex.Width / MyBase.Upper.XFrames;
                        var frameHeight = (int) tex.Height / MyBase.Upper.YFrames;
                        Core.Graphics.DrawTexture(
                            tex,
                            new RectangleF(
                                mUpperFrame % MyBase.Upper.XFrames * frameWidth,
                                (float) Math.Floor((double) mUpperFrame / MyBase.Upper.XFrames) * frameHeight,
                                frameWidth, frameHeight
                            ),
                            new RectangleF(
                                mRenderX - frameWidth / 2, mRenderY - frameHeight / 2, frameWidth, frameHeight
                            ), System.Drawing.Color.White, target, BlendState.NonPremultiplied
                        );
                    }
                }

                Core.Graphics.AddLight(
                    Options.Instance.Map.MapWidth * Options.Instance.Map.TileWidth -
                    Core.Graphics.CurrentView.Left +
                    (int) mRenderX +
                    MyBase.Upper.Lights[mUpperFrame].OffsetX,
                    Options.Instance.Map.MapHeight * Options.Instance.Map.TileHeight -
                    Core.Graphics.CurrentView.Top +
                    (int) mRenderY +
                    MyBase.Upper.Lights[mUpperFrame].OffsetY, MyBase.Upper.Lights[mUpperFrame]
                );
            }
        }
    }

    public void SetPosition(float x, float y, int dir)
    {
        mRenderX = x;
        mRenderY = y;
        mRenderDir = dir;
    }

    public void Update()
    {
        if (mLowerTimer < Timing.Global.MillisecondsUtc && mShowLower)
        {
            mLowerFrame++;
            if (mLowerFrame >= MyBase.Lower.FrameCount)
            {
                mLowerLoop--;
                mLowerFrame = 0;
                if (mLowerLoop < 0)
                {
                    if (mInfiniteLoop)
                    {
                        mLowerLoop = MyBase.Lower.LoopCount;
                    }
                    else
                    {
                        mShowLower = false;
                    }
                }
            }

            mLowerTimer = Timing.Global.MillisecondsUtc + MyBase.Lower.FrameSpeed;
        }

        if (mUpperTimer < Timing.Global.MillisecondsUtc && mShowUpper)
        {
            mUpperFrame++;
            if (mUpperFrame >= MyBase.Upper.FrameCount)
            {
                mUpperLoop--;
                mUpperFrame = 0;
                if (mUpperLoop < 0)
                {
                    if (mInfiniteLoop)
                    {
                        mUpperLoop = MyBase.Upper.LoopCount;
                    }
                    else
                    {
                        mShowUpper = false;
                    }
                }
            }

            mUpperTimer = Timing.Global.MillisecondsUtc + MyBase.Upper.FrameSpeed;
        }
    }

}
