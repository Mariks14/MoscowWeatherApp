using Microsoft.EntityFrameworkCore;
using MoscowWeatherApp.Models.EFModels;

namespace MoscowWeatherApp.Models.EFContext
{
    public class ApplicationContext : DbContext
    {
        public DbSet<FileModel> FileModel { get; set; } = null!;
        public DbSet<Weather> Weather { get; set; } = null!;
        

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
        {
            //Database.EnsureDeleted();   // удаляем бд
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //
        //}
    }
}
