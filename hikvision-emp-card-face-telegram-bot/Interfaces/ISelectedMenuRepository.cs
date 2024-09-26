using hikvision_emp_card_face_telegram_bot.Data.Report;
using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Interfaces
{
    public interface ISelectedMenuRepository
    {
        ICollection<SelectedMenu> GetSelectedMenus();

        SelectedMenu? GetSelectedMenus(long id);

        bool SelectedMenuExists(long id);

        bool CreateSelectedMenu(SelectedMenu selectedMenu);

        bool UpdateSelectedMenu(SelectedMenu selectedMenu);

        bool DeleteSelectedMenu(SelectedMenu selectedMenu);

        bool Save();

        SelectedMenu? GetTodaysSelectedMenuByEmployeeChatId(long chatId);

        Task<ICollection<SelectedMenuReport?>> findTodaySelectedMenus(DateTime today);
    }
}
