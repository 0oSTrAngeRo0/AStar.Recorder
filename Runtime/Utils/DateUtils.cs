using System;

namespace AStar.Recorder
{
    public static class DateUtils
    {
        public static string GetFileString(this DateTime date) => date.ToString("yyyy-MM-dd--hh-mm-ss-ffff");
    }
}