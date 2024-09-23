using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class TerminalConfigurationDTO
    {

        public long Id { get; set; }

        [Required(ErrorMessage = "Device ip can't be null")]
        [NotNull]
        public string? DeviceIp { get; set; }    

        public int? Port { get; set; }

        [Required (ErrorMessage  = "TerminalConfiguration username is not valid")]
        public string? Username {  get; set; }

        [Required(ErrorMessage = "TerminalConfiguration password is not valid")]
        public string? Password { get; set; }

        public int? UserId {get; set; }

        public bool AutoLogin {get; set; }
    }
}
