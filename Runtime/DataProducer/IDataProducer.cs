using System;
using Unity.Collections;
using UnityEngine;

namespace AStar.Recorder
{
    public interface IDataProducer
    {
        public void StartProduce();
        public void PauseProduce();
        public void ResumeProduce();
        public void StopProduce();
    }

    public interface IAudioDataProducer : IDataProducer
    {
        public AudioMetadata Metadata { get; }
        public event Action<NativeArray<byte>> OnDataProduced;
    }

    public interface IVideoDataProducer : IDataProducer
    {
        public VideoMetadata Metadata { get; }
        public event Action<RenderTexture> OnDataProduced;
    }

    public struct AudioMetadata
    {
        public uint SampleRate;
        public uint Channels;
    }

    public struct VideoMetadata
    {
        public Vector2Int Resolution;
    }
}