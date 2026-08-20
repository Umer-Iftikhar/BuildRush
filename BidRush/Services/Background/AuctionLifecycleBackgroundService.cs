using BidRush.Services.Interfaces;

namespace BidRush.Services.Background
{
    public class AuctionLifecycleBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionLifecycleBackgroundService> _logger;

        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        public AuctionLifecycleBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AuctionLifecycleBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auction lifecycle background service started.");
            using var timer = new PeriodicTimer(Interval);


            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await UpdateAuctionStatusesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Auction lifecycle background service stopped.");
            }
        }

        private async Task UpdateAuctionStatusesAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();

                var updates = await auctionService.UpdateAuctionStatusesAsync();

                foreach (var update in updates)
                {
                    _logger.LogInformation(
                        "Auction {AuctionId} transitioned from {PreviousStatus} to {NewStatus}.",
                        update.AuctionId,
                        update.PreviousStatus,
                        update.NewStatus);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating auction statuses.");
            }
        }
    }
}
