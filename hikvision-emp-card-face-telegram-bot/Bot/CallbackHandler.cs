using hikvision_emp_card_face_telegram_bot.Bot.State;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.Bot
{
    public class CallbackHandler
    {
        private readonly TelegramBotClient _botClient;
        private Dictionary<long, MenuInputStates> _botUserMenuInputStates;

        public CallbackHandler(TelegramBotClient botClient)
        {
            _botClient = botClient;
            _botUserMenuInputStates = new Dictionary<long, MenuInputStates>();
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {

            try
            {
                // Try to parse the callback data to a DayOfWeek enum
                if (Enum.TryParse(callbackQuery.Data, out DayOfWeek selectedDay))
                {
                    // Respond with the selected day of the week
                    await _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "Tanlangan kun uchun taom qo'shing!",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Taomlar ro'yhati", "mealList")
                            },

                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Taom qo'shish", "addMeal")
                            }
                        })
                    );
                }
                else
                {
                    switch (callbackQuery.Data.ToLower())
                    {
                        case "meallist":
                            await HandleMealListAsync(callbackQuery);
                            break;
                        case "addmeal":
                            await HandleAddMealListAsync(callbackQuery);
                            break;
                    }
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling callback query: {ex.Message}");
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Something went wrong.");
            }
        }

        private async Task HandleWeekDaysAsync(CallbackQuery callbackQuery)
        {
            // Perform the action for callback "action_1"
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You selected Action 1");
            await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Action 1 was selected.");
        }

        private async Task HandleMealListAsync(CallbackQuery callbackQuery)
        {
            _botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                "Taomlar ro'yhati"
                );
        }

        private async Task HandleAddMealListAsync(CallbackQuery callbackQuery)
        {
            _botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                "Taom qo'shish"
                );

            if (!_botUserMenuInputStates.ContainsKey(callbackQuery.From.Id))
                _botUserMenuInputStates.Add(callbackQuery.From.Id, MenuInputStates.DISH_NAME);


            switch(_botUserMenuInputStates[callbackQuery.From.Id])
            {
                // case MenuInputStates.DISH_NAME:
                    
            }
            
        }
    }
}
