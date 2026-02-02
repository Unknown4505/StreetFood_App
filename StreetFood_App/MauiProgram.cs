using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using StreetFood_App.Services;
using StreetFood_App.ViewModels;
using StreetFood_App.Pages;
using StreetFood_App.Models;
using System;
using System.Diagnostics;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace StreetFood_App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseBarcodeReader()
            .UseMauiCommunityToolkit();


        // (Các đoạn configure fonts giữ nguyên)
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        // Đăng ký Services
        builder.Services.AddSingleton<DatabaseService>();
        

        builder.Services.AddSingleton<LocationService>();

        // Đăng ký ViewModels & Pages
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<MapPage>();

        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<ScanPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}