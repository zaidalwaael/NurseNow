using Microsoft.Extensions.Hosting;

namespace NurseNow.Services
{
    public class NotificationSchedulerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}