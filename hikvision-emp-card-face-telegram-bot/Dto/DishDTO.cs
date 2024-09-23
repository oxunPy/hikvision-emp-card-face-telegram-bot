using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class DishDTO
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Dish dto name size is not valid")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Dish dto image path is not valid")]
        public string? ImagePath { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal? Price { get; set; }

        public long CategoryId { get; set; } // Foreign key for Category
    }
}
