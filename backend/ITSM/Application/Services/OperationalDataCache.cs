using Application.Interfaces;
using Application.Models;

namespace Application.Services
{
    public sealed class OperationalDataCache : IOperationalDataCache
    {
        private OperationalDataSnapshot _current = OperationalDataSnapshot.Empty;

        public OperationalDataSnapshot Current => Volatile.Read(ref _current);

        public void Replace(OperationalDataSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            Interlocked.Exchange(ref _current, snapshot);
        }
    }
}
