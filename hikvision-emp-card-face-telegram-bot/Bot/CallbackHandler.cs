using hikvision_emp_card_face_telegram_bot.Bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Service;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.Bot
{
    public class CallbackHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<long, MenuInputStates> _botUserMenuInputStates;


        public CallbackHandler(TelegramBotClient botClient, IServiceProvider serviceProvider)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
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
                        text: $"Tanlangan kun uchun taom qo'shing! ({selectedDay.ToString()})",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Taomlar ro'yhati", "mealList" + (int) selectedDay)
                            },

                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Taom qo'shish", "addMeal" + (int) selectedDay)
                            }
                        })
                    );
                }
                else
                {
                    switch (callbackQuery.Data.ToLower())
                    {
                        case "meallist1":
                        case "meallist2":
                        case "meallist3":
                        case "meallist4":
                        case "meallist5":
                            await HandleMealListAsync(callbackQuery);
                            break;
                        case "addmeal1":
                        case "addmeal2":
                        case "addmeal3":
                        case "addmeal4":
                        case "addmeal5":
                            await HandleAddMealListAsync(callbackQuery);
                            break;

                        case "endfordish":
                            if(_botUserMenuInputStates.ContainsKey(callbackQuery.From.Id))
                            {
                                _botUserMenuInputStates.Remove(callbackQuery.From.Id);

                                _botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.From.Id,
                                    text: "Muvaffaqiyatli kiritildi!"
                                    );
                            }  
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dishService = scope.ServiceProvider.GetService<IDishService>();
                DayOfWeek dayOfWeek = (DayOfWeek)(callbackQuery.Data.LastOrDefault() - '0');
                ICollection<DishDTO> dishesByDay = _dishService.GetDishesByWeekDay(dayOfWeek);

                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.From.Id,
                    "Taomlar ro'yhati"
                    );

                if (dishesByDay != null && dishesByDay.Count > 0)
                {
                    for (int i = 0; i < dishesByDay.Count; i++)
                    {
                        // Load the image file
                        using (FileStream fs = new FileStream(dishesByDay.ElementAt(i).ImagePath, FileMode.Open, FileAccess.Read))
                        {
                            var photo = InputFile.FromStream(fs);

                            // Create inline keyboard
                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("O'chirish", $"deleteMeal{dishesByDay.ElementAt(i).Id}"),
                                }
                            });

                            // Send photo with inline keyboard
                            await _botClient.SendPhotoAsync(
                                chatId: callbackQuery.From.Id,
                                photo: photo,
                                caption: "Here is your image with an inline keyboard!",
                                replyMarkup: inlineKeyboard
                            );
                        }
                    }
                }
            }
        }

        private async Task HandleAddMealListAsync(CallbackQuery callbackQuery)
        {
            if (!_botUserMenuInputStates.ContainsKey(callbackQuery.From.Id))
                _botUserMenuInputStates.Add(callbackQuery.From.Id, MenuInputStates.DISH_NAME);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _lunchMenuService = scope.ServiceProvider.GetService<ILunchMenuService>();
                DayOfWeek dayOfWeek = (DayOfWeek) (callbackQuery.Data.LastOrDefault() - '0');
                _lunchMenuService.SetCurrentEdit(dayOfWeek);
            }

            switch(_botUserMenuInputStates[callbackQuery.From.Id])
            {
                case MenuInputStates.DISH_NAME:
                    _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.From.Id,
                        "Taom nomini kiriting!"
                        );
                    break;

                case MenuInputStates.DISH_IMAGE:
                    _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.From.Id,
                        "Taom rasmini kiriting!"
                        );
                    break;
                
                case MenuInputStates.DISH_PRICE:
                    _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.From.Id,
                        "Taom narxini kiriting!"
                        );
                    break;

                case MenuInputStates.COMPLETED:
                    _botClient.SendTextMessageAsync(
                        chatId: callbackQuery.From.Id,
                        "Taomingiz kiritildi!"
                        );
                    _botUserMenuInputStates.Remove(callbackQuery.From.Id);
                    break;
            }      
        }


        public bool IsHandlingForMenuInput(long chatId)
        {
            return _botUserMenuInputStates != null && _botUserMenuInputStates.ContainsKey(chatId) && (
                   _botUserMenuInputStates[chatId].Equals(MenuInputStates.DISH_NAME) || 
                   _botUserMenuInputStates[chatId].Equals(MenuInputStates.DISH_PRICE) ||
                   _botUserMenuInputStates[chatId].Equals(MenuInputStates.DISH_IMAGE));
        }   

        public MenuInputStates? GetMenuInputState(long chatId) 
        {
            return _botUserMenuInputStates.ContainsKey(chatId) ? _botUserMenuInputStates[chatId] : null;
        }

        public void UpdateMenuInputState(long chatId, MenuInputStates state)
        {
            if (_botUserMenuInputStates.ContainsKey(chatId))
                _botUserMenuInputStates[chatId] = state;
        }
    }
}
