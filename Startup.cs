using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.EF
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _configuration;
        public Startup(IShellConfiguration configuration) => _configuration = configuration;

        // https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro?view=aspnetcore-3.1
        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            // TODO add support for more provider
            // var provider = _configuration.GetValue<string>("DatabaseProvider");
            //EF context object has a default life time scoped which is per a per-request lifetime.
            // https://github.com/dotnet/efcore/issues/4988
            services.AddDbContext<OrchardDbContext>(options =>
            {
                options.UseSqlServer(_configuration.GetValue<string>("ConnectionString"));
            });
        }
    }
}
