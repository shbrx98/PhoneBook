namespace PhoneBook.Web.Models.Extensions
{
    public static class StringExtensions
    {
        public static string ToPersianNumbers(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var persianDigits = new[] { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
            
            return string.Join("", input.Select(c =>
                char.IsDigit(c) ? persianDigits[int.Parse(c.ToString())] : c
            ));
        }

        public static string ToEnglishNumbers(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var persianDigits = new Dictionary<char, char>
            {
                {'۰', '0'}, {'۱', '1'}, {'۲', '2'}, {'۳', '3'}, {'۴', '4'},
                {'۵', '5'}, {'۶', '6'}, {'۷', '7'}, {'۸', '8'}, {'۹', '9'}
            };

            return string.Join("", input.Select(c =>
                persianDigits.ContainsKey(c) ? persianDigits[c] : c
            ));
        }

        public static string FormatMobileNumber(this string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber) || mobileNumber.Length != 11)
                return mobileNumber;

            // 09123456789 -> 0912 345 6789
            return $"{mobileNumber.Substring(0, 4)} {mobileNumber.Substring(4, 3)} {mobileNumber.Substring(7)}";
        }

        public static bool IsValidMobileNumber(this string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber)) return false;
            
            mobileNumber = mobileNumber.ToEnglishNumbers().Trim();
            
            return mobileNumber.Length == 11 && 
                   mobileNumber.StartsWith("09") && 
                   mobileNumber.All(char.IsDigit);
        }
    }
}
