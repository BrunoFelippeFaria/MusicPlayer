using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionHelper
{
    public static IServiceCollection ConfigureServices (this IServiceCollection services)
    {
        // Controllers
        services.AddSingleton<IMainWindowController, MainWindowController>();

        // Services
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<IAudioPlayerService, WindowsAudioPlayer>();        
        }

        // Janelas
        services.AddSingleton<MainWindow>();
        return services;
    }
    
}