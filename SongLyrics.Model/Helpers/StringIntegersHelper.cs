using System;

namespace SongLyrics.Model.Helpers
{
    public static class StringIntegersHelper
    {
        public static bool IsIntegerWithinRange(string s, int min = 1, int max = 100)
        {
            int i;
            return Int32.TryParse(s, out i) && i >= min && i <= max;
        }
    }
}