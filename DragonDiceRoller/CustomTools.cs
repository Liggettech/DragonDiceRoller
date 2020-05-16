using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

namespace CustomTools
{
    public static class ListExtension
    {
        public static string PrintWithDelimiter<T>(this List<T> list, char cDelimiter)
        {
            string sListWithDelimiter = "";

            for (int i = 0; i < list.Count; i++)
            {
                sListWithDelimiter += list[i].ToString();

                if (i < list.Count - 1)
                {
                    sListWithDelimiter += cDelimiter + " ";
                }
            }

            return sListWithDelimiter;
        }
    }

    public static class ColorExtension
    {
        public static bool Equals(this Color color1, Color color2)
        {
            if (color1.RawValue == color2.RawValue)
                return true;

            else
                return false;
        }
    }
}