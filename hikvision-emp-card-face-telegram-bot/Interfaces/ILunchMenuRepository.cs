using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Interfaces
{
    public interface ILunchMenuRepository
    {
        ICollection<LunchMenu> GetLunchMenus();

        LunchMenu? GetLunchMenu(long id);

        bool LunchMenuExists(long id);

        bool CreateLunchMenu(LunchMenu lunchMenu);

        bool UpdateLunchMenu(LunchMenu lunchMenu);

        bool DeleteLunchMenu(LunchMenu lunchMenu);

        bool Save();
    }
}
