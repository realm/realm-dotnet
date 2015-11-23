using System;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class DateTimeOffsetExtensions
{
    private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    internal static long ToUnixTimeSeconds(this DateTimeOffset @this)
    {
        return Convert.ToInt64((@this.ToUniversalTime() - UnixEpoch).TotalSeconds);
    }

    internal static DateTimeOffset FromUnixTimeSeconds(long seconds)
    {
        return UnixEpoch.AddSeconds(seconds);
    }
}

