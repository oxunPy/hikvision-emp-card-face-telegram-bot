using hikvision_emp_card_face_telegram_bot.bot.State;
using hikvision_emp_card_face_telegram_bot.Dto;
using hikvision_emp_card_face_telegram_bot.Service.Impl;

namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface IEmployeeService
    {
        EmployeeDTO CreateBotUser(long chatId);

        EmployeeService.CodeResultRegistration? FindByChatID(long chatId);

        bool UpdateByChatID(long chatId, RegistrationStates state, EmployeeDTO dto);
    }
}
