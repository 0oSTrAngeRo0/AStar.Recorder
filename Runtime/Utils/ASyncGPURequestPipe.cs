using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace AStar.Recorder
{
    public class ASyncGPURequestPipe
    {
        private const RenderTextureFormat TempRtFormat = RenderTextureFormat.ARGB32;

        private readonly Queue<GPURequest> m_VideoBuffer;
        private bool m_IsReceivingData;
        private bool m_IsVideoConsumerRunning;
        private readonly Allocator m_BufferAllocator;

        public event Func<MonoBehaviour> GetCoroutineManager;
        public event Action<NativeArray<byte>> OnFrameProduced;
        public bool IsFinished => !m_IsVideoConsumerRunning;

        private struct GPURequest
        {
            public NativeArray<byte> Data;
            public AsyncGPUReadbackRequest Request;
        }

        public ASyncGPURequestPipe(Allocator allocator)
        {
            m_BufferAllocator = allocator;
            m_VideoBuffer = new Queue<GPURequest>();
            m_IsReceivingData = false;
            m_IsVideoConsumerRunning = false;
        }

        public void Open()
        {
            if (m_IsReceivingData) return;
            m_IsReceivingData = true;
            if (GetCoroutineManager == null) throw new Exception("Null delegate: [GetCoroutineManager]");
            MonoBehaviour coroutine = GetCoroutineManager.Invoke();
            coroutine.StartCoroutine(VideoConsumer());
        }

        public void PushVideoFrame(RenderTexture texture)
        {
            if (m_VideoBuffer.Count > 4)
            {
                Debug.LogWarning("Too many frames");
                return;
            }

            if (m_IsReceivingData == false) return;
            RenderTexture temp = RenderTexture.GetTemporary(texture.width, texture.height, 0, TempRtFormat);
            Graphics.Blit(texture, temp);
            NativeArray<byte> buffer = new NativeArray<byte>(texture.width * texture.height * 4, m_BufferAllocator);
            AsyncGPUReadbackRequest request = AsyncGPUReadback.RequestIntoNativeArray(ref buffer, temp, 0, result =>
            {
                if (!result.hasError) return;
                Debug.LogError("Error occur when copy frame data from gpu");
            });
            m_VideoBuffer.Enqueue(new GPURequest
            {
                Data = buffer,
                Request = request
            });
            RenderTexture.ReleaseTemporary(temp);
        }

        public void Close() => m_IsReceivingData = false;

        private IEnumerator VideoConsumer()
        {
            m_IsVideoConsumerRunning = false;
            while (m_IsReceivingData || m_VideoBuffer.Count > 0)
                yield return ConsumeVideoBuffer();
            m_IsVideoConsumerRunning = true;
        }

        private IEnumerator ConsumeVideoBuffer()
        {
            if (m_VideoBuffer.Count <= 0) yield break;
            GPURequest peek = m_VideoBuffer.Dequeue();
            AsyncGPUReadbackRequest request = peek.Request;
            yield return new WaitUntil(() => request.done);
            OnFrameProduced?.Invoke(peek.Data);
            Debug.Log($"Produced one video frame with {peek.Data.Length} bytes from {nameof(ASyncGPURequestPipe)}.");
        }
    }
}