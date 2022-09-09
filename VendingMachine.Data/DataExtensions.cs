using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Data.DataAccess;
using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;

namespace VendingMachine.Data;

public static class DataExtensions
{
    public static void AddDataLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IEntityStore<User>, DbUserStore>();
        serviceCollection.AddSingleton<IEntityStore<Product>, DbProductStore>();
    }
}