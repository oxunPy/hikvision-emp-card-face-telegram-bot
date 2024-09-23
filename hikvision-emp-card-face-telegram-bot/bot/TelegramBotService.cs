using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace hikvision_emp_card_face_telegram_bot.bot
{
    public class TelegramBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;

        public TelegramBotService(IConfiguration configuration, ILogger<TelegramBotService> logger)
        {
            // Read the bot token from appsettings.json
            string botToken = configuration["TelegramBot:BotToken"];
            _botClient = new TelegramBotClient(botToken);
            _logger = logger;
        }

        public async Task StartBotAsync(CancellationToken cancellationToken)
        {
            var me = await _botClient.GetMeAsync(cancellationToken);
            _logger.LogInformation($"Bot {me.Username} is starting...");


            // Start receiving messages
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new Telegram.Bot.Polling.ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                cancellationToken
                );
        }

        // Handle updates (messages, callbacks, etc.)
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var messageText = update.Message.Text;
                var chatId = update.Message.Chat.Id;

                _logger.LogInformation($"Received a message from {chatId}: {messageText}");

                // Example: Echo the received message back to the user
                await botClient.SendTextMessageAsync(chatId, $"You said: {messageText}", cancellationToken: cancellationToken);
            }
        }

        // Handle errors during bot polling
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(errorMessage);
            return Task.CompletedTask;
        }

    }
}
