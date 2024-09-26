using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Entity
{
    public class Dish
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string? Name { get; set; }

        public decimal? Price { get; set; }

        public string? ImagePath { get; set; }

        // Method to convert entity to DTO
        public DishDTO ToDTO()
        {
            return new DishDTO
            {
                Id = this.Id,
                Name = this.Name,
                Price = this.Price,
            };
        }
    }
}
