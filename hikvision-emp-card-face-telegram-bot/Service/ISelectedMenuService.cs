namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface ISelectedMenuService
    {
        bool HasEmployeeSelectedMealToday(long chatId);

        bool CreateOrUpdateSelectedMenuIfDeletedMeal(long chatId, long mealId);
    }
}
