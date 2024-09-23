using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Interfaces
{
    public interface IEmployeeRepository
    {
        ICollection<Employee> GetEmployees();

        Employee? GetEmployee(long id);

        bool EmployeeExists(long id);

        bool CreateEmployee(Employee employee);

        bool UpdateEmployee(Employee employee);

        bool DeleteEmployee(Employee employee);

        bool Save();
    }
}
