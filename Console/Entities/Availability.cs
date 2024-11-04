namespace Console.Entities;

public class Availability
{
    public List<TimeRange> TimeRanges { get; set; } = new();

    public Availability Localize(TimeZoneInfo targetTimeZoneInfo)
    {
        return new Availability
        {
            TimeRanges = TimeRanges.Select(tr => tr.Localize(targetTimeZoneInfo)).ToList()
        };
    }

    public Availability ToUtc() => Localize(TimeZoneInfo.Utc);
}