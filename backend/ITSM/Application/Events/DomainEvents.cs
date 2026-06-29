namespace Application.Events
{
    public interface IApplicationEvent
    {
    }

    public sealed record TicketCreatedEvent(int TicketId) : IApplicationEvent;
    public sealed record TicketAssignmentChangedEvent(int TicketId, int TechnicianId) : IApplicationEvent;

    public interface IApplicationEventHandler<in TEvent> where TEvent : IApplicationEvent
    {
        Task HandleAsync(TEvent applicationEvent, CancellationToken cancellationToken = default);
    }

    public interface IApplicationEventDispatcher
    {
        Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default)
            where TEvent : IApplicationEvent;
    }
}
