using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace StreetFood_App.Services;

public class LocationService
{
    // Đã xóa dòng gây lỗi
    public async Task<Location> GetCurrentLocation()
    {
        try
        {
            // 1. Kiểm tra xem đã có quyền chưa
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            // 2. Nếu chưa có thì hiện Popup xin quyền
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            // 3. Nếu người dùng từ chối -> Trả về null hoặc ném lỗi
            if (status != PermissionStatus.Granted)
                return null; // Hoặc hiện thông báo "Bạn cần bật GPS"

            // 4. Lấy vị trí (Code cũ của bạn)
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            }

            return location;
        }
        catch (Exception ex)
        {
            // Xử lý lỗi (ví dụ chưa bật GPS trên thanh thông báo)
            System.Diagnostics.Debug.WriteLine($"Lỗi GPS: {ex.Message}");
            return null;
        }
    }
}