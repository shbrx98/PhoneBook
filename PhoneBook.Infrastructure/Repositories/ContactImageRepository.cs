using Microsoft.EntityFrameworkCore;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using PhoneBook.Infrastructure.Data;

namespace PhoneBook.Infrastructure.Repositories
{
    public class ContactImageRepository : Repository<ContactImage>, IContactImageRepository
    {
        public ContactImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ContactImage?> GetByContactIdAsync(int contactId)
        {
            return await _dbSet.FirstOrDefaultAsync(ci => ci.ContactId == contactId);
        }

        public async Task DeleteByContactIdAsync(int contactId)
        {
            var image = await GetByContactIdAsync(contactId);
            if (image != null)
            {
                Delete(image);
            }
        }
    }
}


