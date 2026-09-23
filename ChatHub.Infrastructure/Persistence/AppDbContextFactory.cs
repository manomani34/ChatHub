using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatHub.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=ChatHubDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new AppDbContext(optionsBuilder.Options);
    }
}