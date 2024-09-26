using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Data.Report;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.SqlClient;

namespace hikvision_emp_card_face_telegram_bot.Repository
{
    public class SelectedMenuRepository : ISelectedMenuRepository
    {
        private DataContext _dbContext;

        public SelectedMenuRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }   

        public bool CreateSelectedMenu(SelectedMenu selectedMenu)
        {
            _dbContext.SelectedMenus.Add(selectedMenu);
            return Save();
        }

        public bool DeleteSelectedMenu(SelectedMenu selectedMenu)
        {
            _dbContext.SelectedMenus.Remove(selectedMenu);
            return Save();
        }

        public ICollection<SelectedMenu> GetSelectedMenus()
        {
            return _dbContext.SelectedMenus.ToList();
        }

        public SelectedMenu? GetSelectedMenus(long id)
        {
            return _dbContext.SelectedMenus.Where(s => s.Id == id).FirstOrDefault();
        }

        public bool Save()
        {
            var saved = _dbContext.SaveChanges();
            return saved > 0;
        }

        public bool SelectedMenuExists(long id)
        {
            return _dbContext.SelectedMenus.Any(s => s.Id == id);
        }

        public bool UpdateSelectedMenu(SelectedMenu selectedMenu)
        {
            _dbContext.SelectedMenus.Update(selectedMenu);
            return Save();
        }

        public async Task<ICollection<SelectedMenuReport?>> findTodaySelectedMenus(DateTime today)
        {
            var query = @"SELECT d.""Name"" as ""DishName"", COUNT(sm.""Id"") as Quantity, 
                             STRING_AGG(e.""FirstName"" || ' ' || e.""LastName"" || ' - ' || sm.""DiscountPrice"" || ' UZS', ', ') AS EmployeeNames
                      FROM ""SelectedMenus"" sm
                      JOIN ""Dishes"" d ON sm.""DishId"" = d.""Id""
                      JOIN ""Employees"" e ON sm.""EmployeeId"" = e.""Id""
                      WHERE sm.""Date"" = @today
                      GROUP BY d.""Name""";
            
            return await _dbContext.Set<SelectedMenuReport>().FromSqlRaw(query, new SqlParameter("today", today)).ToListAsync();
        }

        public SelectedMenu? GetTodaysSelectedMenuByEmployeeChatId(long chatId)
        {
            var query = @"select slm.*
                        from ""SelectedMenus"" slm
                        inner join (
                            select *
                            from ""Employees""
                            where ""TelegramChatId"" = @chatid
                        ) e on e.""Id"" = slm.""EmployeeId""
                        where cast(slm.""Date"" as date) = cast(now() as date)";
            return _dbContext.SelectedMenus.FromSqlRaw(query, new NpgsqlParameter("chatid", chatId.ToString())).FirstOrDefault();
        }
    }
}
