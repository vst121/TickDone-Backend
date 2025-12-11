namespace TickDone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        var provider = config["Database:Provider"];

        if (provider == "SqlServer")
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("SqlServer")));
        }
        else if (provider == "Sqlite")
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite(config.GetConnectionString("Sqlite")));
        }
        else
        {
            throw new Exception("Unknown database provider");
        }

        return services;
    }
}