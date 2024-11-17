using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Data.Report;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class SelectedMenuService : ISelectedMenuService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ISelectedMenuRepository _selectedMenuRepository;
        private readonly IDishRepository _dishRepository;
        private readonly IMapper _mapper;

        public SelectedMenuService(IEmployeeRepository employeeRepository, ISelectedMenuRepository selectedMenuRepository, IDishRepository dishRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _selectedMenuRepository = selectedMenuRepository;
            _dishRepository = dishRepository;
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

                if(empByChatID.PositionEmp == Employee.Position.EMPLOYEE)
                {
                    // discount calculation
                    DateTime visitedDate = empByChatID.VisitedDate == null ? DateTime.Now : (DateTime)empByChatID.VisitedDate;
                    DateTime time70 = DateTime.Now.Date.AddHours(9).AddMinutes(30);
                    DateTime time50 = DateTime.Now.Date.AddHours(10);

                    if (visitedDate < time70)
                        entity.DiscountPercent = 70;
                    else if (visitedDate < time50)
                        entity.DiscountPercent = 50;
                    else
                        entity.DiscountPercent = 30;
                }
                

                return _selectedMenuRepository.CreateSelectedMenu(entity);
            }

            entity.DishId = mealId;
            entity.EmployeeId = empByChatID.Id;
            entity.Date = DateTime.Now.Date.ToUniversalTime();
            return _selectedMenuRepository.UpdateSelectedMenu(entity);
        }

        public bool DeleteMyOrder(long chatId)
        {
            SelectedMenu? myOrder = _selectedMenuRepository.GetTodaysSelectedMenuByEmployeeChatId(chatId);
            if(myOrder != null)
            {
                return _selectedMenuRepository.DeleteSelectedMenu(myOrder);
            }
            return false;
        }

        public Task<ICollection<SelectedMenuReport>> DailyReportForManager()
        {
            return _selectedMenuRepository.findTodaySelectedMenusReportDaily(DateTime.Now.Date.ToUniversalTime());
        }

        public bool HasEmployeeSelectedMealToday(long chatId)
        {
            SelectedMenu entity = _selectedMenuRepository.GetTodaysSelectedMenuByEmployeeChatId(chatId);
            return entity != null && entity.EmployeeId != null && entity.DishId != null;
        }
    }
}
