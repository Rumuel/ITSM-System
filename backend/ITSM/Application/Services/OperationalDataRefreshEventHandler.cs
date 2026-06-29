using Application.Events;
using Application.Interfaces;

namespace Application.Services
{
    public sealed class OperationalDataRefreshEventHandler :
        IApplicationEventHandler<TicketCreatedEvent>,
        IApplicationEventHandler<TicketAssignmentChangedEvent>
    {
        private readonly IOperationalDataLoader _loader;

        public OperationalDataRefreshEventHandler(IOperationalDataLoader loader)
        {
            _loader = loader;
        }

        public Task HandleAsync(TicketCreatedEvent applicationEvent, CancellationToken cancellationToken = default) =>
            _loader.RefreshAsync(cancellationToken);

        public Task HandleAsync(TicketAssignmentChangedEvent applicationEvent, CancellationToken cancellationToken = default) =>
            _loader.RefreshAsync(cancellationToken);
    }
}
