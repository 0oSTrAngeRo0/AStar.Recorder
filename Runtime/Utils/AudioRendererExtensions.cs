using System;
using UnityEngine;

namespace AStar.Recorder
{
    public static class AudioRendererExtensions
    {
        public static uint GetChannelCount(this AudioSpeakerMode mode) => mode switch
        {
            AudioSpeakerMode.Mono => 1,
            AudioSpeakerMode.Stereo => 2,
            AudioSpeakerMode.Quad => 4,
            AudioSpeakerMode.Surround => 5,
            AudioSpeakerMode.Mode5point1 => 5,
            AudioSpeakerMode.Mode7point1 => 7,
            AudioSpeakerMode.Prologic => 2,
            _ => 1
        };
    }
}