using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using hikvision_emp_card_face_telegram_bot.Repository;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public EmployeeDTO CreateBotUser(long chatId)
        {
            Employee newEmployee = new Employee();
            newEmployee.TelegramChatId = chatId.ToString();
            newEmployee.PositionEmp = Employee.Position.EMPLOYEE;

            bool result = _employeeRepository.CreateEmployee(newEmployee);
            if(result)
            {
                return _mapper.Map<EmployeeDTO>(newEmployee);
            }
            
            return null;
        }

        public EmployeeDTO FindByChatID(long chatId)
        {
            var employee = _employeeRepository.FindByTelegramChatId(chatId);

            return employee?.ToDTO();
        }
    }
}
