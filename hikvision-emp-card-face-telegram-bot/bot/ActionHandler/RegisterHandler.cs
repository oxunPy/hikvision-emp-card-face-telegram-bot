using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Service;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace hikvision_emp_card_face_telegram_bot.bot.ActionHandler
{
    public class RegisterHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;

        public RegisterHandler(TelegramBotClient botClient, IServiceProvider serviceProvider)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
        }   

        public async Task HandleRegistrationAsync(Message message, RegistrationStates state, CancellationToken cancellationToken)
        {

            using (var scope = _serviceProvider.CreateScope())
            {
                var _employeeService = scope.ServiceProvider.GetService<IEmployeeService>();

                if (message != null)
                {
                    switch (state)
                    {
                        case RegistrationStates.FIRST_NAME:
                            _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.FIRST_NAME, new Dto.EmployeeDTO
                            {
                                FirstName = message.Text
                            });

                            await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Familyangizni kiriting!"
                                );

                            break;
                        case RegistrationStates.LAST_NAME:
                            _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.LAST_NAME, new Dto.EmployeeDTO
                            {
                                LastName = message.Text
                            });

                            await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Pozitsiyangizni kiriting!",
                                replyMarkup: GetEmployeePositionMarkup()
                                );

                            break;

                        case RegistrationStates.EMPLOYEE_POSITION:
                            _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.EMPLOYEE_POSITION, new Dto.EmployeeDTO
                            {
                                PositionEmp = (Employee.Position) Enum.Parse(typeof(Employee.Position), message.Text)
                            });

                            await _botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Yuz rasmingizni kiriting! (maximum = 200kb)"
                                );

                            // CREATE USER FOR HIKVISION

                            break;


                        case RegistrationStates.FACE_UPLOAD:
                            if(message.Photo != null && message.Photo.Length > 0)
                            {
                                string filePath = await SaveUploadedPhoto(message, cancellationToken);
                                _employeeService.UpdateByChatID(message.Chat.Id, RegistrationStates.FACE_UPLOAD, new Dto.EmployeeDTO
                                {
                                    FaceImagePath = filePath
                                });

                                await _botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: "Foydalanuvchi muvaffaqiyatli yaratildi!"
                                    );

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
