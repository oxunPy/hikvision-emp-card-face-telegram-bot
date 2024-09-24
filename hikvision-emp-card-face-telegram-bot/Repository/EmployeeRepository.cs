using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private DataContext _dbContext;

        public EmployeeRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CreateEmployee(Employee employee)
        {
            _dbContext.Employees.Add(employee);
            return Save();
        }

        public bool DeleteEmployee(Employee employee)
        {
            _dbContext.Employees.Remove(employee);
            return Save();
        }

        public bool EmployeeExists(long id)
        {
            return _dbContext.Employees.Any(x => x.Id == id);   
        }

        public Employee? GetEmployee(long id)
        {
            return _dbContext.Employees.Where(x => x.Id == id).FirstOrDefault();
        }

        public ICollection<Employee> GetEmployees()
        {
            return _dbContext.Employees.ToList();
        }

        public bool Save()
        {
            var saved = _dbContext.SaveChanges();
            return saved > 0;
        }

        public bool UpdateEmployee(Employee employee)
        {
            _dbContext.Update(employee);
            return Save();
        }
        public Employee FindByTelegramChatId(long chatId)
        {
            return _dbContext.Employees.Where(x => x.TelegramChatId == chatId.ToString()).FirstOrDefault();
        }
    }
}
