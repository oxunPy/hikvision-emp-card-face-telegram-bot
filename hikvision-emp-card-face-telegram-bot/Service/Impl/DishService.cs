using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using System.Collections.Generic;

namespace hikvision_emp_card_face_telegram_bot.Service.Impl
{
    public class DishService : IDishService
    {
        private readonly IDishRepository dishRepository;

        public ICollection<DishDTO> GetDishByTodaysMenu()
        {
            throw new NotImplementedException();
        }
    }
}
