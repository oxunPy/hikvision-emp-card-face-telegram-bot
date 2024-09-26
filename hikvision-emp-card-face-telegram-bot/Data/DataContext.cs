using hikvision_emp_card_face_telegram_bot.Data.Report;
using hikvision_emp_card_face_telegram_bot.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace hikvision_emp_card_face_telegram_bot.Data
{
    public class DataContext : DbContext
    {

        protected readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // DbSets for the entities
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LunchMenu> LunchMenus { get; set; }
        public DbSet<SelectedMenu> SelectedMenus { get; set; }  
        public DbSet<TerminalConfiguration> TerminalConfigurations { get; set; }



        // Configuring relationship and database settings
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SelectedMenu>()
                .HasOne(s => s.Dish)
                .WithMany()
                .HasForeignKey(s => s.DishId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SelectedMenu>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.TelegramChatId)
                .IsUnique(true);

            modelBuilder.Entity<LunchMenu>()
                .HasIndex(e => e.DayOfWeek)
                .IsUnique(true);

            modelBuilder.Entity<LunchMenu>()
                .Property(l => l.DishIds)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v), // Serialize List<long> to JSON string
                    v => JsonConvert.DeserializeObject<List<long>>(v)) // Deserialize JSON string back to List<long>
                .HasColumnType("jsonb");

            // Configuring SelectedMenuReport as a query type (not mapped to a table)
            modelBuilder.Entity<SelectedMenuReport>().HasNoKey().ToView(null); // This ensures it's not treated as a table
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            options.UseNpgsql(_configuration.GetConnectionString("PostgreSqlConnection")).EnableDetailedErrors();
        }

    }
}
