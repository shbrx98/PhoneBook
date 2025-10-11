namespace PhoneBook.Web.Models.Constants
{
    public static class ValidationMessages
    {
        public const string RequiredField = "این فیلد الزامی است";
        public const string InvalidMobileNumber = "شماره موبایل معتبر نیست (باید 11 رقم و با 09 شروع شود)";
        public const string DuplicateMobileNumber = "این شماره موبایل قبلاً ثبت شده است";
        public const string InvalidImageFormat = "فقط فایل‌های تصویری (jpg, jpeg, png, gif) مجاز هستند";
        public const string ImageSizeExceeded = "حجم تصویر نباید بیشتر از 5 مگابایت باشد";
        public const string InvalidBirthDate = "تاریخ تولد نمی‌تواند در آینده باشد";
        public const string MaxLengthExceeded = "طول فیلد از حد مجاز بیشتر است";
    }

    public static class SuccessMessages
    {
        public const string ContactCreated = "مخاطب با موفقیت ایجاد شد";
        public const string ContactUpdated = "مخاطب با موفقیت ویرایش شد";
        public const string ContactDeleted = "مخاطب با موفقیت حذف شد";
        public const string OperationSuccessful = "عملیات با موفقیت انجام شد";
    }

    public static class ErrorMessages
    {
        public const string ContactNotFound = "مخاطب مورد نظر یافت نشد";
        public const string UnexpectedError = "خطای غیرمنتظره‌ای رخ داده است";
        public const string DatabaseError = "خطا در ارتباط با پایگاه داده";
        public const string ImageUploadError = "خطا در آپلود تصویر";
    }
}