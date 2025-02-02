namespace hikvision_emp_card_face_telegram_bot.Data.Report
{
    public class SelectedMenuReportInMonth
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? Date { get; set; }

        public decimal? DiscountPrice { get; set; }

        public decimal? DiscountPercent { get; set; }

        public string? DishName { get; set; }

        public decimal? DishPrice { get; set; }
    }
}
