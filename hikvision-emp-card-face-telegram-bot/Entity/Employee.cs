using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace hikvision_emp_card_face_telegram_bot.Entity
{
    public class Employee
    {
        public enum Position
        {
            EMPLOYEE,
            MANAGER,
            CATERING_MANAGER
        }

        public enum RegistrationState
        {
            FIRST_NAME,
            LAST_NAME,
            POSITION,
            PIN_CODE,
            IMAGE_EMP,
            COMPLETED
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string? FirstName { get; set; }


        public string? LastName { get; set; }

        public string? FaceImagePath { get; set; }

        public string? TelegramChatId { get; set; }

        public string? PinCode { get; set; }

        public bool IsPinVerified { get; set; }

        public string? HikCardCode {  get; set; }
        
        public string? HikCardId { get; set; }
    }
}
