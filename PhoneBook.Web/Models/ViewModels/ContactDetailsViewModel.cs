using PhoneBook.Application.DTOs;

namespace PhoneBook.Web.Models.ViewModels
{
    public class ContactDetailsViewModel
    {
        public ContactDto Contact { get; set; } = null!;
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
    }
}