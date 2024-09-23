using System.ComponentModel.DataAnnotations;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class CategoryDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Category dto name is not valid")]
        [StringLength(100, ErrorMessage = "Category dto name size is not valid")]
        public string? Name { get; set; }
    }
}
