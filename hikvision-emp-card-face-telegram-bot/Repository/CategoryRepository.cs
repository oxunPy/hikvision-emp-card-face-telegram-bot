using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private DataContext _dbContext;

        public CategoryRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CategoryExists(long id)
        {
            return _dbContext.Categories.Any(c => c.Id == id);
        }

        public bool CreateCategory(Category category)
        {
            _dbContext.Categories.Add(category);
            return Save();
        }

        public bool DeleteCategory(Category category)
        {
            _dbContext.Categories.Remove(category);
            return Save();
        }

        public ICollection<Category> GetCategories()
        {
            return _dbContext.Categories.ToList();
        }

        public Category? GetCategory(long id)
        {
            return _dbContext.Categories.Where(e => e.Id == id).FirstOrDefault();
        }

        public ICollection<Dish> GetDishesByCategory(long categoryId)
        {
            return _dbContext.Dishes.Where(e => e.CategoryId == categoryId).ToList();
        }

        public bool Save()
        {
            var saved = _dbContext.SaveChanges();
            return saved > 0;
        }

        public bool UpdateCategory(Category category)
        {
            _dbContext.Update(category);
            return Save();
        }
    }
}
