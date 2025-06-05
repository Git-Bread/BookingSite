using System;

namespace BookingSite.Services
{
    public interface IDayMappingService
    {
        int MapDayOfWeek(DayOfWeek dayOfWeek);
        string GetDayName(int dayNumber);
        DayOfWeek MapToDayOfWeek(int dayNumber);
    }

    // For mapping the days of the week used in the project, probaly extremely excessive 
    public class DayMappingService : IDayMappingService
    {
        public int MapDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 3,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 5,
                DayOfWeek.Friday => 6,
                DayOfWeek.Saturday => 7,
                _ => throw new ArgumentException($"Unexpected day of week: {dayOfWeek}")
            };
        }

        public string GetDayName(int dayNumber)
        {
            return dayNumber switch
            {
                1 => "Sunday",
                2 => "Monday",
                3 => "Tuesday",
                4 => "Wednesday",
                5 => "Thursday",
                6 => "Friday",
                7 => "Saturday",
                _ => throw new ArgumentException($"Invalid day number: {dayNumber}")
            };
        }

        public DayOfWeek MapToDayOfWeek(int dayNumber)
        {
            return dayNumber switch
            {
                1 => DayOfWeek.Sunday,
                2 => DayOfWeek.Monday,
                3 => DayOfWeek.Tuesday,
                4 => DayOfWeek.Wednesday,
                5 => DayOfWeek.Thursday,
                6 => DayOfWeek.Friday,
                7 => DayOfWeek.Saturday,
                _ => throw new ArgumentException($"Invalid day number: {dayNumber}")
            };
        }
    }
} 