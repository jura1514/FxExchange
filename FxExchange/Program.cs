using FxExchange.Configuration;
using FxExchange.Services;
using FxExchange.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FxExchange;

public static class Program
{
    public static void Main()
    {
        var host = CreateHostBuilder().Build();
        var exchangeService = host.Services.GetRequiredService<IExchangeService>();

        ConsoleKey? exitKey = null;
        while (exitKey != ConsoleKey.Q)
        {
            try
            {
                exchangeService.ProcessCurrencyExchange();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Please try again.");
            }

            Console.WriteLine("Press any key to continue or 'Q' to quit.");
            exitKey = Console.ReadKey(true).Key;
        }
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.Configure<ExchangeRateConfiguration>(
                    context.Configuration.GetSection(ExchangeRateConfiguration.SectionName));

                services.AddScoped<IConsoleHelper, ConsoleHelper>();
                services.AddScoped<IExchangeRateProvider, StaticExchangeRateProvider>();
                services.AddScoped<IUserInterface, ConsoleUserInterface>();
                services.AddScoped<IExchangeService, StaticExchangeService>();
            });
}