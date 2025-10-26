using Gtk;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    public static void Main()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.ConfigureServices();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        Application.Init();

        serviceProvider.GetRequiredService<MainWindow>();

        Application.Run();
    }
}