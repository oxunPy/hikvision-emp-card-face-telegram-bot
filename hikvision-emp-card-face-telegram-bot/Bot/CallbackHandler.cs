using Telegram.Bot;
using Telegram.Bot.Types;

namespace hikvision_emp_card_face_telegram_bot.Bot
{
    public class CallbackHandler
    {
        private readonly TelegramBotClient _botClient;

        public CallbackHandler(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            try
            {
                switch (callbackQuery.Data)
                {
                    case "action_1":
                        await HandleAction1(callbackQuery);
                        break;

                    case "action_2":
                        await HandleAction2(callbackQuery);
                        break;

                    default:
                        await HandleUnknownAction(callbackQuery);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling callback query: {ex.Message}");
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Something went wrong.");
            }
        }

        private async Task HandleAction1(CallbackQuery callbackQuery)
        {
            // Perform the action for callback "action_1"
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You selected Action 1");
            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Action 1 was selected.");
        }

        private async Task HandleAction2(CallbackQuery callbackQuery)
        {
            // Perform the action for callback "action_2"
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You selected Action 2");
            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Action 2 was selected.");
        }

        private async Task HandleUnknownAction(CallbackQuery callbackQuery)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Unknown action");
            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Unknown callback action.");
        }
    }
}
