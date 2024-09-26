using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.SqlClient;

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

        public LunchMenu? GetTodayLunchMenu()
        {
            var query = @"SELECT *
                        FROM ""LunchMenus""
                        WHERE ""DayOfWeek"" = @dayofweek";

            return _dbContext.LunchMenus.FromSqlRaw(query, new NpgsqlParameter("dayofweek", (int)DateTime.Today.DayOfWeek)).FirstOrDefault();
        }

        public LunchMenu? GetCurrentEditMenu()
        {
            return _dbContext.LunchMenus.FirstOrDefault(l => l.CurrentEdit == true);
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

        public LunchMenu? UpdateLunchMenuForCurrentEdit(DayOfWeek dayOfWeek)
        {
            LunchMenu replaceCurrentEdit = _dbContext.LunchMenus.FirstOrDefault(l => l.CurrentEdit == true);
            if(replaceCurrentEdit != null && replaceCurrentEdit.DayOfWeek.Equals(dayOfWeek)) 
            {
                return replaceCurrentEdit;
            }
            
            if (replaceCurrentEdit != null && !replaceCurrentEdit.DayOfWeek.Equals(dayOfWeek))
            {
                replaceCurrentEdit.CurrentEdit = false;
                _dbContext.SaveChanges();
            }

            LunchMenu currentEdit = _dbContext.LunchMenus.FirstOrDefault(l => l.DayOfWeek == dayOfWeek);
            if (currentEdit == null)
            {
                currentEdit = new LunchMenu();
                currentEdit.DayOfWeek = dayOfWeek;
                currentEdit.CurrentEdit = true;
                _dbContext.LunchMenus.Add(currentEdit);
            }
            else
            {
                currentEdit.CurrentEdit = true;
                _dbContext.Update(currentEdit);
            }

            _dbContext.SaveChanges();
            return currentEdit;
        }
    }
}
