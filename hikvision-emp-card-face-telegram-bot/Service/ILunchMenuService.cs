using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface ILunchMenuService
    {
        LunchMenuDTO GetTodaysMenu();

        LunchMenuDTO GetCurrentEditMenu();

        bool UpdateTodaysLunchMenu(LunchMenuDTO lunchMenu);

        bool UpdateLunchMenu(LunchMenuDTO lunchMenu);

        LunchMenuDTO SetCurrentEdit(DayOfWeek dayOfWeek);

        bool ClearDishIdFromLunchMenu(long dishId);
    }
}
