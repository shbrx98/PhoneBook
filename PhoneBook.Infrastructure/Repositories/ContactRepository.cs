using Microsoft.EntityFrameworkCore;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using PhoneBook.Infrastructure.Data;

namespace PhoneBook.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contact>> GetAllAsync()
        {
            return await _context.Contacts
                .Include(c => c.ContactImage)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        public async Task<Contact?> GetByIdWithImageAsync(int id)
        {
            return await _context.Contacts
                .Include(c => c.ContactImage)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber, int? excludeId = null)
        {
            var query = _context.Contacts.Where(c => c.MobileNumber == phoneNumber);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Contact>> SearchAsync(string? FullName, string? phoneNumber, DateTime? birthFrom, DateTime? birthTo)
        {
            var query = _context.Contacts.Include(c => c.ContactImage).AsQueryable();

            if (!string.IsNullOrWhiteSpace(FullName))
                query = query.Where(c => c.FullName.Contains(FullName));

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                query = query.Where(c => c.MobileNumber.Contains(phoneNumber));



            if (birthFrom.HasValue)
                query = query.Where(c => c.BirthDate >= birthFrom.Value);

            if (birthTo.HasValue)
                query = query.Where(c => c.BirthDate <= birthTo.Value);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Contact> AddAsync(Contact contact)
        {
            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task UpdateAsync(Contact contact)
        {
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int contactId)
        {
            var contact = await GetByIdAsync(contactId);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                
            }
        }

        public void Update(Contact contact)
        {
            _context.Entry(contact).State = EntityState.Modified;
            _context.Contacts.Update(contact);
        }

    }
}