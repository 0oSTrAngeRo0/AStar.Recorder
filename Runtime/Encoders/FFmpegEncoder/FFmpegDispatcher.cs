using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AStar.Recorder
{
    public class FFmpegDispatcher
    {
        private Process m_Process;
        private FFmpegLogger m_Logger;
        public bool IsExited => m_Process == null || m_Process.HasExited;
        
        public FFmpegDispatcher(string filename, string arguments)
        {
            m_Process = FFmpegUtils.CreateProcess(filename, arguments, false);
            m_Logger = new FFmpegLogger(m_Process);
            m_Process.Exited += (s, e) => Debug.Log(m_Logger.PopLog());
            FFmpegUtils.StartProcess(m_Process);
        }
    }
}