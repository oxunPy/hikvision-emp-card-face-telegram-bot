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
                    switch (message.Text.ToLower())
                    {
                        case "/start":
                            await HandleStartCommand(message);
                            return;

                        case "/register":
                            await HandleRegisterCommand(message);
                            return;

                        case "/inputmenu":
                            await HandleInputMenu(message);
                            return;
                    }
                }

                if(isHandleForMenuInput && inputState != null)
                {
                    _menuInputHandler.HandleInputMenuAsync(message, (MenuInputStates) inputState, cancellationToken);
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

        private async Task HandleInputMenu(Message message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();

                EmployeeDTO employee = _employeeService.FindByChatID(message.Chat.Id);

                if (employee != null) 
                { 
                    if(employee.PositionEmp.Equals(Employee.Position.EMPLOYEE) || employee.PositionEmp.Equals(Employee.Position.MANAGER))
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
                 "*/start* - Botni ishga tushirish\n" +
                 "*/inputMenu* - Kundalik menyuni kiritish\n" +
                 "*/register* - Foydalanuvchini ro'yhatdan o'tish\n" +
                 "*/help* - Bot haqida ma'lumotlar";

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
            );
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
                    if(codeResult == null)
                        _employeeService.CreateBotUser(ChatID);
    
                    if(!_botUserRegistrationStates.ContainsKey(ChatID))
                        _botUserRegistrationStates.Add(ChatID, RegistrationStates.FIRST_NAME);
                    else 
                        _botUserRegistrationStates[ChatID] = RegistrationStates.FIRST_NAME;

                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Ismingizni kiriting!"
                        );
                }

                else if(codeResult.Equals(EmployeeService.CodeResultRegistration.LAST_NAME))
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

                else if(codeResult.Equals(EmployeeService.CodeResultRegistration.EMPLOYEE_POSITION))
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

                else if(codeResult.Equals(EmployeeService.CodeResultRegistration.FACE_UPLOAD))
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

                else if(codeResult.Equals(EmployeeService.CodeResultRegistration.COMPLETE))
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Bu user avval registratsiya bo'lgan!",
                        replyMarkup: new ReplyKeyboardRemove()
                        );
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
