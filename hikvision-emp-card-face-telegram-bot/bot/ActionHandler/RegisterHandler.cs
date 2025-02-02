using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Bot;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Service;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.bot.ActionHandler
{
    public class RegisterHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public RegisterHandler(TelegramBotClient botClient, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task HandleRegistrationAsync(Message message, RegistrationStates state, CancellationToken cancellationToken)
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();
                EmployeeDTO dto = new Dto.EmployeeDTO { };
                if (message != null)
                {
                    switch (state)
                    {
                        case RegistrationStates.FIRST_NAME:
                            dto.FirstName = message.Text;
                            _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.FIRST_NAME, ref dto);

                            await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Familyangizni kiriting!"
                                );

                            break;
                        case RegistrationStates.LAST_NAME:
                            dto.LastName = message.Text;
                            _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.LAST_NAME, ref dto);

                            await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Yuz rasmingizni kiriting! (maximum = 200kb)"
                                );

                            break;

                        case RegistrationStates.FACE_UPLOAD:
                            if (message.Photo != null && message.Photo.Length > 0)
                            {
                                string filePath = await SaveUploadedPhoto(message, cancellationToken);
                                dto.FaceImagePath = filePath;
                                _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.FACE_UPLOAD, ref dto);

                                await _botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: "Foydalanuvchi muvaffaqiyatli yaratildi!"
                                    );

                                var ChatID = message.Chat.Id;
                                if (ChatID != _configuration.GetValue<long>("Manager:ChatId"))
                                {
                                    await _botClient.SendTextMessageAsync(
                                    chatId: ChatID,
                                    text: "Pozitsiyangizni manager orqali kiritiladi.\n" +
                                    "Iltimos kuting!"
                                    );

                                    await _botClient.SendTextMessageAsync(
                                        chatId: _configuration.GetValue<long>("Manager:ChatId"),
                                        text: $"{dto.FirstName} {dto.LastName} ro'yhatdan o'tdi, iltimos uning pozitsiyasini belgilang!",
                                        replyMarkup: UserCommandMenus.GetPositionMarkupInline(ChatID)
                                        );
                                }
                                else
                                {
                                    dto.PositionEmp = Employee.Position.MANAGER;
                                    _employeeService.UpdateByChatID(ChatID, RegistrationStates.EMPLOYEE_POSITION, ref dto);
                                }

                                _employeeService.CreateNewHikiEmployee(message.Chat.Id);
                                _employeeService.SendFaceData(message.Chat.Id, filePath);
                            }
                            break;

                    }
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

            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "faces");
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

        public ReplyKeyboardMarkup GetEmployeePositionMarkup()
        {
            ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {Employee.Position.EMPLOYEE.ToString()},
                new KeyboardButton[] {Employee.Position.CATERING_MANAGER.ToString()},
                new KeyboardButton[] {Employee.Position.MANAGER.ToString()}
            });

            return markup;
        }

    }
}
