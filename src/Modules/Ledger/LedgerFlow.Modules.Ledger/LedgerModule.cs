using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Modules.Ledger;

public static class LedgerModule
{
    public static IServiceCollection AddLedgerModule(this IServiceCollection services)
    {
        return services;
    }
}
