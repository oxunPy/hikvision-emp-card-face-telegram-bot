using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Service.Impl;
using hikvision_emp_card_face_telegram_bot.Service;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.bot.ActionHandler;
using Telegram.Bot.Types.ReplyMarkups;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Bot.State;
using hikvision_emp_card_face_telegram_bot.Bot.ActionHandler;
using hikvision_emp_card_face_telegram_bot.Bot;
using hikvision_emp_card_face_telegram_bot.Data.Report;
using System.Text;
using System.Net.Mail;

namespace hikvision_emp_card_face_telegram_bot.bot
{
    public class MessageHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly RegisterHandler _registerHandler;
        private readonly IServiceProvider _serviceProvider;
        private readonly MenuInputHandler _menuInputHandler;


        private Dictionary<long, RegistrationStates> _botUserRegistrationStates;


        public MessageHandler(TelegramBotClient botClient, IServiceProvider serviceProvider, RegisterHandler registerHandler, MenuInputHandler menuInputHandler)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _registerHandler = registerHandler;
            _menuInputHandler = menuInputHandler;
            _botUserRegistrationStates = new Dictionary<long, RegistrationStates>();
        }


        public async Task HandleMessageAsync(Message message, bool isHandleForMenuInput, MenuInputStates? inputState, CancellationToken cancellationToken)
        {
            var ChatID = message.Chat.Id;

            try
            {
                if (message.Type == MessageType.Text)
                {
                    switch (message.Text)
                    {
                        case UserCommandMenus.START:
                            await HandleStartCommand(message);
                            return;

                        case UserCommandMenus.REGISTER:
                            await HandleRegisterCommand(message);
                            return;

                        case UserCommandMenus.REFRESH_WEEKLY_MENU:
                            await HandleInputMenu(message);
                            return;

                        case UserCommandMenus.I_DONT_EAT:
                            await HandleIdontEat(message);
                            return;

                        case UserCommandMenus.REPEAT_ORDER:
                            await HandleRepeatOrder(message);
                            return;

                        case UserCommandMenus.DAILY_ORDER_REPORT:
                            await HandleDailyReportManager(message);
                            return;
                    }
                }

                if (isHandleForMenuInput && inputState != null)
                {
                    _menuInputHandler.HandleInputMenuAsync(message, (MenuInputStates)inputState, cancellationToken);
                    return;
                }

                // handle registration states
                if (_botUserRegistrationStates.ContainsKey(ChatID))
                {
                    _registerHandler.HandleRegistrationAsync(message, _botUserRegistrationStates[ChatID], cancellationToken);
                    _botUserRegistrationStates[ChatID] = _botUserRegistrationStates[ChatID] + 1;

                    if (_botUserRegistrationStates[ChatID].Equals(RegistrationStates.COMPLETED))
                    {
                        _botUserRegistrationStates.Remove(ChatID);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Error handling message: {ex.Message}");
            }
        }

        private async Task HandleDailyReportManager(Message message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _selectedMenuService = scope.ServiceProvider.GetService<ISelectedMenuService>();
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                var manager = _employeeService.FindByChatID(message.Chat.Id);
                if (manager == null || manager.PositionEmp != Employee.Position.MANAGER)
                    return;

                ICollection<SelectedMenuReport> selectedMenuReports = await _selectedMenuService.DailyReportForManager();

                StringBuilder sb = new StringBuilder();
                foreach(var item in selectedMenuReports)
                {
                    sb.Append($"Dish: {item.DishName}, ");
                    sb.Append($"Discount: {item.DiscountPercent}, ");

                    sb.Append($"Emp: {item.EmployeeNames}\n");
                }
                if(sb.Length > 0)
                {
                    await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                sb.ToString()
                            );
                }
            }
        }

        private async Task HandleRepeatOrder(Message message)
        {
            // check the VisetedDate within the period if BotUser's position is equals To Employee 
            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                var _selectedMenuService = scope.ServiceProvider.GetService<ISelectedMenuService>();
                var _commonResponses = scope.ServiceProvider.GetService<CommonResponses>();

                bool isOrderedForToday = _employeeService.OrderedTodaysMenu(message.Chat.Id);
                if (isOrderedForToday)
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        "*Siz oldin buyurtma bergansiz!*",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
                        );
                    return;
                }

                var botUser = _employeeService.FindByChatID(message.Chat.Id);
                if (botUser != null)
                {
                    // check the order period is valid!
                    DateTime visitedDate = botUser.VisitedDate == null ? DateTime.Now : (DateTime) botUser.VisitedDate;
                    DateTime startDate = DateTime.Now.Date.AddHours(9);
                    DateTime endDate = DateTime.Now.Date.AddHours(10).AddMinutes(30);

                    if (botUser.PositionEmp == Employee.Position.EMPLOYEE)
                    {
                        if (visitedDate >= startDate && visitedDate <= endDate)
                        {
                            if(botUser.VisitedDate == null)
                            {
                                botUser.VisitedDate = DateTime.UtcNow;
                                _employeeService.UpdateBotUserVisitDate(botUser);
                            }
                            await _commonResponses.DishListInlineResponse(message.Chat.Id, DayOfWeek.Monday);
                        }
                    }
                    else
                    {
                        if (botUser.VisitedDate == null)
                        {
                            botUser.VisitedDate = DateTime.UtcNow;
                            _employeeService.UpdateBotUserVisitDate(botUser);
                        }
                        await _commonResponses.DishListInlineResponse(message.Chat.Id, DayOfWeek.Monday);
                    }

                }
            }
        }

        private async Task HandleIdontEat(Message message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _selectedMenuService = scope.ServiceProvider.GetService<ISelectedMenuService>();
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();

                var employee = _employeeService.FindByChatID(message.Chat.Id);

                if (employee != null)
                {
                    if (_selectedMenuService.DeleteMyOrder(message.Chat.Id))
                    {
                        await _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Sizning buyurtmangiz bekor qilindi!",
                                    replyMarkup: UserCommandMenus.GetAccessibleCommands(employee.PositionEmp)
                                );
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Sizda buyurtma mavjud emas!",
                                    replyMarkup: UserCommandMenus.GetAccessibleCommands(employee.PositionEmp)
                                );
                    }
                }

            }
        }

        private async Task HandleInputMenu(Message message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();

                EmployeeDTO employee = _employeeService.FindByChatID(message.Chat.Id);

                if (employee != null)
                {
                    if (employee.PositionEmp.Equals(Employee.Position.EMPLOYEE) || employee.PositionEmp.Equals(Employee.Position.MANAGER))
                    {
                        _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Sizda kundalik menyu kiritishingizga ruxsat berilmagan"
                            );
                        return;
                    }

                    // Create an inline keyboard with the days of the week
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // Row 1
                        {
                            InlineKeyboardButton.WithCallbackData("Monday", "Monday"),
                            InlineKeyboardButton.WithCallbackData("Tuesday", "Tuesday"),
                        },
                        new [] // Row 2
                        {
                            InlineKeyboardButton.WithCallbackData("Wednesday", "Wednesday"),
                            InlineKeyboardButton.WithCallbackData("Thursday", "Thursday"),
                        },
                        new [] // Row 3
                        {
                            InlineKeyboardButton.WithCallbackData("Friday", "Friday"),
                        },
                    });

                    _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Hafta kunini tanglang!",
                        replyMarkup: inlineKeyboard
                        );
                }

            }
        }

        private async Task HandleStartCommand(Message message)
        {

            string text = "*Bu bot tushlik uchun ovqat buyurtma qilish uchun yaratilgan.*\n\n" +
                          "Siz botda ro'yhatdan o'tish uchun quyidagi kommandalardan foydalanishingiz mumkin:\n\n" +
                          "*/start*        - Botni ishga tushirish\n" +
                          "*/register*  - Foydalanuvchini ro'yhatdan o'tish\n" +
                          "*/help*         - Bot haqida ma'lumotlar";

            // Check user exists if true give corresponding CommandMenus
            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                var user = _employeeService.FindByChatID(message.Chat.Id);
                ReplyKeyboardMarkup markup = null;
                if (user != null)
                {
                    markup = UserCommandMenus.GetAccessibleCommands(user.PositionEmp);
                }

                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    replyMarkup: markup == null ? new ReplyKeyboardRemove() : markup
                );
            }
        }

        private async Task HandleRegisterCommand(Message message)
        {
            var ChatID = message.Chat.Id;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                EmployeeService.CodeResultRegistration? codeResult = _employeeService.RegisterByChatID(ChatID);

                if (codeResult == null || codeResult.Equals(EmployeeService.CodeResultRegistration.FIRST_NAME))
                {
                    if (codeResult == null)
                        _employeeService.CreateBotUser(ChatID);

                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.FIRST_NAME);
                    else
                        _botUserRegistrationStates[ChatID] = RegistrationStates.FIRST_NAME;

                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Ismingizni kiriting!"
                        );
                }

                else if (codeResult.Equals(EmployeeService.CodeResultRegistration.LAST_NAME))
                {
                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.LAST_NAME);
                    else
                        _botUserRegistrationStates[ChatID] = RegistrationStates.LAST_NAME;

                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Familyangizni kiriting!"
                        );
                }

                else if (codeResult.Equals(EmployeeService.CodeResultRegistration.EMPLOYEE_POSITION))
                {
                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.EMPLOYEE_POSITION);
                    else
                        _botUserRegistrationStates[ChatID] = RegistrationStates.EMPLOYEE_POSITION;

                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Pozitsiyangizni kiriting!",
                        replyMarkup: _registerHandler.GetEmployeePositionMarkup()
                        );
                }

                else if (codeResult.Equals(EmployeeService.CodeResultRegistration.FACE_UPLOAD))
                {
                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.FACE_UPLOAD);
                    else
                        _botUserRegistrationStates[ChatID] = RegistrationStates.FACE_UPLOAD;

                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Yuz rasmingizni kiriting!",
                        replyMarkup: new ReplyKeyboardRemove()
                        );
                }

                else if (codeResult.Equals(EmployeeService.CodeResultRegistration.COMPLETE))
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Bu user avval registratsiya bo'lgan!"
                        );

                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                    {
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.FACE_UPLOAD);
                        _employeeService.RemoveImgInCaseErrorRecognize(message.Chat.Id);
                    }
                }
            }
        }

        private async Task HandleUnknownMessage(Message message)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Sorry, I don't understand that command. Use /help to see available commands."
            );
        }

        private async Task HandleUnsupportedMessageType(Message message)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "This bot only supports text messages at the moment."
            );
        }
    }
}
