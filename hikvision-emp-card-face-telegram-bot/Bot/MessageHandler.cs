using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Service.Impl;
using hikvision_emp_card_face_telegram_bot.Service;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.bot.ActionHandler;

namespace hikvision_emp_card_face_telegram_bot.bot
{
    public class MessageHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly RegisterHandler _registerHandler;
        private readonly IServiceProvider _serviceProvider;

        private Dictionary<long, RegistrationStates> _botUserRegistrationStates;

        public MessageHandler(TelegramBotClient botClient, IServiceProvider serviceProvider, RegisterHandler registerHandler)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _registerHandler = registerHandler;
            _botUserRegistrationStates = new Dictionary<long, RegistrationStates>();
        }


        public async Task HandleMessageAsync(Message message)
        {
            if (message.Type != MessageType.Text)
            {
                await HandleUnsupportedMessageType(message);
                return;
            }

            var ChatID = message.Chat.Id;

            if(_botUserRegistrationStates.ContainsKey(ChatID)) {
                _registerHandler.HandleRegistrationAsync(message);
                return;
            }

            try
            {
                switch (message.Text.ToLower())
                {
                    case "/start":
                        await HandleStartCommand(message);
                        break;

                    case "/inputMenu":
                        await HandleInputMenu(message);
                        break;

                    case "/register":
                        await HandleRegisterCommand(message);
                        break;

                    default:
                        await HandleUnknownMessage(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Error handling message: {ex.Message}");
            }
        }

        private async Task HandleInputMenu(Message message)
        {
            throw new NotImplementedException();
        }

        private async Task HandleStartCommand(Message message)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "This bot was created to order food for lunch. You can register through this bot " +
                      "\r\n \r\n You can use the following commands: \r\n \r\n " + 
                      "/start - to restart and change the language \r\n " + 
                      "/inputMenu - to enter today's menu \r\n " + 
                      "/register - for employee registration \r\n "+
                      "/help - to get information about the bot"
            );
        }

        private async Task HandleRegisterCommand(Message message)
        {
            var ChatID = message.Chat.Id;
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                EmployeeDTO botUser = _employeeService.FindByChatID(ChatID);
                if (botUser == null)
                {
                    _employeeService.CreateBotUser(ChatID);
                    _botUserRegistrationStates.Add(ChatID, RegistrationStates.FIRST_NAME);
                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Ismingizni kiriting!"
                        );
                }
                else
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: ChatID,
                        text: "Bu user avval registratsiya bo'lgan!");
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
