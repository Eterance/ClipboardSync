using Microsoft.AspNetCore.SignalR;

namespace ClipboardSync_Server
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<ServerHub> _signalRHub;

        public Worker(ILogger<Worker> logger, IHubContext<ServerHub> signalRHub)
        {
            _logger = logger;
            _signalRHub = signalRHub;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

    }
}