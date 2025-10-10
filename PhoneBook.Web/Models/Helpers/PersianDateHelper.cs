using MD.PersianDateTime.Standard;

namespace PhoneBook.Web.Models.Helpers
{
    public static class PersianDateHelper
    {
        public static DateTime? ParsePersianDate(string? persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            try
            {
                // Remove extra spaces
                persianDate = persianDate.Trim().Replace("  ", " ");

                // Convert Persian digits to English digits
                persianDate = ConvertPersianNumbersToEnglish(persianDate);

               // Parse the Persian date
                var pd = PersianDateTime.Parse(persianDate);
                return pd.ToDateTime();
            }
            catch
            {
                return null;
            }
        }

        public static string ToPersianDate(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return string.Empty;

            try
            {
                var pd = new PersianDateTime(dateTime.Value);
                return pd.ToString("yyyy/MM/dd");
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ToPersianDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return string.Empty;

            try
            {
                var pd = new PersianDateTime(dateTime.Value);
                return pd.ToString("yyyy/MM/dd HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ConvertPersianNumbersToEnglish(string input)
        {
            var persianDigits = new Dictionary<char, char>
            {
                {'۰', '0'}, {'۱', '1'}, {'۲', '2'}, {'۳', '3'}, {'۴', '4'},
                {'۵', '5'}, {'۶', '6'}, {'۷', '7'}, {'۸', '8'}, {'۹', '9'}
            };

            return string.Join("", input.Select(c =>
                persianDigits.ContainsKey(c) ? persianDigits[c] : c
            ));
        }

        public static (DateTime? from, DateTime? to) ParsePersianDateRange(string? fromDate, string? toDate)
        {
            return (ParsePersianDate(fromDate), ParsePersianDate(toDate));
        }
    }
}