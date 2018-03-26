using System;

namespace bkbot.Core
{
    public static class Extensions
    {

        public static int Height(this String str)
        {
            int height = (str.Length > 0 ? 1 : 0);
            for (int i = 0; i < str.Length; i++)
                if (str[i] == '\n' || str[i] == '\r')
                    height++;
            return height;
        }

        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;

            return value;
        }

    }

}
