using Application.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    public sealed class ApplicationEventDispatcher : IApplicationEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(
            TEvent applicationEvent,
            CancellationToken cancellationToken = default)
            where TEvent : IApplicationEvent
        {
            var handlers = _serviceProvider.GetServices<IApplicationEventHandler<TEvent>>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(applicationEvent, cancellationToken);
            }
        }
    }
}
