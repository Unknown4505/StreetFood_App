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
using Plugin.Maui.Audio;

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

        // Configure Fonts
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        // 1. Đăng ký các Service chung
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<LocationService>();
        builder.Services.AddSingleton(AudioManager.Current);

        // =================================================================
        // [QUAN TRỌNG - PHẦN CÒN THIẾU]
        // Đăng ký Service chạy ngầm cho Android (Dependency Injection)
        // Dòng này giúp MainPage tìm thấy AndroidBackgroundService mà không bị Null
        // =================================================================
#if ANDROID
        builder.Services.AddSingleton<IBackgroundService, StreetFood_App.Platforms.Android.AndroidBackgroundService>();
#endif
        // =================================================================

        // 2. Đăng ký ViewModels & Pages
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