using StreetFood_App.Services;
using StreetFood_App.Models;
using StreetFood_App.Pages;

namespace StreetFood_App;

public partial class AppShell : Shell
{
    private readonly LocationService _locationService;
    private readonly DatabaseService _dbService;

    // Timer chạy trên luồng phụ
    private System.Timers.Timer _timer;
    private bool _isScanning = false;

    private List<int> _triggeredPois = new List<int>();

    // [THAY ĐỔI QUAN TRỌNG]
    // 1. AppShell không nên tự new DatabaseService
    // 2. Vì AppShell được tạo bằng new AppShell() trong App.xaml.cs,
    //    nên ta không thể Tiêm Constructor trực tiếp vào đây dễ dàng được.
    //    => Giải pháp: Dùng Handler.MauiContext.Services (Service Locator) để lấy DatabaseService chuẩn.
    public AppShell()
    {
        InitializeComponent();

        _locationService = new LocationService();

        // Đăng ký Routing
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(ScanPage), typeof(ScanPage));
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));

        // [TỐI ƯU] Bắt đầu quét sau khi giao diện đã load xong 1 chút
        // Để tránh tranh chấp tài nguyên lúc khởi động
        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(5), StartGeofencing);
    }

    private void StartGeofencing()
    {
        // Khởi đầu quét mỗi 10 giây
        _timer = new System.Timers.Timer(10000);
        _timer.Elapsed += async (s, e) => await CheckProximity();
        _timer.AutoReset = false;
        _timer.Start();
    }

    private async Task CheckProximity()
    {
        if (_isScanning) return;
        _isScanning = true;

        // Mặc định lần sau quét sau 10s
        double nextInterval = 10000;

        try
        {
            // [MỚI] Lấy DatabaseService từ kho chung (Singleton)
            // Đảm bảo dùng chung 1 kết nối với toàn bộ App
            var dbService = Handler?.MauiContext?.Services.GetService<DatabaseService>();

            // Nếu chưa lấy được service hoặc chưa có GPS -> Thử lại sau
            if (dbService == null)
            {
                RestartTimer(5000);
                return;
            }

            var myLocation = await _locationService.GetCurrentLocation();
            if (myLocation == null)
            {
                RestartTimer(5000); // Lỗi GPS -> Thử lại nhanh sau 5s
                return;
            }

            var pois = await dbService.GetPOIsAsync();
            if (pois == null || pois.Count == 0)
            {
                RestartTimer(10000);
                return;
            }

            // --- THUẬT TOÁN TỐI ƯU PIN (ADAPTIVE POLLING) ---
            double minDistance = double.MaxValue;

            foreach (var poi in pois)
            {
                // Tính khoảng cách
                double distanceKm = Location.CalculateDistance(
                    myLocation.Latitude, myLocation.Longitude,
                    poi.Latitude, poi.Longitude, DistanceUnits.Kilometers);
                double distanceMeters = distanceKm * 1000;

                if (distanceMeters < minDistance) minDistance = distanceMeters;

                // Logic báo hiệu (Đến gần < 30m và chưa báo lần nào)
                if (distanceMeters < 30 && !_triggeredPois.Contains(poi.Id))
                {
                    _triggeredPois.Add(poi.Id);

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

                        bool answer = await DisplayAlert("📍 Đã đến nơi!",
                            $"Bạn đang đứng trước \"{poi.Name}\". Nghe thuyết minh nhé?",
                            "Nghe luôn", "Để sau");

                        if (answer)
                        {
                            var navParam = new Dictionary<string, object>
                            {
                                { "SelectedPoi", poi }, { "AutoPlay", true }
                            };
                            await Current.GoToAsync(nameof(DetailPage), navParam);
                        }
                    });
                }
            }

            // --- QUYẾT ĐỊNH THỜI GIAN NGỦ ---
            if (minDistance > 2000)
                nextInterval = 60000; // Xa quá -> Ngủ 1 phút
            else if (minDistance > 500)
                nextInterval = 30000; // Xa vừa -> Ngủ 30s
            else
                nextInterval = 5000; // Đã vào vùng -> Quét gắt (5s/lần)
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geofence Error: {ex.Message}");
        }
        finally
        {
            _isScanning = false;
            RestartTimer(nextInterval);
        }
    }

    private void RestartTimer(double interval)
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Interval = interval;
            _timer.Start();
        }
    }
}