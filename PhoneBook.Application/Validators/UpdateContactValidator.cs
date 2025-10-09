using FluentValidation;
using PhoneBook.Application.DTOs;
using System.Text.RegularExpressions;

namespace PhoneBook.Application.Validators
{
    public class UpdateContactValidator : AbstractValidator<UpdateContactDto>
    {
        public UpdateContactValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("شناسه نامعتبر است");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام الزامی است")
                .MaximumLength(100).WithMessage("نام نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("شماره موبایل الزامی است")
                .Length(11).WithMessage("شماره موبایل باید 11 رقم باشد")
                .Must(BeValidMobileNumber).WithMessage("شماره موبایل معتبر نیست (باید با 09 شروع شود)");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Now).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد")
                .When(x => x.BirthDate.HasValue);

            RuleFor(x => x.Image)
                .Must(BeValidImageFile).WithMessage("فقط فایل‌های تصویری (jpg, jpeg, png, gif) مجاز هستند")
                .Must(BeValidImageSize).WithMessage("حجم تصویر نباید بیشتر از 5 مگابایت باشد")
                .When(x => x.Image != null);
        }

        private bool BeValidMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber)) return false;
            return Regex.IsMatch(mobileNumber, @"^09\d{9}$");
        }

        private bool BeValidImageFile(IFormFile? file)
        {
            if (file == null) return true;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        private bool BeValidImageSize(IFormFile? file)
        {
            if (file == null) return true;
            const long maxSize = 5 * 1024 * 1024; // 5MB
            return file.Length <= maxSize;
        }
    }
}
