using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class LunchMenuDTO
    {
        public long Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public List<long>? DishIds { get; set; } = new List<long>();

        public bool CurrentEdit { get; set; } = false;
    }
}
