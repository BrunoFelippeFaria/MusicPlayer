using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using MusicPlayer.Controllers;
using MusicPlayer.Interfaces;
using MusicPlayer.Ui;

namespace MusicPlayer.Helpers;

public static class DependencyInjectionHelper
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
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