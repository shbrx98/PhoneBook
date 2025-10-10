using PhoneBook.Application.DTOs;

namespace PhoneBook.Web.Models.ViewModels
{
    public class ContactFormViewModel
    {
        public CreateContactDto? CreateDto { get; set; }
        public UpdateContactDto? UpdateDto { get; set; }
        public bool IsEditMode { get; set; }
        public bool HasImage { get; set; }
        public string? ImageUrl { get; set; }
    }
}