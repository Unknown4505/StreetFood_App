using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace StreetFood_App;

// 1. Khai báo Activity chính (Chỉ được có 1 cái này thôi)
[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          LaunchMode = LaunchMode.SingleTop, // Quan trọng: Để không mở nhiều App chồng lên nhau
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

// 2. Khai báo Deep Link (Nằm ngay trên class)
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "streetfood",
    DataHost = "app")]

// 3. Class chính
public class MainActivity : MauiAppCompatActivity
{
    // Bên trong thường không cần viết gì thêm cho phần này
}