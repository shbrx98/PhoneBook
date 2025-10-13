using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace PhoneBook.Application.DTOs
{
    public class ContactDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        
        [DataType(DataType.Date)]
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

    public class SearchContactDto
    {
        public string? FullName { get; set; }
        public string? MobileNumber { get; set; }
        public string? SearchTerm { get; set; }
        
        public string? BirthDateFrom { get; set; }
        public string? BirthDateTo { get; set; }
    }
}