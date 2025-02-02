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
using OfficeOpenXml;

namespace hikvision_emp_card_face_telegram_bot.bot
{
    public class MessageHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly RegisterHandler _registerHandler;
        private readonly IServiceProvider _serviceProvider;
        private readonly MenuInputHandler _menuInputHandler;
        private readonly IConfiguration _configuration;


        private Dictionary<long, RegistrationStates> _botUserRegistrationStates;

        public MessageHandler(TelegramBotClient botClient, IServiceProvider serviceProvider, RegisterHandler registerHandler, MenuInputHandler menuInputHandler, IConfiguration configuration)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _registerHandler = registerHandler;
            _menuInputHandler = menuInputHandler;
            _botUserRegistrationStates = new Dictionary<long, RegistrationStates>();
            _configuration = configuration;
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

                        case UserCommandMenus.MONTHLY_ORDER_REPORT:
                            await HandleMonthlyReportManager(message);
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


                // handle late cause message 
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                    var botUser = _employeeService.FindByChatID(message.Chat.Id);

                    if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Equals(string.Format(ConstantTextMessages.LATE_IN_WORK, botUser.FirstName, botUser.LastName)))
                    {
                        // send confirmation to the manager
                        await _botClient.SendTextMessageAsync(_configuration.GetValue<long>("Manager:ChatId"),
                                $"Bugun {botUser.FirstName} {botUser.LastName}ga ishga kech qolish eslatmasi berildi, Unga tushlik beriladimi? \n\n" +
                                $"Uning xabari: {message.Text}",
                                replyMarkup: UserCommandMenus.GetConfirmationMarkupInline(ChatID)
                                );

                        await _botClient.SendTextMessageAsync(ChatID,
                                "Kech qolish sababingiz manager jo'natildi va uning tasdiqlashini kuting!");
                    }
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
                int count = 1;
                foreach(var item in selectedMenuReports)
                {
                    sb.Append($"{count++}. ");
                    sb.Append($"Taom: {item.DishName}, ");
                    sb.Append($"Narxi: {item.DishPrice}, ");
                    sb.Append($"Chegirmali Narxi: {item.DiscountPrice}, ");
                    sb.Append($"Shaxs: {item.EmployeeNames}\n");
                }
                if(sb.Length > 0)
                {
                    string text = "Bugungi kun uchun buyurtmalar hisoboti:\n\n";
                    await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text + 
                                sb.ToString()
                            );
                }
                else
                {
                    string text = "Bugungi kun uchun hali buyurmalar yoq!";

                    await _botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text
                        );
                }
            }
        }

        private async Task HandleMonthlyReportManager(Message message)
        {
            await _botClient.SendTextMessageAsync(
                               chatId: message.Chat.Id,
                               "Bir oylik buyurtmalar hisoboti:\n\n"
                           );

            using (var scope = _serviceProvider.CreateScope())
            {
                var _selectedMenuService = scope.ServiceProvider.GetService<ISelectedMenuService>();
                ICollection<SelectedMenuReportInMonth> selectedMenuReports = await _selectedMenuService.MonthlyReportForManager();

                if (selectedMenuReports == null || selectedMenuReports.Count == 0)
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        "30 kun mobaynida hali buyurtma bo'lmadi"
                    );
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "excel");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                var filename = $"report_month{DateTime.Now:yyyy-mm-dd}.xlsx";
                var filePath = Path.Combine(savePath, filename);

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("month");

                    int shiftRow = 1;
                    worksheet.Cells[shiftRow, 1].Value = "FirstName";
                    worksheet.Cells[shiftRow, 2].Value = "LastName";
                    worksheet.Cells[shiftRow, 3].Value = "Date";
                    worksheet.Cells[shiftRow, 4].Value = "DishName";
                    worksheet.Cells[shiftRow, 5].Value = "DishPrice";
                    worksheet.Cells[shiftRow, 6].Value = "DiscountPrice";
                    worksheet.Cells[shiftRow, 7].Value = "DiscountPercent";
                    shiftRow++;

                    for (int i = 0; i < selectedMenuReports.Count; i++)
                    {
                        worksheet.Cells[i + shiftRow, 1].Value = selectedMenuReports.ElementAt(i).FirstName;
                        worksheet.Cells[i + shiftRow, 2].Value = selectedMenuReports.ElementAt(i).LastName;
                        worksheet.Cells[i + shiftRow, 3].Value = selectedMenuReports.ElementAt(i).Date.Value.ToString("yyyy-MM-dd");
                        worksheet.Cells[i + shiftRow, 4].Value = selectedMenuReports.ElementAt(i).DishName;
                        worksheet.Cells[i + shiftRow, 5].Value = selectedMenuReports.ElementAt(i).DishPrice;
                        worksheet.Cells[i + shiftRow, 6].Value = selectedMenuReports.ElementAt(i).DiscountPrice;
                        worksheet.Cells[i + shiftRow, 7].Value = selectedMenuReports.ElementAt(i).DiscountPercent;

                    }

                    System.IO.File.WriteAllBytes(filePath, package.GetAsByteArray());
                }

                SendExcelFile(message.Chat.Id, filePath, filename);
            }
        }


        private async Task SendExcelFile(long chatId, string filePath, string fileName)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var excelFile = InputFileStream.FromStream(stream, fileName);
                await _botClient.SendDocumentAsync(chatId, excelFile, caption: "Bir oylik hisobot");
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
                    DateTime startDate = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("LunchTime:StartHour"));
                    DateTime endDate = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("LunchTime:EndHour")).AddMinutes(_configuration.GetValue<int>("LunchTime:EndMinute"));

                    if (botUser.PositionEmp != Employee.Position.FULL_ACCESS_UNLIMIT)
                    {
                        if (visitedDate >= startDate && visitedDate <= endDate)
                        {
                            if(botUser.VisitedDate == null)
                            {
                                // botUser.VisitedDate = DateTime.UtcNow;
                                // _employeeService.UpdateBotUserVisitDate(botUser);
                                await _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Siz Face-ID orqali ro'yhatdan o'tishingiz kerak!"
                                    );
                                return;
                            }
                            await _commonResponses.DishListInlineResponse(message.Chat.Id, DateTime.Now.DayOfWeek);
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(
                                message.Chat.Id,
                                "Siz vaqtida buyurtma berishga ulgarmadingiz! " +
                                $"({_configuration.GetValue<string>("LunchTime:StartHour")}:{_configuration.GetValue<string>("LunchTime:StartMinute")}, " +
                                $"{_configuration.GetValue<string>("LunchTime:EndHour")}:{_configuration.GetValue<string>("LunchTime:EndMinute")})"
                                );
                        }
                    }  
                    else
                    {
                        if (botUser.VisitedDate == null)
                        {
                            botUser.VisitedDate = DateTime.UtcNow;
                            _employeeService.UpdateBotUserVisitDate(botUser);
                        }
                        await _commonResponses.DishListInlineResponse(message.Chat.Id, DateTime.Now.DayOfWeek);
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



                    _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Hafta kunini tanglang!",
                        replyMarkup: UserCommandMenus.GetWeekDaysInlineMarkup()
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
                EmployeeService.CodeResultRegistration? codeResult = _employeeService.RegisterByChatID(ChatID, out string firstName, out string lastName);

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

                else if (codeResult.Equals(EmployeeService.CodeResultRegistration.FACE_UPLOAD))
                {
                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.FACE_UPLOAD);
                    else
                        _botUserRegistrationStates[ChatID] = RegistrationStates.FACE_UPLOAD;

                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Yuz rasmingizni kiriting! (maximum = 200kb)",
                        replyMarkup: new ReplyKeyboardRemove()
                        );
                }

                else if (codeResult.Equals(EmployeeService.CodeResultRegistration.EMPLOYEE_POSITION))
                {
                    if (!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.EMPLOYEE_POSITION);
                    else
                        _botUserRegistrationStates[ChatID] = RegistrationStates.EMPLOYEE_POSITION;


                    if(ChatID != _configuration.GetValue<long>("Manager:ChatId"))
                    {
                        await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Pozitsiyangizni manager orqali kiritiladi.\n" +
                              "Iltimos kuting!"
                        );

                        await _botClient.SendTextMessageAsync(
                            chatId: _configuration.GetValue<long>("Manager:ChatId"),
                            text: $"{firstName} {lastName} ro'yhatdan o'tdi, iltimos uning pozitsiyasini belgilang!",
                            replyMarkup: UserCommandMenus.GetPositionMarkupInline(ChatID)
                            );
                    }
                    else
                    {
                        var dto = new EmployeeDTO { PositionEmp = Employee.Position.MANAGER };
                        _employeeService.UpdateByChatID(ChatID, RegistrationStates.EMPLOYEE_POSITION, ref dto);
                    }
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
