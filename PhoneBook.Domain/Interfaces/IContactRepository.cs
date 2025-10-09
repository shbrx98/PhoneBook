using PhoneBook.Domain.Entities;

namespace PhoneBook.Domain.Interfaces
{
    public interface IContactRepository : IRepository<Contact>
    {
        Task<IEnumerable<Contact>> SearchContactsAsync(
            string? searchTerm,
            DateTime? birthDateFrom,
            DateTime? birthDateTo);
        
        Task<bool> IsMobileNumberExistsAsync(string mobileNumber, int? excludeContactId = null);
        
        Task<Contact?> GetContactWithImageAsync(int id);
    }
}