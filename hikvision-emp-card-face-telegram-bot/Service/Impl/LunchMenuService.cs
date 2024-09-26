using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class LunchMenuService : ILunchMenuService
    {

        private readonly ILunchMenuRepository _lunchRepository;
        private readonly IMapper _mapper;

        public LunchMenuService(ILunchMenuRepository lunchRepository, IMapper mapper)
        {
            _lunchRepository = lunchRepository;
            _mapper = mapper;
        }

        public LunchMenuDTO GetTodaysMenu()
        {
            LunchMenu entity = _lunchRepository.GetTodayLunchMenu();
            if(entity == null) return new LunchMenuDTO() { DayOfWeek = DateTime.Now.DayOfWeek };

            return _mapper.Map<LunchMenuDTO>(entity);
        }

        public LunchMenuDTO GetCurrentEditMenu()
        {
            return _mapper.Map<LunchMenuDTO>(_lunchRepository.GetCurrentEditMenu());
        }

        public LunchMenuDTO SetCurrentEdit(DayOfWeek dayOfWeek)
        {
            return _mapper.Map<LunchMenuDTO>(_lunchRepository.UpdateLunchMenuForCurrentEdit(dayOfWeek));
        }

        public bool UpdateTodaysLunchMenu(LunchMenuDTO lunchMenu)
        {
            if(lunchMenu == null) return false;

            LunchMenu entity = _lunchRepository.GetTodayLunchMenu();
            if(entity == null)
            {
                entity = new LunchMenu();
                entity.DishIds = lunchMenu.DishIds;
                entity.DayOfWeek = DateTime.Now.DayOfWeek;
                return _lunchRepository.CreateLunchMenu(entity);
            }

            entity.DishIds = lunchMenu.DishIds;
            entity.DayOfWeek = DateTime.Now.DayOfWeek;
            entity.Id = lunchMenu.Id;
            return _lunchRepository.UpdateLunchMenu(entity);
        }

        public bool UpdateLunchMenu(LunchMenuDTO lunchMenu)
        {
            if(lunchMenu == null || lunchMenu.Id == null || lunchMenu.Id <= 0) return false;

            LunchMenu entity = _lunchRepository.GetLunchMenu(lunchMenu.Id);
            if(entity == null) return false;

            entity.Id = lunchMenu.Id;
            entity.DishIds = lunchMenu.DishIds;
            entity.DayOfWeek = lunchMenu.DayOfWeek;
            return _lunchRepository.UpdateLunchMenu(entity);
        }
    }
}
