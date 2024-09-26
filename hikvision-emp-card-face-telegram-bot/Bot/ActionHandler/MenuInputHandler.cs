using hikvision_emp_card_face_telegram_bot.Bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Service;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.Bot.ActionHandler
{
    public class MenuInputHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly CallbackHandler _callbackHandler;

        public MenuInputHandler(TelegramBotClient botClient, IServiceProvider serviceProvider, CallbackHandler callbackHandler)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _callbackHandler = callbackHandler;
        }

        public async Task HandleInputMenuAsync(Message message, MenuInputStates state, CancellationToken cancellationToken)
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                var _lunchMenuService = scope.ServiceProvider.GetService<ILunchMenuService>();
                var _dishService = scope.ServiceProvider.GetService<IDishService>();

                var lunchMenuDto = _lunchMenuService.GetCurrentEditMenu();

                switch (state)
                {
                    case MenuInputStates.DISH_NAME:
                        DishDTO newDish = _dishService.CreateNewDish(new DishDTO() { Name = message.Text});
                        if (newDish != null && newDish.Id != null)
                        {
                            lunchMenuDto.DishIds.Add(newDish.Id);
                            _lunchMenuService.UpdateLunchMenu(lunchMenuDto);
                            _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Rasm yuklang!"
                                );
                            
                            _callbackHandler.UpdateMenuInputState(message.Chat.Id, MenuInputStates.DISH_IMAGE);
                        }
                        break;

                    case MenuInputStates.DISH_IMAGE:
                        if (lunchMenuDto == null || lunchMenuDto.DishIds == null || lunchMenuDto.DishIds.Count() == 0)
                            return;

                        DishDTO existingDishForImg = _dishService.GetLatestDishByLunchMenu(lunchMenuDto.DishIds.Last());
                        if(existingDishForImg == null)
                        {
                            _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Taom topilmadi!"
                                );
                            return;
                        }
                        string imagePath = await SaveUploadedPhoto(message, cancellationToken);
                        existingDishForImg.ImagePath = imagePath;
                        _dishService.UpdateDish(existingDishForImg);
                        _botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Narxini kiriting!"
                            );

                        _callbackHandler.UpdateMenuInputState(message.Chat.Id, MenuInputStates.DISH_PRICE);
                        break;

                    case MenuInputStates.DISH_PRICE:
                        DishDTO existingDishForPrice = _dishService.GetLatestDishByLunchMenu(lunchMenuDto.DishIds.Last());
                        if (existingDishForPrice == null)
                        {
                            _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Taom topilmadi!"
                                );
                            return;
                        }

                        existingDishForPrice.Price = decimal.Parse(message.Text);
                        _dishService.UpdateDish(existingDishForPrice);
                        _callbackHandler?.UpdateMenuInputState(message.Chat.Id, MenuInputStates.COMPLETED);
                        _botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Taomingiz kiritildi!",
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new []
                                {
                                    InlineKeyboardButton.WithCallbackData("Yakunlash", "endForDish")
                                }
                            })
                            );

                        break;
                }
            }

            
        }

        private async Task<string> SaveUploadedPhoto(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;

            // Get the highest resolution photo (the last element in the Photo array)
            var photo = message.Photo[^1];
            var fileId = photo.FileId;

            // Get the file info using the file ID
            var file = await _botClient.GetFileAsync(fileId, cancellationToken);

            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dishes");
            // Ensure that the directory exists
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            // Generate a unique filename to avoid overwriting existing files
            var filename = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(savePath, filename);
            // Download the file to the specified path
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                await _botClient.DownloadFileAsync(file.FilePath, fs, cancellationToken);
            }

            return filePath;
        }
    }
}
