using Application.Models;

namespace Application.Interfaces
{
    public interface IOperationalDataCache
    {
        OperationalDataSnapshot Current { get; }
        void Replace(OperationalDataSnapshot snapshot);
    }

    public interface IOperationalDataLoader
    {
        Task<OperationalDataSnapshot> LoadAsync(CancellationToken cancellationToken = default);
        Task RefreshAsync(CancellationToken cancellationToken = default);
    }
}
