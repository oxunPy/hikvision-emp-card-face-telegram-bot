using hikvision_emp_card_face_telegram_bot.Dto;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface IEmployeeService
    {
        EmployeeDTO CreateBotUser(long chatId);

        EmployeeDTO FindByChatID(long chatId);
    }
}
