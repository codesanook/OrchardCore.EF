using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace OrchardCore.EF.Filters
{
    public class TransactionActionServiceFilter : IAsyncActionFilter
    {
        private readonly OrchardDbContext dbContext;
        private readonly ILogger<TransactionActionServiceFilter> logger;

        public TransactionActionServiceFilter(OrchardDbContext dbContext, ILogger<TransactionActionServiceFilter> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var actionExecutedContext = await next();
                if (IsResultValid(context, actionExecutedContext))
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    await RollbackTransactionWithHandledExceptionAsync(transaction);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Tranaction commit throw exception in {nameof(TransactionActionServiceFilter)}");
                await RollbackTransactionWithHandledExceptionAsync(transaction);
            }
        }

        private async Task RollbackTransactionWithHandledExceptionAsync(IDbContextTransaction transaction)
        {
            try
            {
                // Attempt to roll back the transaction.
                await transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                // This catch block will handle any errors that may have occurred
                // on the server that would cause the rollback to fail, such as
                // a closed connection.
                logger.LogError(ex, $"Transaction rollback throw exception in {nameof(TransactionActionServiceFilter)}");
            }
        }

        private static bool IsResultValid(ActionExecutingContext context, ActionExecutedContext actionExecutedContext)
        {
            var noUnhandledException = actionExecutedContext.Exception == null || actionExecutedContext.ExceptionHandled;
            return noUnhandledException && context.ModelState.IsValid;
        }
    }
}
