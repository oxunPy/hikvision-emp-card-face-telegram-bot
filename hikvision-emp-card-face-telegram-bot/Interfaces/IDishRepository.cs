using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Interfaces
{
    public interface IDishRepository
    {
        ICollection<Dish> GetDishes();

        Dish? GetDish(long id);

        ICollection<Dish> GetDishesByCategory(long categoryId);

        bool DishExists(long id);

        bool CreateDish(Dish dish);

        bool UpdateDish(Dish dish);

        bool DeleteDish(Dish dish);

        bool Save();
    }
}
