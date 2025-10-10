using Microsoft.AspNetCore.Http;

namespace PhoneBook.Web.Models.Helpers
{
    public static class ImageHelper
    {
        public static async Task<(bool isValid, string? errorMessage, byte[]? imageData)> ValidateAndProcessImageAsync(IFormFile? image)
        {
            if (image == null)
                return (true, null, null);

            // Check file size
            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (image.Length > maxSize)
                return (false, "حجم تصویر نباید بیشتر از 5 مگابایت باشد", null);

            if (image.Length == 0)
                return (false, "فایل تصویر خالی است", null);

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return (false, "فقط فایل‌های تصویری (jpg, jpeg, png, gif) مجاز هستند", null);

            // Check content type
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedContentTypes.Contains(image.ContentType.ToLowerInvariant()))
                return (false, "نوع فایل نامعتبر است", null);

            // Read image data
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var imageData = memoryStream.ToArray();

            return (true, null, imageData);
        }

        public static string GetImageDataUrl(byte[] imageData, string contentType)
        {
            if (imageData == null || imageData.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(imageData);
            return $"data:{contentType};base64,{base64}";
        }
    }
}
