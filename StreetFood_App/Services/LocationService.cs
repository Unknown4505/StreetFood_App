using Microsoft.Maui.Devices.Sensors;

namespace StreetFood_App.Services;

public class LocationService
{
    // 1. Hàm lấy vị trí hiện tại (Giữ nguyên logic cũ nhưng gọn hơn)
    public async Task<Location> GetCurrentLocation()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted) return null;

            // Lấy vị trí nhanh (LastKnown) trước
            var location = await Geolocation.Default.GetLastKnownLocationAsync();

            // Nếu không có, mới bắt buộc lấy vị trí chính xác (tốn pin hơn chút)
            if (location == null)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                location = await Geolocation.Default.GetLocationAsync(request);
            }
            return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi GPS: {ex.Message}");
            return null;
        }
    }

    // 2. [MỚI] Hàm tính khoảng cách giữa 2 điểm (Trả về ki-lô-mét)
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var location1 = new Location(lat1, lon1);
        var location2 = new Location(lat2, lon2);

        // Dùng hàm có sẵn của MAUI để tính cho chính xác nhất
        return Location.CalculateDistance(location1, location2, DistanceUnits.Kilometers);
    }
}