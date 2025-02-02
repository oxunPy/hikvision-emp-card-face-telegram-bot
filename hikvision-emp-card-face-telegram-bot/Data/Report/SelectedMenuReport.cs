namespace hikvision_emp_card_face_telegram_bot.Data.Report
{
    public class SelectedMenuReport
    {
        public string? DishName { get; set; }

        public decimal? DishPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        
        public int? DiscountPercent { get; set; }
       
        public string? EmployeeNames { get; set; }
    }
}
