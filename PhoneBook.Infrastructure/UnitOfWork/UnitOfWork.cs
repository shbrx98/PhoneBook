using Microsoft.EntityFrameworkCore.Storage;
using PhoneBook.Domain.Interfaces;
using PhoneBook.Infrastructure.Data;
using PhoneBook.Infrastructure.Repositories;

namespace PhoneBook.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IContactRepository Contacts { get; }
        public IContactImageRepository ContactImages { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Contacts = new ContactRepository(context);
            ContactImages = new ContactImageRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}