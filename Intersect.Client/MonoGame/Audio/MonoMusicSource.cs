﻿using Intersect.Client.Framework;
using Intersect.Client.Framework.Audio;
using Intersect.Client.Framework.Content;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Localization;
using Intersect.Client.Utilities;
using Intersect.Logging;

using Microsoft.Xna.Framework.Audio;

using NVorbis;

using System;
using System.Diagnostics;
using System.Threading;

namespace Intersect.Client.MonoGame.Audio
{
    public class MonoMusicSource : GameAudioSource
    {
        public VorbisReader Reader { get; set; }

        public DynamicSoundEffectInstance Instance { get; set; }

        private static Thread mUnderlyingThread;

        private static object mInstanceLock = new object();

        private static MonoMusicSource mActiveSource;

        public MonoMusicSource(IGameContext gameContext, AssetReference assetReference) : base(
            gameContext, assetReference, AudioType.Music
        )
        {
            if (mUnderlyingThread == null)
            {
                mUnderlyingThread = new Thread(EnsureBuffersFilled)
                {
                    Priority = ThreadPriority.Lowest,
                    IsBackground = true
                };

                mUnderlyingThread.Start();
            }
        }

        public override IAudioInstance CreateInstance() => new MonoMusicInstance(GameContext, this);

        public DynamicSoundEffectInstance LoadSong()
        {
            lock (mInstanceLock)
            {
                try
                {
                    if (Reader == null)
                    {
                        var assetStream = GameContext.ContentManager.OpenRead(Reference);
                        Reader = new VorbisReader(assetStream, true);
                    }

                    if (Instance != null)
                    {
                        Instance.Dispose();
                        Instance = null;
                    }

                    Instance = new DynamicSoundEffectInstance(
                        Reader.SampleRate, Reader.Channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo
                    );

                    mActiveSource = this;
                    return Instance;
                }
                catch (Exception exception)
                {
                    Log.Error($"Error loading '{Reference}'.", exception);
                    ChatboxMsg.AddMessage(
                        new ChatboxMsg(
                            Strings.Errors.LoadFile.ToString(Strings.Words.lcase_sound), new Color(0xBF, 0x0, 0x0)
                        )
                    );
                }
            }

            mActiveSource = this;
            return null;
        }

        public void Close()
        {
            lock (mInstanceLock)
            {
                Reader?.Dispose();
                Reader = null;

                Instance?.Dispose();
                Instance = null;

                mActiveSource = null;
            }
        }

        private static void EnsureBuffersFilled()
        {
            try
            {
                var buffers = 3;
                var samples = 44100;
                var updateRate = 10;

                while (Globals.IsRunning)
                {
                    Thread.Sleep((int) (1000 / Math.Max(updateRate, 1)));
                    lock (mInstanceLock)
                    {
                        if (mActiveSource != null)
                        {
                            var reader = mActiveSource.Reader;
                            var soundInstance = mActiveSource.Instance;

                            if (reader != null && soundInstance != null && !soundInstance.IsDisposed)
                            {
                                float[] sampleBuffer = null;
                                while (soundInstance.PendingBufferCount < buffers)
                                {
                                    if (sampleBuffer == null)
                                        sampleBuffer = new float[samples];

                                    var read = reader.ReadSamples(sampleBuffer, 0, sampleBuffer.Length);
                                    if (read == 0)
                                    {
                                        reader.DecodedPosition = 0;

                                        continue;
                                    }

                                    var dataBuffer = new byte[read << 1];
                                    for (var sampleIndex = 0; sampleIndex < read; ++sampleIndex)
                                    {
                                        var sample = (short) MathHelper.Clamp(
                                            sampleBuffer[sampleIndex] * 32767f, short.MinValue, short.MaxValue
                                        );

                                        var sampleData = BitConverter.GetBytes(sample);
                                        for (var sampleByteIndex = 0;
                                            sampleByteIndex < sampleData.Length;
                                            ++sampleByteIndex)
                                            dataBuffer[(sampleIndex << 1) + sampleByteIndex] =
                                                sampleData[sampleByteIndex];
                                    }

                                    soundInstance.SubmitBuffer(dataBuffer, 0, read << 1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
#if DEBUG
                Debugger.Break();
#endif
            }
        }

        ~MonoMusicSource()
        {
            Close();
        }
    }
}
