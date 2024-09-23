namespace hikvision_emp_card_face_telegram_bot.Dto
{
    public class SelectedMenuDTO
    {
        public long Id {get; set;}
        
        public long EmployeeId { get; set;} 

        public DateTime Date { get; set;}

        public long DishId { get; set;}

        public decimal? DiscountPrice {get; set;}

        public decimal? DiscountPercent { get; set;}
    }
}
