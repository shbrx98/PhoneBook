using FluentValidation;
using Microsoft.AspNetCore.Http;
using PhoneBook.Application.DTOs;

namespace PhoneBook.Application.Validators
{
    public class CreateContactValidator : AbstractValidator<CreateContactDto>
    {
        public CreateContactValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام الزامی است")
                .MaximumLength(100).WithMessage("نام نباید بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("شماره همراه الزامی است")
                .Matches(@"^09\d{9}$").WithMessage("شماره همراه باید 11 رقمی و با 09 شروع شود");

            RuleFor(x => x.BirthDate)
                .Must(BeValidBirthDate).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد")
                .When(x => x.BirthDate.HasValue);

            RuleFor(x => x.Image)
                .Must(BeValidImage).WithMessage("فرمت تصویر باید JPG, JPEG, PNG یا GIF باشد")
                .Must(BeValidSize).WithMessage("حجم تصویر نباید بیشتر از 5 مگابایت باشد")
                .When(x => x.Image != null);
        }

        private bool BeValidBirthDate(DateTime? birthDate)
        {
            if (!birthDate.HasValue) return true;
            
            // Compare only the date part to avoid time zone issues
            var today = DateTime.UtcNow.Date;
            var birthDateOnly = birthDate.Value.Date;
            
            var isValid = birthDateOnly <= today;
            
            return isValid;
        }

        private bool BeValidImage(IFormFile? file)
        {
            if (file == null) return true;
            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            
            return allowedExtensions.Contains(extension);
        }

        private bool BeValidSize(IFormFile? file)
        {
            if (file == null) return true;
            
            var maxSize = 5 * 1024 * 1024; // 5MB
            return file.Length <= maxSize;
        }
    }
}