using PhoneBook.Application.DTOs;
using PhoneBook.Domain.Entities;

namespace PhoneBook.Application.Interfaces
{
    public interface IContactInterfaces
    {
        Task<IEnumerable<ContactDto>> GetAllContactsAsync();
        Task<ContactDto?> GetContactByIdAsync(int id);
        Task<ContactImage?> GetContactImageAsync(int contactId);
        Task<IEnumerable<ContactDto>> SearchContactsAsync(SearchContactDto search);
        Task<ServiceResult<int>> CreateContactAsync(CreateContactDto dto);
        Task<ServiceResult> UpdateContactAsync(UpdateContactDto dto);
        Task<ServiceResult> DeleteContactAsync(int id);
    }
}