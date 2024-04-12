using System;

namespace AStar.Recorder
{
    public interface IRecorder
    {
        public void Begin();
        public void Stop();
        public void Pause();
        public void Resume();
        public string OutputPath { get; set; }
        public event Action<string> OnEncoded;
    }
}