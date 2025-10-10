using PhoneBook.Application.DTOs;
using PhoneBook.Domain.Entities;
namespace PhoneBook.Application.Services
{
    public interface IContactService
    {
        Task<IEnumerable<ContactDto>> GetAllContactsAsync();
        Task<ContactDto?> GetContactByIdAsync(int id);
        Task<ContactImage?> GetContactImageAsync(int contactId);
        Task<IEnumerable<ContactDto>> SearchContactsAsync(SearchContactDto searchDto);
        Task<int> CreateContactAsync(CreateContactDto dto);
        Task UpdateContactAsync(UpdateContactDto dto);
        Task DeleteContactAsync(int id);
        Task<bool> IsMobileNumberExistsAsync(string mobileNumber, int? contactId = null);
    }
}