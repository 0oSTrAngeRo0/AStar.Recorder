using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace AStar.Recorder
{
    public class FFmpegPipe
    {
        private Process m_Process;
        private readonly ConcurrentQueue<NativeArray<byte>> m_Data;
        private bool m_IsReceivingData;
        private Thread m_DataConsumer;
        private bool m_IsExited;
        private FFmpegLogger m_Logger;
        public bool IsExited => m_IsExited;

        /// <summary>
        /// Don't call unity api in this events!!!
        /// </summary>
        public event Action<NativeArray<byte>> OnDataConsumed;

        public FFmpegPipe()
        {
            m_Data = new ConcurrentQueue<NativeArray<byte>>();
            m_IsReceivingData = true;
            m_IsExited = true;
        }

        public void Open(string arguments)
        {
            m_IsReceivingData = true;
            m_IsExited = false;

            m_Process = FFmpegUtils.CreateProcess(FFmpegUtils.FFmpegPath, arguments, true);
            m_Logger = new FFmpegLogger(m_Process);
            FFmpegUtils.StartProcess(m_Process);

            m_DataConsumer = new Thread(DataConsumer);
            m_DataConsumer.Start();
        }

        public void PushData(NativeArray<byte> data)
        {
            if (m_IsReceivingData == false)
            {
                Debug.LogWarning("Pushing data while cannot receive data! Dispose it!");
                OnDataConsumed?.Invoke(data);
                return;
            }

            m_Data.Enqueue(data);
        }

        private void DataConsumer()
        {
            while (m_IsReceivingData || !m_Data.IsEmpty)
            {
                if (!m_Data.TryDequeue(out NativeArray<byte> peek))
                    continue; // maybe sleep for a while is a better choice
                Stream stream = m_Process.StandardInput.BaseStream;
                try
                {
                    stream.Write(peek.AsReadOnlySpan());
                    stream.Flush();
                    OnDataConsumed?.Invoke(peek);
                    // Debug.Log($"Consume a frame, {m_Data.Count} frames rest");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    m_IsReceivingData = false;
                }
            }
        }

        public void Close()
        {
            m_IsReceivingData = false;

            Thread thread = new Thread(CloseInternal);
            thread.Start();
            return;

            void CloseInternal()
            {
                m_DataConsumer.Join();

                // Debug.Log(m_Data.Count);
                m_Process.StandardInput.Flush();
                m_Process.StandardInput.Close();
                m_Process.WaitForExit();
                m_Process.Close();
                m_Process.Dispose();

                Debug.Log(m_Logger.PopLog());

                m_IsExited = true;
            }
        }
    }
}