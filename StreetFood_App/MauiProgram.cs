using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui; // keep this for compile-time APIs
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using StreetFood_App.Services;
using StreetFood_App.ViewModels;
using StreetFood_App.Pages;
using System;
using System.Diagnostics;

namespace StreetFood_App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        // Guard around CommunityToolkit registration to prevent startup crash if toolkit assemblies mismatch at runtime
        try
        {
            builder.UseMauiCommunityToolkit();
        }
        catch (TypeLoadException tle)
        {
            Debug.WriteLine("CommunityToolkit registration failed: " + tle.Message);
            Debug.WriteLine("Action: Ensure CommunityToolkit.Maui and CommunityToolkit.Maui.Core package versions match, then Clean/Rebuild and redeploy.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Unexpected error registering CommunityToolkit: " + ex);
        }

        // .UseMauiMaps() // <-- Kích hoạt bản đồ
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        // Đăng ký Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<LocationService>();

        // Đăng ký ViewModels & Pages
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<MapPage>();

        builder.Services.AddTransient<DetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}