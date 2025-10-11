using PhoneBook.Application.DTOs;

namespace PhoneBook.Web.Models.ViewModels
{
    public class ContactListViewModel
    {
        public IEnumerable<ContactDto> Contacts { get; set; } = new List<ContactDto>();
        public  SearchContactDto SearchDto { get; set; } = new SearchContactDto();
        public int TotalCount { get; set; }
        public bool HasSearch { get; set; }
    }
}