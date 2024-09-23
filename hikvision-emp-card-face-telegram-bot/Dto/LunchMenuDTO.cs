using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class LunchMenuDTO
    {
        public long Id { get; set; }

        [NotNull]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public List<long>? DishIds { get; set; } 
    }
}
