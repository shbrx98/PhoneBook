using PhoneBook.Domain.Entities;

namespace PhoneBook.Domain.Interfaces
{
    public interface IContactImageRepository : IRepository<ContactImage>
    {
        Task<ContactImage?> GetByContactIdAsync(int contactId);
        Task DeleteByContactIdAsync(int contactId);
    }
}
