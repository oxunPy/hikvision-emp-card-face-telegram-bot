namespace hikvision_emp_card_face_telegram_bot.Service
{
    public interface ITerminalConfigurationService
    {
        void AutoLogin(string username, string password);
    }
}
