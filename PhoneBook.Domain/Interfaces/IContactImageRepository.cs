using PhoneBook.Domain.Entities;

namespace PhoneBook.Domain.Interfaces
{
    public interface IContactImageRepository
    {
        Task<ContactImage?> GetByContactIdAsync(int contactId);
        Task AddAsync(ContactImage image);
        Task UpdateAsync(ContactImage image);
        Task DeleteAsync(int contactId);
    }
}