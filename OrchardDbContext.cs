using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
            {
                // Use GetAwaiter().GetResult() to propagate an exception rather than wrapping them in an AggregateException.
                SetMappingsAsync(modelBuilder).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OrchardContext.OnModelCreating");
            }
        }

        private async Task SetMappingsAsync(ModelBuilder modelBuilder)
        {
            // Set ConfigureAwait(false) to complete a method after a task is complete without resuming a web context. 
            // https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html#preventing-the-deadlock
            var enableExtensions = await shellFeaturesManager.GetEnabledExtensionsAsync().ConfigureAwait(false);
            var enableModuls = enableExtensions.Where(x => x.Manifest.Type.ToUpperInvariant() == "MODULE");
            var assemblies = enableModuls.Select(m => Assembly.Load(m.Id));
            foreach (var assembly in assemblies)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }
        }
    }
}

