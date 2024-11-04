using Console.Entities;

namespace UnitTests;

public class UnitTest1
{
    private static DateTimeOffset CreateDateTimeOffset(string timeZoneId, int year, int month, int day, int hour, int minute)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        return new DateTimeOffset(year, month, day, hour, minute, 0, timeZone.BaseUtcOffset);
    }

    [Fact]
    public void LisbonToShangai_PartiallySelect_NextDay()
    {
        var availabilityRange = new Availability
        {
            TimeRanges =
            [
                new TimeRange
                {
                    StartTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 15, 0),
                    EndTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 23, 0),
                }
            ],
        };

        var availabilityInUtc = availabilityRange.ToUtc();

        var requestedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
        var requestedDate = CreateDateTimeOffset(requestedTimeZone.Id, 2024, 11, 2, 0, 0);

        var expectedTimeSlots = new List<TimeRange>
        {
            new() { StartTime = requestedDate.AddHours(0), EndTime = requestedDate.AddHours(1) },
            new() { StartTime = requestedDate.AddHours(1), EndTime = requestedDate.AddHours(2) },
            new() { StartTime = requestedDate.AddHours(2), EndTime = requestedDate.AddHours(3) },
            new() { StartTime = requestedDate.AddHours(3), EndTime = requestedDate.AddHours(4) },
            new() { StartTime = requestedDate.AddHours(4), EndTime = requestedDate.AddHours(5) },
            new() { StartTime = requestedDate.AddHours(5), EndTime = requestedDate.AddHours(6) },
            new() { StartTime = requestedDate.AddHours(6), EndTime = requestedDate.AddHours(7) },
        };

        foreach (var utcTimeRange in availabilityInUtc.TimeRanges)
        {
            var hourlySlots = utcTimeRange.GenerateHourlySlotsForDate(requestedDate, requestedTimeZone);
            foreach (var slot in hourlySlots)
            {
                Assert.Contains(expectedTimeSlots, x => 
                    x.StartTime == slot.StartTime 
                    && x.EndTime == slot.EndTime);
            }
            
            Assert.Equal(expectedTimeSlots.Count, hourlySlots.Count);
        }
    }
    
    [Fact]
    public void LisbonToShangai_PartiallySelect_SameDay()
    {
        var availabilityRange = new Availability
        {
            TimeRanges =
            [
                new TimeRange
                {
                    StartTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 15, 0),
                    EndTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 23, 0),
                }
            ],
        };

        var availabilityInUtc = availabilityRange.ToUtc();

        var requestedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
        // Friday, 1 of November 2024, 23:00 to 23:59 in Asia/Shanghai
        var requestedDate = CreateDateTimeOffset(requestedTimeZone.Id, 2024, 11, 1, 0, 0);

        // Friday, 1 of November 2024, 23:00 to 23:59
        // It grabs the same day, because it is partially in the same day
        // 15:00 is 23:00 in Shanghai and 15:59 is 23:59 in Shanghai
        // It needs to this 59 automatically, (GenerateHourlySlotsForDate)
        var expectedTimeSlots = new List<TimeRange>
        {
            new() { StartTime = requestedDate.AddHours(23), EndTime = requestedDate.AddHours(23).AddMinutes(59) },
        };

        foreach (var utcTimeRange in availabilityInUtc.TimeRanges)
        {
            var hourlySlots = utcTimeRange.GenerateHourlySlotsForDate(requestedDate, requestedTimeZone);
            foreach (var slot in hourlySlots)
            {
                Assert.Contains(expectedTimeSlots, x => 
                    x.StartTime == slot.StartTime 
                    && x.EndTime == slot.EndTime);
            }
            
            Assert.Equal(expectedTimeSlots.Count, hourlySlots.Count);
        }
    }
    
    [Fact]
    public void Lisbon_AllTimeSlotsInOneDay()
    {
        // Define an availability range from 00:00 to 23:59 in Europe/Lisbon timezone
        var availabilityRange = new Availability
        {
            TimeRanges =
            [
                new TimeRange
                {
                    StartTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 0, 0),
                    EndTime = CreateDateTimeOffset("Europe/Lisbon", 2024, 11, 1, 23, 59),
                }
            ],
        };

        // Convert availability to UTC for internal storage
        var availabilityInUtc = availabilityRange.ToUtc();

        // Define the requested timezone as Europe/Lisbon and date as 1st November 2024
        var requestedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");
        var requestedDate = CreateDateTimeOffset(requestedTimeZone.Id, 2024, 11, 1, 0, 0);

        // Generate the expected list of hourly time slots
        var expectedTimeSlots = new List<TimeRange>();
        for (int hour = 0; hour < 23; hour++)
        {
            expectedTimeSlots.Add(new TimeRange
            {
                StartTime = requestedDate.AddHours(hour),
                EndTime = requestedDate.AddHours(hour + 1)
            });
        }
        // Add the final slot from 23:00 to 23:59
        expectedTimeSlots.Add(new TimeRange
        {
            StartTime = requestedDate.AddHours(23),
            EndTime = requestedDate.AddHours(23).AddMinutes(59)
        });

        // Run the slot generation for the availability range in UTC and compare with expected results
        foreach (var utcTimeRange in availabilityInUtc.TimeRanges)
        {
            var hourlySlots = utcTimeRange.GenerateHourlySlotsForDate(requestedDate, requestedTimeZone);
            
            // Check if each generated slot matches the expected slots
            foreach (var slot in hourlySlots)
            {
                Assert.Contains(expectedTimeSlots, x => 
                    x.StartTime == slot.StartTime && 
                    x.EndTime == slot.EndTime);
            }
            
            // Ensure the number of generated slots matches the expected count
            Assert.Equal(expectedTimeSlots.Count, hourlySlots.Count);
        }
    }

    [Fact]
    public void ApiaSamoa_Anchorange()
    {
        var availabilityRange = new Availability
        {
            TimeRanges =
            [
                new TimeRange
                {
                    StartTime = CreateDateTimeOffset("Pacific/Apia", 2024, 11, 4, 9, 0),
                    EndTime = CreateDateTimeOffset("Pacific/Apia", 2024, 11, 4, 17, 0),
                }
            ],
        };

        var availabilityInUtc = availabilityRange.ToUtc();

        var requestedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Anchorage");
        var requestedDate = CreateDateTimeOffset(requestedTimeZone.Id, 2024, 11, 4, 0, 0);

        var expectedTimeSlots = new List<TimeRange>
        {
            new() { StartTime = requestedDate.AddHours(12), EndTime = requestedDate.AddHours(13) },
            new() { StartTime = requestedDate.AddHours(13), EndTime = requestedDate.AddHours(14) },
            new() { StartTime = requestedDate.AddHours(14), EndTime = requestedDate.AddHours(15) },
            new() { StartTime = requestedDate.AddHours(15), EndTime = requestedDate.AddHours(16) },
            new() { StartTime = requestedDate.AddHours(16), EndTime = requestedDate.AddHours(17) },
            new() { StartTime = requestedDate.AddHours(17), EndTime = requestedDate.AddHours(18) },
            new() { StartTime = requestedDate.AddHours(18), EndTime = requestedDate.AddHours(19) },
            new() { StartTime = requestedDate.AddHours(19), EndTime = requestedDate.AddHours(20) },
        };

        foreach (var utcTimeRange in availabilityInUtc.TimeRanges)
        {
            var hourlySlots = utcTimeRange.GenerateHourlySlotsForDate(requestedDate, requestedTimeZone);
            foreach (var slot in hourlySlots)
            {
                Assert.Contains(expectedTimeSlots, x => 
                    x.StartTime == slot.StartTime 
                    && x.EndTime == slot.EndTime);
            }
            
            Assert.Equal(expectedTimeSlots.Count, hourlySlots.Count);
        }
    }
}