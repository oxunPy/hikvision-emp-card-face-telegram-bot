using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Service;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.Bot
{
    public class CommonResponses
    {
        private IDishService _dishService;
        private TelegramBotClient _botClient;

        public CommonResponses(IDishService dishService, TelegramBotClient botClient)
        {
            _dishService = dishService;
            _botClient = botClient;
        }

        public async Task DishListInlineResponse(long chatId, DayOfWeek dayOfWeek)
        {
            //ICollection<DishDTO> dishesByDay = _dishService.GetDishesByWeekDay(DateTime.Now.DayOfWeek);
            ICollection<DishDTO> dishesByDay = _dishService.GetDishesByWeekDay(DayOfWeek.Monday);

            await _botClient.SendTextMessageAsync(
                   chatId: chatId,
                   "*Bugungi kun uchun taom tanlang!*",
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
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
                                InlineKeyboardButton.WithCallbackData("Tanlash", $"selectMeal_{dishesByDay.ElementAt(i).Id}"),
                            }
                        });

                        // Send photo with inline keyboard
                        string dishInfoText = $"Nomi: *{dishesByDay.ElementAt(i).Name}*\n\nNarxi: *{dishesByDay.ElementAt(i).Price} so'm*";
                        await _botClient.SendPhotoAsync(
                            chatId: chatId,
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
}
