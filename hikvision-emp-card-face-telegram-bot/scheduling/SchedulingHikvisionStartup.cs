using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Service;
using System.Threading;
using System.Threading.Tasks;

namespace hikvision_emp_card_face_telegram_bot.scheduling
{
    public class SchedulingHikvisionStartup : IHostedService, IDisposable
    {
        private readonly ITerminalConfigurationService _terminalConfigurationService;
        private Timer _timer;


        public SchedulingHikvisionStartup(ITerminalConfigurationService terminalConfigurationService)
        {
            _terminalConfigurationService = terminalConfigurationService;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoLogin, null, TimeSpan.FromMinutes(1), TimeSpan.FromDays(7));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service is stopping...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }


        private void DoLogin(object state)
        {
            _terminalConfigurationService.AutoLogin("admin", "JDev@2022");
        }

    }
}
