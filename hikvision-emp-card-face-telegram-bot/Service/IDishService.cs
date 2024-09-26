using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface IDishService
    {
        ICollection<DishDTO> GetDishesByTodaysMenu();

        ICollection<DishDTO> GetDishesByWeekDay(DayOfWeek dayOfWeek);

        DishDTO CreateNewDish(DishDTO dto);

        DishDTO UpdateDish(DishDTO dto);

        DishDTO GetLatestDishByLunchMenu(long dishId);

        bool DeleteDishAndItsRelatingImg(long dishId);
    }
}
