using StreetFood_App.Services;
using StreetFood_App.Models;
using StreetFood_App.Pages;

namespace StreetFood_App;

public partial class AppShell : Shell
{
    private readonly LocationService _locationService;
    private readonly DatabaseService _dbService;

    // Timer để chạy vòng lặp ngầm (Geofence)
    private IDispatcherTimer _timer;
    private bool _isScanning = false;

    // [FIX SPAM] Danh sách ID các quán ĐÃ BÁO rồi -> Không báo lại nữa
    private List<int> _triggeredPois = new List<int>();

    public AppShell()
    {
        InitializeComponent();

        _locationService = new LocationService();
        _dbService = new DatabaseService();

        // Đăng ký định tuyến (Routing) cho các trang con
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(ScanPage), typeof(ScanPage));
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));

        // Bắt đầu chạy quét vị trí
        StartGeofencing();
    }

    private void StartGeofencing()
    {
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(10); // Quét mỗi 10 giây
        _timer.Tick += async (s, e) => await CheckProximity();
        _timer.Start();
    }

    private async Task CheckProximity()
    {
        // Nếu đang quét dở thì bỏ qua lượt này
        if (_isScanning) return;
        _isScanning = true;

        try
        {
            // 1. Lấy vị trí hiện tại
            var myLocation = await _locationService.GetCurrentLocation();
            if (myLocation == null) return;

            // 2. Lấy danh sách quán từ DB
            var pois = await _dbService.GetPOIsAsync();
            if (pois == null || pois.Count == 0) return;

            foreach (var poi in pois)
            {
                // [FIX SPAM] Nếu quán này đã báo rồi thì bỏ qua ngay
                if (_triggeredPois.Contains(poi.Id)) continue;

                // 3. Tính khoảng cách
                double distanceKm = _locationService.CalculateDistance(
                    myLocation.Latitude, myLocation.Longitude,
                    poi.Latitude, poi.Longitude);

                double distanceMeters = distanceKm * 1000;

                // 4. Nếu khoảng cách < 30 mét (đã đến nơi)
                if (distanceMeters < 30)
                {
                    // [FIX SPAM] Đánh dấu là đã báo -> Lần sau quét sẽ bỏ qua
                    _triggeredPois.Add(poi.Id);

                    // Rung điện thoại báo hiệu
                    try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

                    // Hiện thông báo hỏi người dùng
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        bool answer = await DisplayAlert("📍 Đã đến nơi!",
                            $"Bạn đang đứng trước \"{poi.Name}\". \nBạn có muốn nghe thuyết minh không?",
                            "Nghe luôn", "Để sau");

                        if (answer)
                        {
                            var navParam = new Dictionary<string, object>
                            {
                                { "SelectedPoi", poi },
                                { "AutoPlay", true } // Bật cờ tự động đọc
                            };

                            // Chuyển sang trang chi tiết
                            await Current.GoToAsync(nameof(DetailPage), navParam);
                        }
                    });

                    // Đã tìm thấy 1 quán gần nhất thì break vòng lặp (tránh báo 2 quán cùng lúc)
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geofence Error: {ex.Message}");
        }
        finally
        {
            _isScanning = false;
        }
    }
}