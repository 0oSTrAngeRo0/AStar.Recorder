using System;
using System.Text;
using UnityEngine;

namespace AStar.Recorder
{
    public enum FFmpegLogLevel
    {
        Quiet,
        Panic,
        Fatal,
        Error,
        Warning,
        Info,
        Verbose,
        Debug,
        Trace,
    }

    [Serializable]
    public struct FFmpegConfig
    {
        public string OutputPath;
        public VideoConfig VideoConfig;
        public AudioConfig AudioConfig;
    }

    [Serializable]
    public struct VideoConfig
    {
        public Vector2Int Resolution;
        public string FilePath;
        public int Fps;
        public FFmpegLogLevel LogLevel;
    }

    [Serializable]
    public struct AudioConfig
    {
        public uint SampleRate;
        public uint Channels;
        public string FilePath;
        public FFmpegLogLevel LogLevel;
    }

    public static class FFmpegConfigExtensions
    {
        private static StringBuilder M_BUILDER = new StringBuilder();

        public static string GetVideoArguments(this VideoConfig config)
        {
            M_BUILDER.Clear();
            M_BUILDER.Append("-y -f rawvideo -vcodec rawvideo -pixel_format rgba -colorspace bt709 -video_size ");
            M_BUILDER.Append(config.Resolution.x);
            M_BUILDER.Append("x");
            M_BUILDER.Append(config.Resolution.y);
            M_BUILDER.Append(" -framerate ");
            M_BUILDER.Append(config.Fps);
            M_BUILDER.Append(" -loglevel ");
            M_BUILDER.Append(config.LogLevel.GetFFmpegLogLevelArguments());
            M_BUILDER.Append(" -i - -pix_fmt yuv420p -vf vflip \"");
            M_BUILDER.Append(config.FilePath);
            M_BUILDER.Append("\"");

            string arguments = M_BUILDER.ToString();
            M_BUILDER.Clear();
            return arguments;
        }

        public static string GetAudioArguments(this AudioConfig config)
        {
            string codec = BitConverter.IsLittleEndian ? "f32le" : "f32be";
            M_BUILDER.Clear();
            M_BUILDER.Append("-y -f ");
            M_BUILDER.Append(codec);
            M_BUILDER.Append(" -ar ");
            M_BUILDER.Append(config.SampleRate);
            M_BUILDER.Append(" -ac ");
            M_BUILDER.Append(config.Channels);
            M_BUILDER.Append(" -loglevel ");
            M_BUILDER.Append(config.LogLevel.GetFFmpegLogLevelArguments());
            M_BUILDER.Append(" -i - -c:a pcm_");
            M_BUILDER.Append(codec);
            M_BUILDER.Append(" \"");
            M_BUILDER.Append(config.FilePath);
            M_BUILDER.Append("\"");

            string arguments = M_BUILDER.ToString();
            M_BUILDER.Clear();
            return arguments;
        }

        public static string GetFFmpegLogLevelArguments(this FFmpegLogLevel level) => level switch
        {
            FFmpegLogLevel.Quiet => "quiet",
            FFmpegLogLevel.Panic => "panic",
            FFmpegLogLevel.Fatal => "fatal",
            FFmpegLogLevel.Error => "error",
            FFmpegLogLevel.Warning => "warning",
            FFmpegLogLevel.Info => "info",
            FFmpegLogLevel.Verbose => "verbose",
            FFmpegLogLevel.Debug => "debug",
            FFmpegLogLevel.Trace => "trace",
            _ => "info"
        };

        public static string GetMergeArguments(this FFmpegConfig config)
        {
            M_BUILDER.Clear();
            M_BUILDER.Append("-y -i ");
            M_BUILDER.Append(config.VideoConfig.FilePath);
            M_BUILDER.Append(" -i ");
            M_BUILDER.Append(config.AudioConfig.FilePath);
            M_BUILDER.Append(" -c:v copy -c:a aac ");
            M_BUILDER.Append(config.OutputPath);
            string arguments = M_BUILDER.ToString();
            M_BUILDER.Clear();
            return arguments;
        }
    }
}