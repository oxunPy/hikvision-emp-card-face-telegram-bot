using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class DishDTO
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? ImagePath { get; set; }

        public decimal? Price { get; set; }
    }
}
