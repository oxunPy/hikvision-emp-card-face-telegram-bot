using hikvision_emp_card_face_telegram_bot.Bot;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Service;
using hikvision_emp_card_face_telegram_bot.Service.Impl;
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
        private readonly CallbackHandler _callbackHandler;
        private readonly MessageHandler _messageHandler;


        public TelegramBotService(ILogger<TelegramBotService> logger, 
                                  TelegramBotClient botClient, 
                                  MessageHandler messageHandler, 
                                  CallbackHandler callbackHandler)
        {
            _botClient = botClient;
            _logger = logger;
            _messageHandler = messageHandler;
            _callbackHandler = callbackHandler;
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
            if (update.Type == UpdateType.Message)
            {

                if(update.CallbackQuery != null)
                {
                    await _callbackHandler.HandleCallbackQueryAsync(update.CallbackQuery);
                }

                if(update.Message != null)
                {
                    await _messageHandler.HandleMessageAsync(update.Message, cancellationToken);
                }
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
