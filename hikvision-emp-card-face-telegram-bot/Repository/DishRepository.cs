using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hikvision_emp_card_face_telegram_bot.Repository
{
    public class DishRepository : IDishRepository
    {
        private DataContext _dbContext;
        
        public DishRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CreateDish(Dish dish)
        {
            _dbContext.Dishes.Add(dish);
            return Save();
        }

        public bool DeleteDish(Dish dish)
        {
            _dbContext.Dishes.Remove(dish); 
            return Save();
        }

        public bool DishExists(long id)
        {
            return _dbContext.Dishes.Any(dish => dish.Id == id);
        }

        public Dish? GetDish(long id)
        {
            return _dbContext.Dishes.FirstOrDefault(dish => dish.Id == id);
        }

        public ICollection<Dish> GetDishes()
        {
            return _dbContext.Dishes.ToList();
        }

        public ICollection<Dish> GetDishesByCategory(long categoryId)
        {
            return _dbContext.Dishes.Where(dish => dish.CategoryId == categoryId).ToList();  
        }

        public bool UpdateDish(Dish dish)
        {
            _dbContext.Update(dish);
            return Save();
        }

        public ICollection<Dish> GetDishesByTodaysMenu()
        {
            return _dbContext.Dishes.FromSqlRaw<Dish>("").ToList();
        }

        public bool Save()
        {
            var saved = _dbContext.SaveChanges();
            return saved > 0;
        }

        public ICollection<Dish> GetDishByTodaysMenu()
        {
            throw new NotImplementedException();
        }
    }
}
