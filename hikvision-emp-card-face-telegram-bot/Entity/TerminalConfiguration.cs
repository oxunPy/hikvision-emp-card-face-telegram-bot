using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Entity
{
    public class TerminalConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string? DeviceIp { get; set; }

        public int? Port { get; set; }

        public string? UserName { get; set; }   

        public string? Password { get; set; }

        public int? UserId { get; set; }

        public bool AutoLogin {  get; set; }    


        // convert entity to DTO
        public TerminalConfigurationDTO ToDTO()
        {
            return new TerminalConfigurationDTO
            {
                Id = this.Id,
                DeviceIp = this.DeviceIp,
                Port = this.Port,
                Username = this.UserName,
                Password = this.Password,
                UserId = this.UserId,
                AutoLogin = this.AutoLogin
            };
        }


    }
}
