namespace API.Config
{
    public static class TimeSpanExtensions
    {
        public static string ToHumanReadableString(this TimeSpan ts)
        {
            var parts = new List<string>();

            if (ts.Days > 0)
            {
                parts.Add($"{ts.Days} day{(ts.Days != 1 ? "s" : "")}");
                if (ts.Hours > 0) parts.Add($"{ts.Hours} hour{(ts.Hours != 1 ? "s" : "")}");
                if (ts.Minutes > 0) parts.Add($"{ts.Minutes} minute{(ts.Minutes != 1 ? "s" : "")}");
            }
            else if (ts.Hours > 0)
            {
                parts.Add($"{ts.Hours} hour{(ts.Hours != 1 ? "s" : "")}");
                if (ts.Minutes > 0) parts.Add($"{ts.Minutes} minute{(ts.Minutes != 1 ? "s" : "")}");
                if (ts.Seconds > 0) parts.Add($"{ts.Seconds} second{(ts.Seconds != 1 ? "s" : "")}");
            }
            else if (ts.Minutes > 0)
            {
                parts.Add($"{ts.Minutes} minute{(ts.Minutes != 1 ? "s" : "")}");
                if (ts.Seconds > 0) parts.Add($"{ts.Seconds} second{(ts.Seconds != 1 ? "s" : "")}");
                if (ts.Milliseconds > 0) parts.Add($"{ts.Milliseconds} ms");
            }
            else if (ts.Seconds > 0)
            {
                parts.Add($"{ts.Seconds} second{(ts.Seconds != 1 ? "s" : "")}");
                if (ts.Milliseconds > 0) parts.Add($"{ts.Milliseconds} ms");
            }
            else
            {
                parts.Add($"{ts.Milliseconds} ms");
            }

            return string.Join(", ", parts.Take(3));
        }
    }

}
