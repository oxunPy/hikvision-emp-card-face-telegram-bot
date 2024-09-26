using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Entity
{
    public class LunchMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [EnumDataType(typeof(DayOfWeek))]
        public DayOfWeek DayOfWeek { get; set; }

        // Storing List of Dish IDs as JSONB in PostgreSQL
        [Column(TypeName = "jsonb")]
        public List<long>? DishIds { get; set; } = new List<long>();

        public bool CurrentEdit { get; set; } = false;

        // Convert entity to DTO
        public LunchMenuDTO ToDTO()
        {
            return new LunchMenuDTO
            {
                Id = this.Id,
                DayOfWeek = this.DayOfWeek,
                DishIds = this.DishIds
            };
        }

        // Create entity from DTO
        public static LunchMenu FromDTO(LunchMenuDTO dto)
        {
            return new LunchMenu
            {
                Id = dto.Id,
                DayOfWeek = dto.DayOfWeek,
                DishIds = dto.DishIds,
            };
        } 
    }
}
