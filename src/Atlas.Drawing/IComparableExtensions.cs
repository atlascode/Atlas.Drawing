using System;

namespace Atlas.Drawing
{
    internal static class IComparableExtensions
    {
        public static T Clamp<T>(this T source, T min, T max) where T : IComparable<T>
        {
            if (source.CompareTo(min) < 0)
                return min;
            else if (source.CompareTo(max) > 0)
                return max;
            else
                return source;
        }
    }
}
