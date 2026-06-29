using Application.Interfaces;

namespace api.Services
{
    public sealed class OperationalDataWarmupService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OperationalDataWarmupService> _logger;

        public OperationalDataWarmupService(
            IServiceScopeFactory scopeFactory,
            ILogger<OperationalDataWarmupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var loader = scope.ServiceProvider.GetRequiredService<IOperationalDataLoader>();
                await loader.RefreshAsync(cancellationToken);
                _logger.LogInformation("Estruturas operacionais em memoria carregadas da base de dados.");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Nao foi possivel carregar as estruturas em memoria no arranque. Sera feita nova tentativa na atribuicao de tickets.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
