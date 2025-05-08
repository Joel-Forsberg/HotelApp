using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HotelApp.Data
{
    public class HotelContextFactory : IDesignTimeDbContextFactory<HotelContext>
    {
        public HotelContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("json.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new HotelContext(optionsBuilder.Options);
        }
    }
}
