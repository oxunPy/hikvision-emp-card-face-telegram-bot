using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace hikvision_emp_card_face_telegram_bot.bot
{
    public class MessageHandler
    {
        private readonly TelegramBotClient _botClient;

        public MessageHandler(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }


        public async Task HandleMessageAsync(Message message)
        {
            if (message.Type != MessageType.Text)
            {
                await HandleUnsupportedMessageType(message);
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

                    case "/help":
                        await HandleHelpCommand(message);
                        break;

                    default:
                        await HandleUnknownMessage(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message: {ex.Message}");
                await _botClient.SendTextMessageAsync(message.Chat.Id, "An error occurred while processing your message.");
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
            //replyMarkup: 
            );
        }

        private async Task HandleHelpCommand(Message message)
        {
            string helpText = "Available commands:\n" +
                              "/start - Start the bot\n" +
                              "/help - Show this help message\n" +
                              "hello - Say hello to the bot";

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: helpText
            );
        }

        private async Task HandleRegisterCommand(Message message)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Hello! How can I assist you today?"
            );
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
