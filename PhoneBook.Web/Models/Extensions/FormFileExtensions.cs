using Microsoft.AspNetCore.Http;

namespace PhoneBook.Web.Models.Extensions
{
    public static class FormFileExtensions
    {
        public static bool IsImage(this IFormFile file)
        {
            if (file == null) return false;

            var allowedContentTypes = new[] 
            { 
                "image/jpeg", 
                "image/jpg", 
                "image/png", 
                "image/gif" 
            };

            return allowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
        }

        public static bool IsValidSize(this IFormFile file, long maxSize = 5242880) // 5MB default
        {
            if (file == null) return false;
            return file.Length <= maxSize && file.Length > 0;
        }

        public static async Task<byte[]> GetBytesAsync(this IFormFile file)
        {
            if (file == null) return Array.Empty<byte>();

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public static string GetFileExtension(this IFormFile file)
        {
            if (file == null) return string.Empty;
            return Path.GetExtension(file.FileName).ToLowerInvariant();
        }

        public static bool HasValidImageExtension(this IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = file.GetFileExtension();
            
            return allowedExtensions.Contains(extension);
        }
    }
}