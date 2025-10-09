using Microsoft.AspNetCore.Http;

namespace PhoneBook.Application.DTOs
{
    public class ContactDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? BirthDatePersian { get; set; }
        public bool HasImage { get; set; }
    }

    public class CreateContactDto
    {
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateContactDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public IFormFile? Image { get; set; }
        public bool RemoveImage { get; set; }
    }

    public class ContactSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? BirthDateFrom { get; set; }
        public string? BirthDateTo { get; set; }
    }

    public class ContactImageDto
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}