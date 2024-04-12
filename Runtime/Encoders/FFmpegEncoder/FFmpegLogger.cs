using System.Diagnostics;
using System.Text;

namespace AStar.Recorder
{
    public class FFmpegLogger
    {
        private readonly StringBuilder m_Builder;
        public FFmpegLogger(Process process)
        {
            m_Builder = new StringBuilder();
            m_Builder.Append(process.StartInfo.FileName);
            m_Builder.Append(' ');
            m_Builder.Append(process.StartInfo.Arguments);
            m_Builder.Append('\n');
            if (process.EnableRaisingEvents)
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    m_Builder.Append(e.Data);
                    m_Builder.Append('\n');
                }; 
            }
        }

        public string PopLog()
        {
            string log = m_Builder.ToString();
            m_Builder.Clear();
            return log;
        }
    }
}