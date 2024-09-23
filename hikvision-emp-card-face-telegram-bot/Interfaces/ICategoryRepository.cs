using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Interfaces
{
    public interface ICategoryRepository
    {
        ICollection<Category> GetCategories();

        Category? GetCategory(long id);

        ICollection<Dish> GetDishesByCategory(long categoryId);

        bool CategoryExists(long id);

        bool CreateCategory(Category category);

        bool UpdateCategory(Category category); 

        bool DeleteCategory(Category category);

        bool Save();
    }
}
