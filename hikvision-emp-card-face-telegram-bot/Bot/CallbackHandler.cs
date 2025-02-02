using hikvision_emp_card_face_telegram_bot.Bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Service;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.Bot
{
    public class CallbackHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private Dictionary<long, MenuInputStates> _botUserMenuInputStates;



        public CallbackHandler(TelegramBotClient botClient, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
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
                        case "meallist6":
                        case "meallist0":
                            await HandleMealListAsync(callbackQuery);
                            break;
                        case "addmeal1":
                        case "addmeal2":
                        case "addmeal3":
                        case "addmeal4":
                        case "addmeal5":
                        case "addmeal6":
                        case "addmeal0":
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

                    if(callbackQuery.Data.ToLower().StartsWith("deletemeal"))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _dishService = scope.ServiceProvider.GetService<IDishService>();
                            var _lunchService = scope.ServiceProvider.GetService<ILunchMenuService>();
                            // delete meal from db and clear from lunch menu dishIds
                            long mealId = long.Parse(callbackQuery.Data.Substring(callbackQuery.Data.LastIndexOf('_') + 1));
                            _dishService.DeleteDishAndItsRelatingImg(mealId);
                            _lunchService.ClearDishIdFromLunchMenu(mealId);
                        }
                    }

                    else if(callbackQuery.Data.ToLower().StartsWith("selectmeal"))
                    {
                        DateTime lunchStartTime = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("LunchTime:StartHour")).AddMinutes(_configuration.GetValue<int>("LunchTime:StartMinute"));
                        if (callbackQuery.Message.Date.AddHours(5) < DateTime.Now.Date || (callbackQuery.Message.Date.AddHours(5) < lunchStartTime))
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: callbackQuery.From.Id,
                                "Ushbu buyurtmaning vaqti tugagan. Siz bugungi kundagi buyurtmalarni tanlay olasiz!"
                                );
                            return;
                        }

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var _selectedMenuService = scope.ServiceProvider.GetService<ISelectedMenuService>();

                            long mealId = long.Parse(callbackQuery.Data.Substring(callbackQuery.Data.LastIndexOf('_') + 1));
                            // check first if employee has selected meal item today or not
                            if(_selectedMenuService.HasEmployeeSelectedMealToday(callbackQuery.From.Id))
                            {
                                await _botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.From.Id,
                                    text: "Siz bugungi kun uchun ovqat tanladingiz!"
                                    );
                                return;
                            }

                            Boolean result = _selectedMenuService.CreateOrUpdateSelectedMenuIfDeletedMeal(callbackQuery.From.Id, mealId);
                            if(result)
                            {
                                await _botClient.SendTextMessageAsync(
                                chatId: callbackQuery.From.Id,
                                text: "Sizning tanlovingiz qabul qilindi!"
                                );
                            } 
                            else
                            {
                                await _botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.From.Id,
                                    text: "Tanlovingiz qabul qilinmadi.\nUshbu taom menyudan o'chirilgan bo'lishi mumkin!"
                                    );
                            }
                            
                        }

                        return;
                    }
                   
                    
                    // MANAGER SELECT POSITION OF BOT USERS
                    {
                        if (callbackQuery.Data.ToLower().StartsWith(UserCommandMenus.EMPLOYEE_POS_INL))
                        {
                            HandleBotUserPositionByManagerCallback(callbackQuery, Employee.Position.EMPLOYEE);
                        }

                        else if (callbackQuery.Data.ToLower().StartsWith(UserCommandMenus.CATERING_MANAGER_POS_INL))
                        {
                            HandleBotUserPositionByManagerCallback(callbackQuery, Employee.Position.CATERING_MANAGER);
                        }

                        else if(callbackQuery.Data.ToLower().StartsWith(UserCommandMenus.MANAGER_POS_INL))
                        {
                            HandleBotUserPositionByManagerCallback(callbackQuery, Employee.Position.MANAGER);
                        }
                    }

                    // MANAGER HAS GIVED PERMISSION YES/NO 
                    {
                        if(callbackQuery.Data.ToLower().StartsWith(UserCommandMenus.YES))
                        {
                            var chatID = long.Parse(callbackQuery.Data.Substring(callbackQuery.Data.LastIndexOf("_") + 1));

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var _commonResponses = scope.ServiceProvider.GetService<CommonResponses>();

                                await _botClient.SendTextMessageAsync(
                                    chatId: chatID,
                                    text: "Manager sizga tushlik uchun ruhsat berdi!"
                                );

                                await _botClient.SendTextMessageAsync(
                                    chatId: callbackQuery.From.Id,
                                    text: "Foydalanuvchiga ruhsat berildi."
                                    );

                                _commonResponses.DishListInlineResponse(chatID, DateTime.Now.DayOfWeek);
                            }
                        }

                        else if(callbackQuery.Data.ToLower().StartsWith(UserCommandMenus.NO))
                        {
                            var chatID = long.Parse(callbackQuery.Data.Substring(callbackQuery.Data.LastIndexOf("_") + 1));
                            await _botClient.SendTextMessageAsync(
                                chatId: chatID,
                                text: "Manager sizga tushlik uchun ruhsat bermadi afsus!"
                                );

                            await _botClient.SendTextMessageAsync(
                                   chatId: callbackQuery.From.Id,
                                   text: "Foydalanuvchiga ruhsat berilmadi."
                                   );
                        }
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
                                    InlineKeyboardButton.WithCallbackData("O'chirish", $"deleteMeal_{dishesByDay.ElementAt(i).Id}"),
                                }
                            });

                            // Send photo with inline keyboard
                            string dishInfoText = $"Nomi: *{dishesByDay.ElementAt(i).Name}*\n\nNarxi: *{dishesByDay.ElementAt(i).Price} so'm*";
                            await _botClient.SendPhotoAsync(
                                chatId: callbackQuery.From.Id,
                                photo: photo,
                                caption: dishInfoText,
                                replyMarkup: inlineKeyboard,
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
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

        public void CompleteStateIfGivenMessageCommand(long chatId)
        {
            if (_botUserMenuInputStates.ContainsKey(chatId))
                _botUserMenuInputStates.Remove(chatId);
        }

        private async void HandleBotUserPositionByManagerCallback(CallbackQuery callbackQuery, Employee.Position position)
        {
            long ChatID = long.Parse(callbackQuery.Data.Substring(callbackQuery.Data.LastIndexOf("_") + 1));
            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                var dto = new EmployeeDTO { PositionEmp = position };
                _employeeService.UpdateByChatID(ChatID, bot.State.RegistrationStates.EMPLOYEE_POSITION, ref dto);
                await _botClient.SendTextMessageAsync(
                    chatId: ChatID,
                    text: "Sizga manager tomonidan pozitsiya belgilandi!\n" +
                          $"Sizning pozitsiyasingiz bu - {position.ToString()}"
                    );

                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.From.Id,
                    text: "Foydalanuvchiga ruxsat berildi!\n" + 
                          $"Foydalanuvchi {dto.FirstName} {dto.LastName} ning pozitsiya bu - {position.ToString()}" 
                    );
            }
        }
    }
}
