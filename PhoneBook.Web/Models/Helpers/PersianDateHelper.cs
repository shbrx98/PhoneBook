using System.Globalization;
namespace PhoneBook.Web.Helpers
{
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar _persianCalendar = new PersianCalendar();

        /// <summary>
        /// CONVERT TO PERSIAN DATE (FORMAT: 1402/05/15)
        /// </summary>
        public static string ToPersianDate(this DateTime date)
        {
            int year = _persianCalendar.GetYear(date);
            int month = _persianCalendar.GetMonth(date);
            int day = _persianCalendar.GetDayOfMonth(date);

            return $"{year:0000}/{month:00}/{day:00}";
        }

        /// <summary>
        /// Convert to Persian date (format: 1402/05/15)
        /// </summary>
        public static string ToPersianDateLong(this DateTime date)
        {
            int year = _persianCalendar.GetYear(date);
            int month = _persianCalendar.GetMonth(date);
            int day = _persianCalendar.GetDayOfMonth(date);

            string monthName = GetPersianMonthName(month);
            string dayName = GetPersianDayOfWeek(date.DayOfWeek);

            return $"{dayName} {day} {monthName} {year}";
        }

        /// <summary>
        /// Name of the Persian month
        /// </summary>
        private static string GetPersianMonthName(int month)
        {
            return month switch
            {
                1 => "فروردین",
                2 => "اردیبهشت",
                3 => "خرداد",
                4 => "تیر",
                5 => "مرداد",
                6 => "شهریور",
                7 => "مهر",
                8 => "آبان",
                9 => "آذر",
                10 => "دی",
                11 => "بهمن",
                12 => "اسفند",
                _ => ""
            };
        }

        /// <summary>
        /// Name of the Persian day of the week
        /// </summary>
        private static string GetPersianDayOfWeek(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Saturday => "شنبه",
                DayOfWeek.Sunday => "یکشنبه",
                DayOfWeek.Monday => "دوشنبه",
                DayOfWeek.Tuesday => "سه‌شنبه",
                DayOfWeek.Wednesday => "چهارشنبه",
                DayOfWeek.Thursday => "پنج‌شنبه",
                DayOfWeek.Friday => "جمعه",
                _ => ""
            };
        }

        /// <summary>
        /// Convert Persian date (format: 1402/05/15) to Gregorian date
        /// </summary>
        public static DateTime? ToGregorianDate(string persianDate)
        {
            try
            {
                var parts = persianDate.Split('/');
                if (parts.Length != 3)
                    return null;

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                return _persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Validate Persian date (format: 1402/05/15)
        /// </summary>
        public static bool IsValidPersianDate(string persianDate)
        {
            return ToGregorianDate(persianDate).HasValue;
        }
    }
}