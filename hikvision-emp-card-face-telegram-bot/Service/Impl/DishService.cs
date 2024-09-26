using AutoMapper;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using System.Collections.Generic;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class DishService : IDishService
    {
        private readonly IDishRepository _dishRepository;
        private readonly IMapper _mapper;

        public DishService(IDishRepository dishRepository, IMapper mapper)
        {
            _dishRepository = dishRepository;
            _mapper = mapper;
        }

        public DishDTO CreateNewDish(DishDTO dto)
        {
            Dish dish = new Dish();
            dish.Name = dto.Name;
            dish.ImagePath = dto.ImagePath;
            dish.Price = dto.Price;
            
            _dishRepository.CreateDish(dish);
            return _mapper.Map<DishDTO>(dish);
        }



        public DishDTO GetLatestDishByLunchMenu(long dishId)
        {
            return _mapper.Map<DishDTO>(_dishRepository.GetDish(dishId));
        }

        public ICollection<DishDTO> GetDishesByTodaysMenu()
        {
            throw new NotImplementedException();
        }

        public DishDTO UpdateDish(DishDTO dto)
        {
            if (dto == null || dto.Id == null || dto.Id <= 0)
                return null;


            Dish entityUpdate = _dishRepository.GetDish(dto.Id);
            entityUpdate.Id = dto.Id;
            entityUpdate.Name = dto.Name;
            entityUpdate.ImagePath = dto.ImagePath;
            entityUpdate.Price = dto.Price;

            _dishRepository.UpdateDish(entityUpdate);
            return _mapper.Map<DishDTO>(entityUpdate);
        }

        ICollection<DishDTO> IDishService.GetDishesByWeekDay(DayOfWeek dayOfWeek)
        {
            return _mapper.Map<List<DishDTO>>(_dishRepository.GetDishesByWeekDay(dayOfWeek));
        }

        public bool DeleteDishAndItsRelatingImg(long dishId)
        {
            Dish entity = _dishRepository.GetDish(dishId);
            if(entity == null) return false;

            string filePath = entity.ImagePath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return _dishRepository.DeleteDish(entity);
        }
    }
}
