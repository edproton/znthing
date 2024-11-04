namespace Console.Entities;
public class TimeRange
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public TimeRange Localize(TimeZoneInfo targetTimeZoneInfo)
    {
        return new TimeRange
        {
            StartTime = TimeZoneInfo.ConvertTime(StartTime, targetTimeZoneInfo),
            EndTime = TimeZoneInfo.ConvertTime(EndTime, targetTimeZoneInfo),
        };
    }

   public List<TimeRange> GenerateHourlySlotsForDate(DateTimeOffset requestedDate, TimeZoneInfo targetTimeZone)
    {
        // Convert the original StartTime and EndTime to the target timezone
        var localizedStartTime = TimeZoneInfo.ConvertTime(StartTime, targetTimeZone);
        var localizedEndTime = TimeZoneInfo.ConvertTime(EndTime, targetTimeZone);

        // Define the start and end of the requested date in the target timezone
        var dayStart = new DateTimeOffset(requestedDate.Year, requestedDate.Month, requestedDate.Day, 0, 0, 0, targetTimeZone.BaseUtcOffset);
        var dayEnd = dayStart.AddDays(1);

        var hourlySlots = new List<TimeRange>();

        // Determine if localizedStartTime is after the start of the requested day
        var isLocalizedStartAfterDayStart = localizedStartTime > dayStart;

        // Set the initial start time based on whether localizedStartTime or dayStart is later
        DateTimeOffset currentStart;
        if (isLocalizedStartAfterDayStart)
        {
            currentStart = localizedStartTime;
        }
        else
        {
            currentStart = dayStart;
        }

        // Determine if localizedEndTime is before the end of the requested day
        var isLocalizedEndBeforeDayEnd = localizedEndTime < dayEnd;

        // Set the initial end time based on whether localizedEndTime or dayEnd is earlier
        DateTimeOffset currentEnd;
        if (isLocalizedEndBeforeDayEnd)
        {
            currentEnd = localizedEndTime;
        }
        else
        {
            currentEnd = dayEnd;
        }

        // Loop to generate hourly slots as long as there is remaining time in the day
        var hasRemainingTime = currentStart < currentEnd;
        while (hasRemainingTime)
        {
            // Calculate the potential end of the next hourly slot
            var nextHour = currentStart.AddHours(1);

            // Determine if the next hour is within the currentEnd boundary
            var isWithinEnd = nextHour <= currentEnd;

            // Set the slot end based on whether it should be nextHour or currentEnd
            DateTimeOffset slotEnd;
            if (isWithinEnd)
            {
                slotEnd = nextHour;
            }
            else
            {
                slotEnd = currentEnd;
            }

            // Check if the slot end time crosses into the next day at midnight (00:00)
            var crossesIntoNextDay = slotEnd is { Hour: 0, Minute: 0 } && slotEnd.Day == currentStart.Day + 1;
            if (crossesIntoNextDay)
            {
                // Adjust slotEnd to 23:59 of the current day to avoid crossing into the next day
                slotEnd = new DateTimeOffset(slotEnd.Year, slotEnd.Month, slotEnd.Day - 1, 23, 59, 0, slotEnd.Offset);

                // Add the final adjusted slot and exit early
                hourlySlots.Add(new TimeRange { StartTime = currentStart, EndTime = slotEnd });
                return hourlySlots;
            }

            // Add the current slot to the list
            hourlySlots.Add(new TimeRange { StartTime = currentStart, EndTime = slotEnd });

            // Move to the next start time for the next iteration
            currentStart = slotEnd;

            // Update loop condition to check if there is more remaining time
            hasRemainingTime = currentStart < currentEnd;
        }

        return hourlySlots;
    }

    public override string ToString()
    {
        return $"[{StartTime.DayOfWeek}] Start: {StartTime}, [{EndTime.DayOfWeek}] End: {EndTime}";
    }
}