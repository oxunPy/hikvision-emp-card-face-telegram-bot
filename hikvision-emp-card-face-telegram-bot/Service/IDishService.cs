using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface IDishService
    {
        ICollection<DishDTO> GetDishByTodaysMenu();
    }
}
