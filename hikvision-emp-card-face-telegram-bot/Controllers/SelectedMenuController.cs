using hikvision_emp_card_face_telegram_bot.Data.Report;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hikvision_emp_card_face_telegram_bot.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class SelectedMenuController : ControllerBase
    {

        private ISelectedMenuRepository _selectedMenuRepository;

        public SelectedMenuController(ISelectedMenuRepository selectedMenuRepository)
        {
            _selectedMenuRepository = selectedMenuRepository;
        }

        [HttpGet]
        public async Task<ICollection<SelectedMenuReport>> getSelectedMenuReport([FromQuery(Name = "date")] string date)
        {
            return await _selectedMenuRepository.findTodaySelectedMenus(DateTime.Now.Date.ToUniversalTime());
        }
    }
}
