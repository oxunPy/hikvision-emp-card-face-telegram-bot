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

        [Required]
        public string? Name { get; set; }

        [Required]
        public decimal? Price { get; set; }

        public string? ImagePath { get; set; }

        // Foreign Key for Category
        public long CategoryId { get; set; }

        // Many-to-One relationship with Category
        [ForeignKey("CategoryId")] 
        public Category? Category { get; set; }  

        // Method to convert entity to DTO
        public DishDTO ToDTO()
        {
            return new DishDTO
            {
                Id = this.Id,
                Name = this.Name,
                Price = this.Price,
                CategoryId = this.CategoryId
            };
        }
    }
}
