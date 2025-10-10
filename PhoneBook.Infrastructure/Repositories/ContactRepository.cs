using Microsoft.EntityFrameworkCore;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using PhoneBook.Infrastructure.Data;

namespace PhoneBook.Infrastructure.Repositories
{
    public class ContactRepository : Repository<Contact>, IContactRepository
    {
        public ContactRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Contact>> SearchContactsAsync(
            string? searchTerm,
            DateTime? birthDateFrom,
            DateTime? birthDateTo)
        {
            var query = _dbSet.Include(c => c.ContactImage).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(searchTerm) ||
                    c.MobileNumber.Contains(searchTerm));
            }

            if (birthDateFrom.HasValue)
            {
                query = query.Where(c => c.BirthDate >= birthDateFrom.Value);
            }

            if (birthDateTo.HasValue)
            {
                query = query.Where(c => c.BirthDate <= birthDateTo.Value);
            }

            return await query.OrderBy(c => c.FullName).ToListAsync();
        }

        public async Task<bool> IsMobileNumberExistsAsync(string mobileNumber, int? excludeContactId = null)
        {
            var query = _dbSet.Where(c => c.MobileNumber == mobileNumber);

            if (excludeContactId.HasValue)
            {
                query = query.Where(c => c.Id != excludeContactId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Contact?> GetContactWithImageAsync(int id)
        {
            return await _dbSet
                .Include(c => c.ContactImage)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Contact>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.ContactImage)
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }
    }
}