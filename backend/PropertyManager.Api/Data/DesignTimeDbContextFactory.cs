using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PropertyManager.Api.Data
{
    // Design-time factory used by dotnet-ef so it can create the DbContext
    // without building the full application host (which may register hosted services
    // that depend on scoped services and cause design-time resolution errors).
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Read connection string from environment if provided (compose sets this),
            // otherwise fall back to the default used in appsettings.Development.json
            var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                             ?? "Host=postgres;Port=5432;Database=property_manager;Username=property;Password=propertypass";

            builder.UseNpgsql(connection);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
