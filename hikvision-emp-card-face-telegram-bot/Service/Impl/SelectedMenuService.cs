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
        private readonly IConfiguration _configuration;

        public SelectedMenuService(IEmployeeRepository employeeRepository, ISelectedMenuRepository selectedMenuRepository, IDishRepository dishRepository, IMapper mapper, IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _selectedMenuRepository = selectedMenuRepository;
            _dishRepository = dishRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public bool CreateOrUpdateSelectedMenuIfDeletedMeal(long chatId, long mealId)
        {
            Employee empByChatID = _employeeRepository.FindByTelegramChatId(chatId);
            if (empByChatID == null) return false;

            SelectedMenu entity = _selectedMenuRepository.GetTodaysSelectedMenuByEmployeeChatId(chatId);
            Dish mealEntity = _dishRepository.GetDish(mealId);
            
            if(mealEntity == null)
            {
                return false;
            }

            if(entity == null)
            {
                entity = new SelectedMenu();
                entity.DishId = mealId;
                entity.DishName = mealEntity.Name;
                entity.DishPrice = mealEntity.Price;
                entity.EmployeeId = empByChatID.Id;
                entity.Date = DateTime.Now.ToUniversalTime();


                // discount calculation
                DateTime visitedDate = empByChatID.VisitedDate == null ? DateTime.Now : (DateTime)empByChatID.VisitedDate;
                DateTime time70 = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("LunchTime:StartHour")).AddMinutes(_configuration.GetValue<int>("LunchTime:ShortenDiscountTime"));
                DateTime time50 = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("LunchTime:EndHour"));
                DateTime time30 = DateTime.Now.Date.AddHours(_configuration.GetValue<int>("LunchTime:EndHour")).AddMinutes(_configuration.GetValue<int>("LunchTime:ShortenDiscountTime"));

                int biggestDiscount = _configuration.GetValue<int>("LunchTime:TheBiggestDiscount");
                int shortenDiscountPercent = _configuration.GetValue<int>("LunchTime:ShortenDiscountPercent");

                if (visitedDate < time70)
                {
                    entity.DiscountPercent = biggestDiscount;
                    entity.DiscountPrice = mealEntity.Price * biggestDiscount / 100;
                }
                else if (visitedDate < time50)
                {
                    entity.DiscountPercent = biggestDiscount - shortenDiscountPercent;
                    entity.DiscountPrice = mealEntity.Price * (biggestDiscount - shortenDiscountPercent)  /100;
                }
                else if (visitedDate < time30)
                {
                    entity.DiscountPercent = biggestDiscount - 2 * shortenDiscountPercent;
                    entity.DiscountPrice = mealEntity.Price * (biggestDiscount - 2 * shortenDiscountPercent) / 100;
                }

                return _selectedMenuRepository.CreateSelectedMenu(entity);
            }

            entity.DishId = mealId;
            entity.DishName = mealEntity.Name;
            entity.DishPrice = mealEntity.Price;
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

        public Task<ICollection<SelectedMenuReportInMonth>> MonthlyReportForManager()
        {
            return _selectedMenuRepository.findInMonthSelectedMenusReport(DateTime.Now.AddDays(-30).Date.ToUniversalTime());
        }

        public bool HasEmployeeSelectedMealToday(long chatId)
        {
            SelectedMenu entity = _selectedMenuRepository.GetTodaysSelectedMenuByEmployeeChatId(chatId);
            return entity != null && entity.EmployeeId != null && entity.DishId != null;
        }
    }
}
