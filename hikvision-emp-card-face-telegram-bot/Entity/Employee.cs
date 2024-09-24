using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using hikvision_emp_card_face_telegram_bot.Dto;

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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string? FirstName { get; set; }


        public string? LastName { get; set; }

        public string? FaceImagePath { get; set; }

        public string? TelegramChatId { get; set; }

        public string? PinCode { get; set; }
        public Position? PositionEmp { get; set; }

        public bool IsPinVerified { get; set; }

        public string? HikCardCode {  get; set; }
        
        public string? HikCardId { get; set; }

        public EmployeeDTO ToDTO()
        {
            return new EmployeeDTO
            {
                Id = this.Id,
                FirstName = this.FirstName,
                LastName = this.LastName,
                FaceImagePath = this.FaceImagePath,
                TelegramChatId = this.TelegramChatId,
                IsPinVerified = this.IsPinVerified,
                Position = this.PositionEmp
            };
        }
    }
  

}
