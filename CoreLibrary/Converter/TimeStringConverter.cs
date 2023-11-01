namespace CoreLibrary.Converter
{

    /// <summary>
    /// Provides conversion functions between string formatted time and the total number of seconds.
    /// Actually there is simpler approach using TimeSpan.ToString() and TimeSpan.TotalSeconds 
    /// </summary>
    public static class TimeStringConverter
    {
        /// <summary>
        /// Converts a "mm:ss" or "hh:mm:ss" formatted 
        /// timespan to numeric number of seconds
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int TimeStringToSeconds(string str) 
        {
            string[] words = str.Split(':');
            int sec = 0;

            if (words.Length == 1) 
            {
                bool success = int.TryParse(words[0], out int res);
                if (success)
                    sec = res;
                else
                    sec = 0;
            }
            else if (words.Length == 2)
            {
                sec = 60 * int.Parse(words[0]) + int.Parse(words[1]);
            }
            else if (words.Length == 3)
            {
                sec = 3600 * int.Parse(words[0]) + 60 * int.Parse(words[1]) + int.Parse(words[2]);
            }

            return sec;
        }

        /// <summary>
        /// Converts a numveric number of seconds into a formatted time string
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string SecondsToTimeString(int seconds) 
        {
            TimeSpan span = new TimeSpan(0,0,seconds);
            return span.ToString();
        }    
    }
}
