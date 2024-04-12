using System;
using Unity.Collections;
using UnityEngine;

namespace AStar.Recorder
{
    public interface IEncoder
    {
        public event Action OnEncoded;
        public void Open();
        public void Close();
        public void PushVideoFrame(NativeArray<byte> video);
        public void PushAudioFrame(NativeArray<byte> audio);
        public event Action<NativeArray<byte>> OnVideoDataConsumed;
        public event Action<NativeArray<byte>> OnAudioDataConsumed;
    }
}