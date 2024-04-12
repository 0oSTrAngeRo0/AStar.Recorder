using System;
using UnityEngine;

namespace AStar.Recorder
{
    public class RenderTextureVideoDataProducer : MonoBehaviour, IVideoDataProducer
    {
        private bool m_IsProducing;
        private ProducerSettings m_Settings;
        private VideoMetadata m_Metadata;

        public ProducerSettings Settings
        {
            get => m_Settings;
            set
            {
                if (m_IsProducing)
                {
                    Debug.LogError("Cannot set producer setting when producing");
                    return;
                }

                RenderTexture rt = value.Target;
                m_Metadata.Resolution = new Vector2Int(rt.width, rt.height);
                m_Settings = value;
            }
        }

        public struct ProducerSettings
        {
            public RenderTexture Target;
        }

        private void Awake() => m_IsProducing = false;

        private void LateUpdate()
        {
            if (!m_IsProducing) return;
            RenderTexture rt = m_Settings.Target;
            if (!rt.IsCreated())
            {
                Debug.LogError("Target RenderTexture is not created!");
                return;
            }

            OnDataProduced?.Invoke(rt);
        }

        public void StartProduce() => m_IsProducing = true;
        public void PauseProduce() => m_IsProducing = false;
        public void ResumeProduce() => m_IsProducing = true;
        public void StopProduce() => m_IsProducing = false;

        public event Action<RenderTexture> OnDataProduced;
        public VideoMetadata Metadata => m_Metadata;

        public static RenderTextureVideoDataProducer Create() =>
            MonoBehaviourExtensions.Generate<RenderTextureVideoDataProducer>();
    }
}