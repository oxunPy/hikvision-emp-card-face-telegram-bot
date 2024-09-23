using hikvision_emp_card_face_telegram_bot.Entity;

namespace hikvision_emp_card_face_telegram_bot.Interfaces
{
    public interface ITerminalConfigurationRepository
    {
        ICollection<TerminalConfiguration> GetTerminalConfigurations();

        TerminalConfiguration? GetTerminalConfiguration(long id);

        bool TerminalConfigurationExists(long id);

        bool CreateTerminalConfiguration(TerminalConfiguration terminalConfiguration);

        bool UpdateTerminalConfiguration(TerminalConfiguration terminalConfiguration);

        bool DeleteTerminalConfiguration(TerminalConfiguration terminalConfiguration);

        bool Save();
    }
}
