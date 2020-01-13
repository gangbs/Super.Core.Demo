using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Super.Core.Calc.CalcService
{
    public class CalcService : BackgroundService
    {
        private readonly ILogger<CalcService> _logger;
        private readonly ICalcTaskQueue _queue;

        public CalcService(ILogger<CalcService> logger, ICalcTaskQueue queue)
        {
            _logger = logger;
            _queue = queue;
        }


        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            //服务启动前的操作
            await base.StartAsync(cancellationToken);
        }


        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            //服务关闭时的操作
           await base.StopAsync(cancellationToken);
        }


        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);


                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }
    }
}
