using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace AStar.Recorder
{
    public class OfflineRecorder : MonoSingletonBase<OfflineRecorder>, IRecorder
    {
        private IEncoder m_Encoder;
        private bool m_IsCapturing;
        private IAudioDataProducer m_AudioDataProducer;
        private ASyncGPURequestPipe m_ASyncGPURequestPipe;
        private IVideoDataProducer m_VideoDataProducer;
        private string m_OutputPath;
        private RecorderConfig m_Config;

        [Serializable]
        public struct RecorderConfig
        {
            public uint Fps;
            public RenderTexture RenderTexture;
            public string CacheDirectory;
        }

        public string OutputPath
        {
            get => m_OutputPath;
            set
            {
                if (m_IsCapturing) throw new Exception("Cannot set output path when capturing");
                m_OutputPath = value;
            }
        }

        public RecorderConfig Config
        {
            get => m_Config;
            set
            {
                if (m_IsCapturing) throw new Exception("Cannot set config path when capturing");
                m_Config = value;
            }
        }

        public event Action<string> OnEncoded;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            m_IsCapturing = false;
        }

        private void OnDisable() => Stop();

        public void Begin()
        {
            if (m_IsCapturing) return;

            Time.captureFramerate = (int)m_Config.Fps;

            AudioRendererDataProducer audioProducer = AudioRendererDataProducer.Instance;
            audioProducer.Settings = new AudioRendererDataProducer.ProducerSettings
            {
                BufferAllocator = Allocator.Persistent
            };
            m_AudioDataProducer = audioProducer;
            m_AudioDataProducer.OnDataProduced += OnAudioDataProduced;
            m_AudioDataProducer.StartProduce();

            if (m_VideoDataProducer is MonoBehaviour mono) Destroy(mono.gameObject);
            RenderTextureVideoDataProducer videoProducer = RenderTextureVideoDataProducer.Create();
            videoProducer.Settings = new RenderTextureVideoDataProducer.ProducerSettings
            {
                Target = m_Config.RenderTexture
            };
            m_VideoDataProducer = videoProducer;
            m_VideoDataProducer.OnDataProduced += OnVideoDataProduced;
            m_VideoDataProducer.StartProduce();

            m_ASyncGPURequestPipe = new ASyncGPURequestPipe(Allocator.Persistent);
            m_ASyncGPURequestPipe.GetCoroutineManager += GetCoroutineManager;
            m_ASyncGPURequestPipe.OnFrameProduced += OnVideoDataProduced;
            m_ASyncGPURequestPipe.Open();

            m_Encoder = new FFmpegEncoder(new FFmpegConfig
            {
                OutputPath = OutputPath,
                VideoConfig = new VideoConfig
                {
                    Resolution = m_VideoDataProducer.Metadata.Resolution,
                    FilePath = Path.Combine(m_Config.CacheDirectory, $"{DateTime.Now.GetFileString()}.mp4"),
                    Fps = (int)m_Config.Fps,
                    LogLevel = FFmpegLogLevel.Trace
                },
                AudioConfig = new AudioConfig
                {
                    SampleRate = m_AudioDataProducer.Metadata.SampleRate,
                    Channels = m_AudioDataProducer.Metadata.Channels,
                    FilePath = Path.Combine(m_Config.CacheDirectory, $"{DateTime.Now.GetFileString()}.wav"),
                    LogLevel = FFmpegLogLevel.Trace
                }
            });
            m_Encoder.Open();
            m_Encoder.OnAudioDataConsumed += DisposeNativeArray;
            m_Encoder.OnVideoDataConsumed += DisposeNativeArray;

            m_IsCapturing = true;
        }

        public void Pause()
        {
            if (!m_IsCapturing) return;
            m_VideoDataProducer.PauseProduce();
            m_AudioDataProducer.PauseProduce();
        }

        public void Resume()
        {
            if (m_IsCapturing) return;
            m_VideoDataProducer.ResumeProduce();
            m_AudioDataProducer.ResumeProduce();
        }

        public void Stop()
        {
            if (!m_IsCapturing) return;
            Time.captureFramerate = 0;

            m_Encoder.Close();
            m_ASyncGPURequestPipe.Close();
            m_AudioDataProducer.StopProduce();
            m_VideoDataProducer.StopProduce();
            m_AudioDataProducer.OnDataProduced -= OnAudioDataProduced;
            m_VideoDataProducer.OnDataProduced -= OnVideoDataProduced;

            m_Encoder.OnEncoded += () =>
            {
                m_Encoder.OnAudioDataConsumed -= DisposeNativeArray;
                m_Encoder.OnVideoDataConsumed -= DisposeNativeArray;
                m_ASyncGPURequestPipe.OnFrameProduced -= OnVideoDataProduced;
                m_ASyncGPURequestPipe.GetCoroutineManager -= GetCoroutineManager;
                OnEncoded?.Invoke(OutputPath);
            };
            m_IsCapturing = false;
        }

        private void OnAudioDataProduced(NativeArray<byte> data) => m_Encoder.PushAudioFrame(data);
        private void OnVideoDataProduced(NativeArray<byte> data) => m_Encoder.PushVideoFrame(data);
        private void OnVideoDataProduced(RenderTexture rt) => m_ASyncGPURequestPipe.PushVideoFrame(rt);
        private void DisposeNativeArray(NativeArray<byte> array) => array.Dispose();
        private MonoBehaviour GetCoroutineManager() => CoroutineManager.Instance;
    }
}