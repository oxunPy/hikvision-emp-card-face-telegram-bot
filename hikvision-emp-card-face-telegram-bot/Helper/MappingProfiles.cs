using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Category, CategoryDTO>();
            CreateMap<Dish, DishDTO>();
            CreateMap<Employee, EmployeeDTO>();
            CreateMap<LunchMenu, LunchMenuDTO>();
            CreateMap<SelectedMenu, SelectedMenuDTO>();
            CreateMap<TerminalConfiguration, TerminalConfigurationDTO>();
        }
    }
}
