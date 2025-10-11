using PhoneBook.Domain.Entities;

namespace PhoneBook.Domain.Interfaces
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllAsync();
        Task<Contact?> GetByIdAsync(int id);
        Task<Contact?> GetByIdWithImageAsync(int id);
        Task<bool> PhoneNumberExistsAsync(string phoneNumber, int? excludeId = null);
        Task<IEnumerable<Contact>> SearchAsync(string? name, string? phone, DateTime? birthDateFrom, DateTime? birthDateTo);
        Task<Contact> AddAsync(Contact contact);
        Task UpdateAsync(Contact contact);
        Task DeleteAsync(int id);
        void Update(Contact contact);

  }
}