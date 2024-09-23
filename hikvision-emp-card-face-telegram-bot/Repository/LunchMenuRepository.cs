using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Repository
{
    public class LunchMenuRepository : ILunchMenuRepository
    {
        private DataContext _dbContext;

        public LunchMenuRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CreateLunchMenu(LunchMenu lunchMenu)
        {
            _dbContext.LunchMenus.Add(lunchMenu);
            return Save();
        }

        public bool DeleteLunchMenu(LunchMenu lunchMenu)
        {
            _dbContext.LunchMenus.Remove(lunchMenu);
            return Save();
        }

        public LunchMenu? GetLunchMenu(long id)
        {
            return _dbContext.LunchMenus.FirstOrDefault(l => l.Id == id);
        }

        public ICollection<LunchMenu> GetLunchMenus()
        {
            return _dbContext.LunchMenus.ToList();
        }

        public bool LunchMenuExists(long id)
        {
            return _dbContext.LunchMenus.Where(l => l.Id == id).Any();
        }

        public bool Save()
        {
            var saved = _dbContext.SaveChanges();
            return saved > 0;
        }

        public bool UpdateLunchMenu(LunchMenu lunchMenu)
        {
            _dbContext.Update(lunchMenu);
            return Save();
        }
    }
}
