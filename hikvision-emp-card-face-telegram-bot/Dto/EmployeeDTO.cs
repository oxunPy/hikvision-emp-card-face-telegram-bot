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

        public Employee.Position? PositionEmp { get; set; }    

        public string? HikCardCode { get; set; }
    }
}
