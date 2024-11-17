using hikvision_emp_card_face_telegram_bot.Data.Report;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface ISelectedMenuService
    {
        bool HasEmployeeSelectedMealToday(long chatId);

        bool CreateOrUpdateSelectedMenuIfDeletedMeal(long chatId, long mealId);

        bool DeleteMyOrder(long chatId);

        Task<ICollection<SelectedMenuReport>> DailyReportForManager();
    }
}
