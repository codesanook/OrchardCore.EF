using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;

namespace OrchardCore.EF
{
    public class OrchardDbContext : DbContext
    {
        private readonly IShellFeaturesManager shellFeaturesManager;
        private readonly ILogger<OrchardDbContext> logger;

        public OrchardDbContext(
            DbContextOptions dbContextOption,
            IShellFeaturesManager shellFeaturesManager,
            ILogger<OrchardDbContext> logger
        ) : base(dbContextOption)
        {
            this.shellFeaturesManager = shellFeaturesManager;
            this.logger = logger;
        }

        protected override async void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
            {
                var enableExtensions = await shellFeaturesManager.GetEnabledExtensionsAsync();
                var enableModuls = enableExtensions.Where(x => x.Manifest.Type.ToUpperInvariant() == "MODULE");
                var assemblies = enableModuls.Select(m => Assembly.Load(m.Id));
                foreach (var assembly in assemblies)
                {
                    modelBuilder.ApplyConfigurationsFromAssembly(assembly);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OrchardContext.OnModelCreating");
            }
        }
    }
}

