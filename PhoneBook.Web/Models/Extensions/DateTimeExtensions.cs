using MD.PersianDateTime.Standard;

namespace PhoneBook.Web.Models.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToPersianDateString(this DateTime dateTime)
        {
            var persianDate = new PersianDateTime(dateTime);
            return persianDate.ToString("yyyy/MM/dd");
        }

        public static string ToPersianDateTimeString(this DateTime dateTime)
        {
            var persianDate = new PersianDateTime(dateTime);
            return persianDate.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static string? ToPersianDateString(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return dateTime.Value.ToPersianDateString();
        }

        public static DateTime? ParsePersianDate(string? persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate)) return null;
            
            try
            {
                return PersianDateTime.Parse(persianDate).ToDateTime();
            }
            catch
            {
                return null;
            }
        }
    }
}
