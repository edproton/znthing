
using Console.Entities;

namespace Console;

public static class Program
{
    public static void Main()
    {
        // Availability is defined in the user's local timezone
        var availabilityRange = new Availability
        {
            TimeRanges =
            [
                // Europe/Lisbon is 0 hours ahead of UTC (currently)
                // Friday, 1 of November 2024, 15:00 to 23:00
                
                // Asia/Shanghai is 8 hours ahead of Europe/Lisbon
                // Friday, 1 of November 2024, 23:00 to 23:59
                // Saturday, 2 of November 2024, 00:00 to 07:00
                new TimeRange
                {
                    StartTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 9, 0),
                    EndTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 12, 0),
                },
                new TimeRange
                {
                    StartTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 15, 0),
                    EndTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 23, 0),
                },
            ],
        };

        // Convert availability to UTC
        var availabilityInUtc = availabilityRange.ToUtc();

        // User requests availability for Saturday, 2 of November 2024 in Asia/Shanghai timezone
        // I want to know the availability for Saturday, 2 of November 2024 in Asia/Shanghai timezone
        // The availability timeranges are in UTC and can be picked partially or fully
        // The time ranges were set in Europe/Lisbon timezone and are the following (converted to UTC, is the same, because Europe/Lisbon is 0 hours ahead of UTC, but they are stored in UTC, availabilityInUtc):
        // Europe/Lisbon - Friday, 1 of November 2024, 15:00 to 23:00
        // Utc - 1 of November 2024, 15:00 to 23:00
        // Asia/Shanghai - 1 of November 2024, 23:00 to 7:00 (2nd of November 2024)
        
        // I want to know the availability for Saturday, 2 of November 2024 in Asia/Shanghai timezone
        // It is partially in the next day, so I want to pick the time range that is partially in the next day
        // The set availability is the following.
        // Friday, 1 of November 2024, 23:00 to 23:59
        // Saturday, 2 of November 2024, 00:00 to 07:00 (I want to pick this, is partially in the next day, not the full time range)
        var requestedDate = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 0, 0);
        foreach (var utcTimeRange in availabilityInUtc.TimeRanges)
        {
            // Generate hourly slots that overlap with the requested date in Shanghai timezone
            var hourlySlots = utcTimeRange.GenerateHourlySlotsForDate(requestedDate, TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon"));
            foreach (var slot in hourlySlots)
            {
                slot.Dump(); // Or Console.WriteLine(slot);
            }
        }
    }
    
    private static DateTimeOffset CreateDateTimeOffset(string timeZoneId, int year, int month, int day, int hour, int minute)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        return new DateTimeOffset(year, month, day, hour, minute, 0, timeZone.BaseUtcOffset);
    }
    
    private static void Dump(this object obj)
    {
        System.Console.WriteLine(obj);
    }
}