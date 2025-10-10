namespace PhoneBook.Web.Models.Constants
{
    public static class AppSettings
    {
        public const string DefaultConnectionString = "DefaultConnection";
        
        public static class ImageSettings
        {
            public const long MaxImageSize = 5 * 1024 * 1024; // 5MB
            public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            public static readonly string[] AllowedContentTypes = 
            { 
                "image/jpeg", 
                "image/jpg", 
                "image/png", 
                "image/gif" 
            };
        }

        public static class MobileNumberSettings
        {
            public const int Length = 11;
            public const string Pattern = @"^09\d{9}$";
            public const string Prefix = "09";
        }

        public static class ValidationSettings
        {
            public const int MaxNameLength = 100;
            public const int MinNameLength = 2;
        }
    }
}