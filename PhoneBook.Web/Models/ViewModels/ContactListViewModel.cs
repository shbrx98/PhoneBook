using PhoneBook.Application.DTOs;

namespace PhoneBook.Web.Models.ViewModels
{
    public class ContactListViewModel
    {
        public IEnumerable<ContactDto> Contacts { get; set; } = new List<ContactDto>();
        public ContactSearchDto SearchDto { get; set; } = new ContactSearchDto();
        public int TotalCount { get; set; }
        public bool HasSearch { get; set; }
    }
}