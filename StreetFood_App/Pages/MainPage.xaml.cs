using StreetFood_App.ViewModels;
using StreetFood_App.Models;
using StreetFood_App.Services;

namespace StreetFood_App.Pages;

public partial class MainPage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    // [MỚI 1] Biến lưu trữ "Chìa khóa vạn năng"
    private readonly IServiceProvider _serviceProvider;

    // [MỚI 2] Thêm IServiceProvider vào Constructor
    public MainPage(HomeViewModel vm, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _viewModel = vm;
        _serviceProvider = serviceProvider; // Lưu lại để dùng sau
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();

        // [MẸO] Đợi 2 giây cho UI vẽ xong và Android ghi nhận App đã Active
        await Task.Delay(5000);

        // Sau đó mới gọi Service
        await CheckPermissionsAndStartService();
    }

    private async Task CheckPermissionsAndStartService()
    {
        // Chỉ chạy trên Android
        if (DeviceInfo.Platform != DevicePlatform.Android) return;

        PermissionStatus statusNoti = PermissionStatus.Unknown;
        PermissionStatus statusLoc = PermissionStatus.Unknown;

        // A. Xin quyền Thông báo
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            statusNoti = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (statusNoti != PermissionStatus.Granted)
            {
                statusNoti = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }
        else
        {
            statusNoti = PermissionStatus.Granted;
        }
#endif

        // B. Xin quyền Vị trí
        statusLoc = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (statusLoc != PermissionStatus.Granted)
        {
            statusLoc = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        // C. BẮT ĐẦU BẬT SERVICE
        if (statusNoti == PermissionStatus.Granted && statusLoc == PermissionStatus.Granted)
        {
            try
            {
                // [THAY ĐỔI QUAN TRỌNG - AN TOÀN TUYỆT ĐỐI]
                // Dùng _serviceProvider lấy từ Constructor (Luôn luôn có)
                // Thay vì dùng Handler (Lúc có lúc không)
                var service = _serviceProvider.GetService<IBackgroundService>();

                if (service != null)
                {
                    service.Start();

                    // [DEBUG]
                    // await DisplayAlert("THÀNH CÔNG", "Service đã khởi động!", "OK");
                }
                else
                {
                    await DisplayAlert("LỖI", "Không tìm thấy Service (Provider trả về null)", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("CRASH", $"Lỗi khi gọi Start: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Thiếu quyền", "Bạn cần cấp quyền Vị trí và Thông báo.", "OK");
        }
    }

    // --- CÁC HÀM XỬ LÝ GIAO DIỆN KHÁC (GIỮ NGUYÊN) ---

    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        if (sender is SearchBar sb) sb.Unfocus();
        else MySearchBar.Unfocus();
    }

    private void OnBackgroundClicked(object sender, TappedEventArgs e)
    {
        if (MySearchBar.IsFocused) MySearchBar.Unfocus();
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ScanPage));
    }
}

