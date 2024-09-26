using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class SelectedMenuService : ISelectedMenuService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ISelectedMenuRepository _selectedMenuRepository;
        private readonly IMapper _mapper;

        public SelectedMenuService(IEmployeeRepository employeeRepository, ISelectedMenuRepository selectedMenuRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _selectedMenuRepository = selectedMenuRepository;
            _mapper = mapper;
        }

        public bool CreateOrUpdateSelectedMenuIfDeletedMeal(long chatId, long mealId)
        {
            Employee empByChatID = _employeeRepository.FindByTelegramChatId(chatId);
            if (empByChatID == null) return false;

            SelectedMenu entity = _selectedMenuRepository.GetTodaysSelectedMenuByEmployeeChatId(chatId);
            if(entity == null)
            {
                entity = new SelectedMenu();
                entity.DishId = mealId;
                entity.EmployeeId = empByChatID.Id;
                entity.Date = DateTime.Now.ToUniversalTime();

                return _selectedMenuRepository.CreateSelectedMenu(entity);
            }

            entity.DishId = mealId;
            entity.EmployeeId = empByChatID.Id;
            entity.Date = DateTime.Now.Date.ToUniversalTime();
            return _selectedMenuRepository.UpdateSelectedMenu(entity);
        }

        public bool HasEmployeeSelectedMealToday(long chatId)
        {
            SelectedMenu entity = _selectedMenuRepository.GetTodaysSelectedMenuByEmployeeChatId(chatId);
            return entity != null && entity.EmployeeId != null && entity.DishId != null;
        }
    }
}
