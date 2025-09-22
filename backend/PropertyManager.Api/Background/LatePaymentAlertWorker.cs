using System.Threading;
using Microsoft.Extensions.Hosting;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Background;

public class LatePaymentAlertWorker(IAlertService alertService, ILogger<LatePaymentAlertWorker> logger) : BackgroundService
{
    // Guard to avoid duplicate startup log messages in case ExecuteAsync is invoked more than once for any reason.
    private static int _startedFlag = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Interlocked.Exchange(ref _startedFlag, 1) == 0)
        {
            logger.LogInformation("Late payment alert worker started.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await alertService.GenerateAlertsAsync(stoppingToken);
                logger.LogInformation("Late payment alerts generated at {Timestamp}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while generating late payment alerts");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
