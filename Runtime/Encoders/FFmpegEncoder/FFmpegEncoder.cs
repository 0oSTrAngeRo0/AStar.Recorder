using System;
using Unity.Collections;
using UnityEngine;

namespace AStar.Recorder
{
    public class FFmpegEncoder : IEncoder
    {
        public event Action OnEncoded;

        public event Action<NativeArray<byte>> OnVideoDataConsumed
        {
            add => m_VideoEncoder.OnDataConsumed += value;
            remove => m_VideoEncoder.OnDataConsumed -= value;
        }

        public event Action<NativeArray<byte>> OnAudioDataConsumed
        {
            add => m_AudioEncoder.OnDataConsumed += value;
            remove => m_AudioEncoder.OnDataConsumed -= value;
        }

        private bool m_IsReceivingData;
        private readonly FFmpegConfig m_Config;
        private readonly FFmpegPipe m_VideoEncoder;
        private readonly FFmpegPipe m_AudioEncoder;

        public FFmpegEncoder(FFmpegConfig config)
        {
            m_Config = config;
            m_VideoEncoder = new FFmpegPipe();
            m_AudioEncoder = new FFmpegPipe();
        }

        public void Open()
        {
            m_VideoEncoder.Open(m_Config.VideoConfig.GetVideoArguments());
            m_AudioEncoder.Open(m_Config.AudioConfig.GetAudioArguments());
            m_IsReceivingData = true;
        }

        public void PushVideoFrame(NativeArray<byte> video)
        {
            if (!m_IsReceivingData) return;
            m_VideoEncoder.PushData(video);
        }

        public void PushAudioFrame(NativeArray<byte> audio)
        {
            if (!m_IsReceivingData) return;
            m_AudioEncoder.PushData(audio);
        }

        public void Close()
        {
            if (m_IsReceivingData == false) return;
            m_IsReceivingData = false;
            m_VideoEncoder.Close();
            m_AudioEncoder.Close();
            CoroutineManager.WaitUntil(() => m_VideoEncoder.IsExited && m_AudioEncoder.IsExited, () =>
            {
                FFmpegDispatcher merge = new FFmpegDispatcher(FFmpegUtils.FFmpegPath, m_Config.GetMergeArguments());
                CoroutineManager.WaitUntil(() => merge.IsExited, OnEncoded);
            });
        }
    }
}