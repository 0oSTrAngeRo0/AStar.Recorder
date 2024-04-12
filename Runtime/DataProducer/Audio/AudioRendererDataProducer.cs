using System;
using Unity.Collections;
using UnityEngine;

namespace AStar.Recorder
{
    public class AudioRendererDataProducer : MonoSingletonBase<AudioRendererDataProducer>, IAudioDataProducer
    {
        private const NativeArrayOptions M_BUFFER_OPTIONS = NativeArrayOptions.UninitializedMemory;

        private AudioMetadata m_Metadata;
        private bool m_IsCapturing;
        private ProducerSettings m_Settings;

        public ProducerSettings Settings
        {
            get => m_Settings;
            set
            {
                if (m_IsCapturing)
                {
                    Debug.LogError("Cannot change settings when capturing");
                    return;
                }

                m_Settings = value;
            }
        }

        public struct ProducerSettings
        {
            public Allocator BufferAllocator;
        }

        private void OnEnable()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioSettingsChanged;
            OnAudioSettingsChanged(false);
        }

        private void LateUpdate()
        {
            if (!m_IsCapturing) return;

            int count = AudioRenderer.GetSampleCountForCaptureFrame() * (int)m_Metadata.Channels;
            NativeArray<float> buffer = new NativeArray<float>(count, m_Settings.BufferAllocator, M_BUFFER_OPTIONS);
            bool success = AudioRenderer.Render(buffer);

            if (count <= 0)
            {
                Debug.Log("There is no samples produced");
                buffer.Dispose();
                return;
            }

            if (!success)
            {
                Debug.LogError("Failed to get samples");
                buffer.Dispose();
                return;
            }

            NativeArray<byte> result = buffer.Reinterpret<byte>(sizeof(float));
            OnDataProduced?.Invoke(result);
        }

        private void OnDisable()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioSettingsChanged;
        }

        private void OnAudioSettingsChanged(bool isChangedByDevices)
        {
            if (m_IsCapturing)
            {
                Debug.LogError("AudioSettings changed when capturing, it is an invalid operation!");
                return;
            }
            AudioConfiguration config = AudioSettings.GetConfiguration();
            m_Metadata.Channels = config.speakerMode.GetChannelCount();
            m_Metadata.SampleRate = (uint)config.sampleRate;
        }

        public void StartProduce()
        {
            if (m_IsCapturing) return;
            AudioRenderer.Start();
            m_IsCapturing = true;
        }

        public void PauseProduce() => StopProduce();

        public void ResumeProduce() => StartProduce();

        public void StopProduce()
        {
            if (!m_IsCapturing) return;
            AudioRenderer.Stop();
            m_IsCapturing = false;
        }

        public event Action<NativeArray<byte>> OnDataProduced;
        public AudioMetadata Metadata => m_Metadata;
    }
}