using hikvision_emp_card_face_telegram_bot.Data.Report;
using hikvision_emp_card_face_telegram_bot.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LunchMenu> LunchMenus { get; set; }
        public DbSet<SelectedMenu> SelectedMenus { get; set; }  
        public DbSet<TerminalConfiguration> TerminalConfigurations { get; set; }



        // Configuring relationship and database settings
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Configuring the one-to-many relationship between Category and Dish
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Category)                // One dish has one Category
                .WithMany(c => c.Dishes)                // One category has many Dishes
                .HasForeignKey(d => d.CategoryId)       // Foreign key in Dish table
                .OnDelete(DeleteBehavior.SetNull);      // Specify delete behaviour

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

            // Configuring SelectedMenuReport as a query type (not mapped to a table)
            modelBuilder.Entity<SelectedMenuReport>().HasNoKey().ToView(null); // This ensures it's not treated as a table
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            options.UseNpgsql(_configuration.GetConnectionString("PostgreSqlConnection"));
        }

    }
}
