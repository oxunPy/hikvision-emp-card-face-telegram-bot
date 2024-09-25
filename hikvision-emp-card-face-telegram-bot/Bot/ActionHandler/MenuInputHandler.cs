using hikvision_emp_card_face_telegram_bot.Bot.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace hikvision_emp_card_face_telegram_bot.Bot.ActionHandler
{
    public class MenuInputHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;

        public MenuInputHandler(TelegramBotClient botClient, IServiceProvider serviceProvider)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleInputMenuAsync(Message message, MenuInputStates state, CancellationToken cancellationToken)
        {

        }
    }
}
