using FluentValidation;
using Microsoft.AspNetCore.Http;
using PhoneBook.Application.DTOs;

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
                .MaximumLength(100).WithMessage("نام نباید بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("شماره همراه الزامی است")
                .Matches(@"^09\d{9}$").WithMessage("شماره همراه باید 11 رقمی و با 09 شروع شود");

            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(DateTime.Today).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد")
                .When(x => x.BirthDate.HasValue);

            RuleFor(x => x.Image)
                .Must(BeValidImage).WithMessage("فرمت تصویر باید JPG, JPEG یا PNG باشد")
                .Must(BeValidSize).WithMessage("حجم تصویر نباید بیشتر از 5 مگابایت باشد")
                .When(x => x.Image != null);
        }

        private bool BeValidImage(IFormFile? file)
        {
            if (file == null) return true;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private bool BeValidSize(IFormFile? file)
        {
            if (file == null) return true;
            return file.Length <= 5 * 1024 * 1024;
        }
    }
}