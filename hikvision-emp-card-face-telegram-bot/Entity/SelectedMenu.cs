using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Entity
{
    public class SelectedMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; } 

        public DateTime Date { get; set; }

        public long? DishId {  get; set; }

        [ForeignKey("DishId")]
        public Dish? Dish { get; set; }

        public decimal? DiscountPrice { get; set; }

        public decimal? DiscountPercent { get; set; }   
            

        // Convert entity to DTO
        public SelectedMenuDTO ToDTO() 
        {
            return new SelectedMenuDTO
            {
                Id = this.Id,
                EmployeeId = this.EmployeeId,
                Date = this.Date,
                DishId = this.DishId,
                DiscountPercent = this.DiscountPercent,
                DiscountPrice = this.DiscountPrice
            };
        }
    }
}
