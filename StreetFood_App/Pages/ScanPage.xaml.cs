using ZXing.Net.Maui;
using StreetFood_App.Services;

namespace StreetFood_App.Pages;

public partial class ScanPage : ContentPage
{
    private bool _isScanning = true;

    // Không cần DatabaseService nữa vì không cần tra cứu quán
    public ScanPage()
    {
        InitializeComponent();

        MyCamera.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };
    }

    private void Camera_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (!_isScanning) return;
        var first = e.Results?.FirstOrDefault();
        if (first is null) return;

        // Quét trúng bất cứ mã gì cũng coi là Check-in thành công
        ProcessCheckIn();
    }

    private void OnFakeScanClicked(object sender, EventArgs e)
    {
        if (!_isScanning) return;

        // Giả vờ quét trúng
        ProcessCheckIn();
    }

    private void ProcessCheckIn()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _isScanning = false;
            try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

            // Logic đơn giản: Chào mừng và đưa vào trang chủ
            await DisplayAlert("🎉 Xin chào!",
                "Chào mừng bạn đến với Phố Ẩm Thực Vĩnh Khánh!\nHãy bắt đầu hành trình khám phá ẩm thực ngay nào.",
                "Bắt đầu đi thôi");

            // Chuyển về trang chủ (Reset ngăn xếp điều hướng)
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        });
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }
}