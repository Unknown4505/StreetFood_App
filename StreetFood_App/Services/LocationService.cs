namespace StreetFood_App.Services;

public class LocationService
{
    public async Task<Location> GetCurrentLocation()
    {
        try
        {
            // [TỐI ƯU PIN] Cấu hình lấy vị trí
            // - Accuracy.High: Đủ chính xác cho du lịch (10-20m), không tốn pin như Best
            // - Timeout 5s: Nếu 5s không bắt được thì bỏ qua, tránh treo GPS lâu
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));

            return await Geolocation.Default.GetLocationAsync(request);
        }
        catch (Exception)
        {
            // Nếu lỗi (tắt GPS, không có quyền...) thì trả về null để App xử lý sau
            return null;
        }
    }

    // Hàm tính khoảng cách giữ nguyên
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        return Location.CalculateDistance(lat1, lon1, lat2, lon2, DistanceUnits.Kilometers);
    }
}