using ZXing.Net.Maui;
using StreetFood_App.Services;
using StreetFood_App.Models;

namespace StreetFood_App.Pages;

public partial class ScanPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private bool _isScanning = true;

    public ScanPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;

        MyCamera.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false
        };
    }

    // Sự kiện của Camera thật
    private void Camera_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (!_isScanning) return;

        var first = e.Results?.FirstOrDefault();
        if (first is null) return;

        // Gọi hàm xử lý chung
        ProcessQrContent(first.Value);
    }

    // [MỚI] Sự kiện của Nút Test (Cheat)
    private void OnFakeScanClicked(object sender, EventArgs e)
    {
        if (!_isScanning) return;

        // Giả vờ như vừa quét được mã "STREETFOOD"
        ProcessQrContent("STREETFOOD");
    }

    // --- HÀM XỬ LÝ TRUNG TÂM (Dùng chung cho cả 2 cách) ---
    private void ProcessQrContent(string qrContent)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _isScanning = false; // Dừng quét để xử lý
            try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

            // =========================================================
            // CASE 1: QUÉT MÃ CỔNG CHÀO (ACTIVE)
            // =========================================================

            // Logic: Hiện thông báo rồi về trang chủ
            await DisplayAlert("Xin chào!", "Chào mừng bạn đến với Phố Ẩm Thực Vĩnh Khánh!", "Bắt đầu khám phá");
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            return;

            /* =========================================================
               CASE 2: QUÉT MÃ TỪNG QUÁN (TẠM ĐÓNG)
               (Khi nào cần test từng quán thì mở ra và sửa chữ "STREETFOOD" ở trên thành tên quán)
               =========================================================
            
            var allPois = await _dbService.GetPOIsAsync();
            var foundPoi = allPois.FirstOrDefault(p => p.Name.ToLower().Contains(qrContent.ToLower()) || 
                                                       p.Id.ToString() == qrContent);

            if (foundPoi != null)
            {
                var navParam = new Dictionary<string, object> 
                { 
                    { "SelectedPoi", foundPoi },
                    { "AutoPlay", true } 
                };
                await Shell.Current.GoToAsync(nameof(DetailPage), navParam);
            }
            else
            {
                bool retry = await DisplayAlert("Lỗi", $"Không tìm thấy: {qrContent}", "Thử lại", "Về trang chủ");
                if (retry) _isScanning = true;
                else await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            ========================================================= */
        });
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }
}