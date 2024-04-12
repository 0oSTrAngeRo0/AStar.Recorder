using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace AStar.Recorder
{
    public static class FFmpegUtils
    {
        internal static string FFmpegPath =>
            Path.Combine(Application.streamingAssetsPath, "FFmpegOut", "Windows", "ffmpeg.exe");
        
        internal static Process CreateProcess(string filename, string arguments, bool hasInput) => new Process
        {
            EnableRaisingEvents = true,
            StartInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                CreateNoWindow = true,
                FileName = filename,
                RedirectStandardError = true,
                RedirectStandardInput = hasInput,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
            },
        };

        internal static void StartProcess(Process process)
        {
            if (process == null) return;
            process.Start();
            process.BeginErrorReadLine();
        }
    }
}