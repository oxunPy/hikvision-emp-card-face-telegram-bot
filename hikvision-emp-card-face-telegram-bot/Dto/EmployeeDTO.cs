using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class EmployeeDTO
    {
        public long Id { get; set; }    

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? FaceImagePath { get; set; }

        public string? TelegramChatId { get; set; }

        public bool IsPinVerified { get; set; }

        public string? PinCode { get; set; }    
        public Employee.Position? Position { get; set; }    

        public string? HikCardCode { get; set; }

        public string? HikEmployeeId { get; set; }
    }
}
