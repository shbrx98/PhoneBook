namespace PhoneBook.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IContactRepository Contacts { get; }
        IContactImageRepository ContactImages { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}