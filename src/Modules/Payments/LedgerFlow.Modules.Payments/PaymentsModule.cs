using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Modules.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        return services;
    }
}
