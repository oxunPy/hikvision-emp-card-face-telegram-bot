using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Entity;
using hikvision_emp_card_face_telegram_bot.Interfaces;

namespace hikvision_emp_card_face_telegram_bot.Repository
{
    public class TerminalConfigurationRepository : ITerminalConfigurationRepository
    {
        private DataContext _dbContext;

        public TerminalConfigurationRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CreateTerminalConfiguration(TerminalConfiguration terminalConfiguration)
        {
            _dbContext.TerminalConfigurations.Add(terminalConfiguration);
            return Save();
        }

        public bool DeleteTerminalConfiguration(TerminalConfiguration terminalConfiguration)
        {
            _dbContext.TerminalConfigurations.Remove(terminalConfiguration);
            return Save();
        }

        public TerminalConfiguration? GetTerminalConfiguration(long id)
        {
            return _dbContext.TerminalConfigurations.Find(id);
        }

        public ICollection<TerminalConfiguration> GetTerminalConfigurations()
        {
            return _dbContext.TerminalConfigurations.ToList();
        }

        public bool Save()
        {
            var saved = _dbContext.SaveChanges();
            return saved > 0;
        }

        public bool TerminalConfigurationExists(long id)
        {
            return _dbContext.TerminalConfigurations.Any(t => t.Id == id);
        }

        public bool UpdateTerminalConfiguration(TerminalConfiguration terminalConfiguration)
        {
            _dbContext.TerminalConfigurations.Update(terminalConfiguration);
            return Save();  
        }
    }
}
