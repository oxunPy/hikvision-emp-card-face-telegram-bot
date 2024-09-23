using hikvision_emp_card_face_telegram_bot.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hikvision_emp_card_face_telegram_bot.Entity
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }    

        // One-to-Many relationship with Dish
        public List<Dish>? Dishes { get; set; }

        // Method to convert entity to DTO
        public CategoryDTO ToDTO()
        {
            return new CategoryDTO
            {
                Id = this.Id,
                Name = this.Name
            };
        }

        // Static method to create an entity from DTO
        public static Category FromDTO(CategoryDTO dto)
        {
            return new Category
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}
