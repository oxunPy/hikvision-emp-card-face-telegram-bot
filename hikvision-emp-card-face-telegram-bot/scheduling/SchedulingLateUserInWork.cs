using hikvision_emp_card_face_telegram_bot.bot;
using hikvision_emp_card_face_telegram_bot.Bot;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Service;
using hikvision_emp_card_face_telegram_bot.Service.Impl;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace hikvision_emp_card_face_telegram_bot.scheduling
{
    public class SchedulingLateUserInWork : IHostedService, IDisposable
    {

        private readonly TelegramBotClient _botClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public SchedulingLateUserInWork(TelegramBotClient botClient, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _botClient = botClient;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            DateTime scheduleTime = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("Remainder:StartHour")).AddMinutes(_configuration.GetValue<int>("Remainder:StartMinute"));

            if(now > scheduleTime)
            {
                scheduleTime = scheduleTime.AddDays(1);
            }

            TimeSpan initialDelay = scheduleTime - now;
            TimeSpan repeatInterval = TimeSpan.FromDays(1);

            _timer = new Timer(DoSendRemainderToAllBotUsers, null,  initialDelay, repeatInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service is stopping...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void DoSendRemainderToAllBotUsers(object state)
        {
            int remainderHour = _configuration.GetValue<int>("Remainder:StartHour");
            int remainderMinute = _configuration.GetValue<int>("Remainder:StartMinute");

            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                ICollection<EmployeeDTO> employees = _employeeService.findAllLateInWorkWorkers(remainderHour, remainderMinute);

                foreach (EmployeeDTO e in employees)
                {
                    _botClient.SendTextMessageAsync(
                        chatId: e.TelegramChatId,
                        string.Format(ConstantTextMessages.LATE_IN_WORK, e.FirstName, e.LastName)
                        );
                }
            }
            
        }
    }
}
