using AutoMapper;
using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using hikvision_emp_card_face_telegram_bot.Repository;
using System.Net.Http.Headers;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public enum CodeResultRegistration
        {
            FIRST_NAME,
            LAST_NAME,
            EMPLOYEE_POSITION,
            FACE_UPLOAD
        }

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public EmployeeDTO CreateBotUser(long chatId)
        {
            Employee newEmployee = new Employee();
            newEmployee.TelegramChatId = chatId.ToString();
            newEmployee.HikCardCode = chatId.ToString();

            bool result = _employeeRepository.CreateEmployee(newEmployee);
            if(result)
            {
                return _mapper.Map<EmployeeDTO>(newEmployee);
            }
            
            return null;
        }

        public CodeResultRegistration? FindByChatID(long chatId)
        {
            var employee = _employeeRepository.FindByTelegramChatId(chatId);
            
            if (employee == null)
            {
                return null;
            }
            else if (employee.FirstName == null)
            {
                return CodeResultRegistration.FIRST_NAME;
            }

            else if (employee.LastName == null)
            {
                return CodeResultRegistration.LAST_NAME;
            }

            else if (employee.PositionEmp == null)
            {
                return CodeResultRegistration.EMPLOYEE_POSITION;
            }
            else if (employee.FaceImagePath == null)
            {
                return CodeResultRegistration.FACE_UPLOAD;
            }

            return null;
        }

        public bool UpdateByChatID(long chatId, RegistrationStates state, EmployeeDTO dto)
        {
            if(state == null || dto == null)
                return false;

            Employee employee = _employeeRepository.FindByTelegramChatId(chatId);
            if(employee == null) return false;

            switch(state)
            {
                case RegistrationStates.FIRST_NAME:
                    employee.FirstName = dto.FirstName;
                    break;
                case RegistrationStates.LAST_NAME:
                    employee.LastName = dto.LastName;
                    break;
                case RegistrationStates.EMPLOYEE_POSITION:
                    employee.PositionEmp = dto.PositionEmp;
                    break;
                case RegistrationStates.FACE_UPLOAD:
                    employee.FaceImagePath = dto.FaceImagePath;
                    break;
            }

            return _employeeRepository.UpdateEmployee(employee);
        }
    }
}
