using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatmapManager
{
    class FormatString
    {
        public static string getFormattedTimeString(int time)
        {
            int minutes, seconds, milliseconds;
            minutes = time / 60000;
            time -= minutes * 60000;
            seconds = time / 1000;
            time -= seconds * 1000;
            milliseconds = time;
            return minutes.ToString("D2") + ":" + seconds.ToString("D2") + ":" + milliseconds.ToString("D3");
        }
    }
}
