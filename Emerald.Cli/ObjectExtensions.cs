using System;

namespace Emerald.Cli
{
    public static class ObjectExtensions
    {
        public static T Tap<T>(this T source, Action<T> block)
        {
            block(source);
            return source;
        }
    }
}