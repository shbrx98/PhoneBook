using Microsoft.EntityFrameworkCore;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Interfaces;
using PhoneBook.Infrastructure.Data;

namespace PhoneBook.Infrastructure.Repositories
{
    public class ContactImageRepository : IContactImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContactImage?> GetByContactIdAsync(int contactId)
        {
            return await _context.ContactImages
                .FirstOrDefaultAsync(i => i.ContactId == contactId);
        }

        public async Task AddAsync(ContactImage image)
        {
            await _context.ContactImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ContactImage image)
        {
            _context.ContactImages.Update(image);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int contactId)
        {
            var image = await GetByContactIdAsync(contactId);
            if (image != null)
            {
                _context.ContactImages.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }
}