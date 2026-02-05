#nullable disable
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using StreetFood_App.Services;
using Android.Runtime;
using Android.Content.PM;

namespace StreetFood_App.Platforms.Android;

// =========================================================================================
// CLASS 1: CẦU NỐI (BRIDGE)
// Nhiệm vụ: Nhận lệnh từ MainPage và chuyển tiếp cho Android Service
// =========================================================================================
public class AndroidBackgroundService : IBackgroundService
{
    public static Intent ServiceIntent;

    public void Start()
    {
        // Tạo Intent trỏ tới class ForegroundLocationService bên dưới
        if (ServiceIntent == null)
        {
            ServiceIntent = new Intent(global::Android.App.Application.Context, typeof(ForegroundLocationService));
        }

        // Logic khởi động an toàn cho các đời Android
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            global::Android.App.Application.Context.StartForegroundService(ServiceIntent);
        }
        else
        {
            global::Android.App.Application.Context.StartService(ServiceIntent);
        }
    }

    public void Stop()
    {
        if (ServiceIntent != null)
        {
            global::Android.App.Application.Context.StopService(ServiceIntent);
            ServiceIntent = null;
        }
    }
}

// =========================================================================================
// CLASS 2: NGƯỜI THI CÔNG (NATIVE SERVICE)
// Nhiệm vụ: Chạy ngầm thực sự, hiển thị thông báo, giữ App không bị Android giết
// =========================================================================================

// [QUAN TRỌNG CỰC KỲ]
// 1. Name: Phải trùng khớp 100% với thẻ <service android:name="..." /> trong AndroidManifest.xml
// 2. Thay "com.companyname.streetfood_app" bằng Package Name thực tế của bạn (Xem trong file csproj hoặc Manifest)
// 3. ForegroundServiceType: Bắt buộc phải có để chạy trên Android 14
[Service(Name = "com.companyname.streetfood_app.ForegroundLocationService", ForegroundServiceType = ForegroundService.TypeLocation)]
public class ForegroundLocationService : Service
{
    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        // 1. Cấu hình ID và Tên kênh thông báo
        string channelId = "streetfood_service_channel_clean_v1";
        string channelName = "Street Food Location Service";

        // 2. Tạo Notification Channel (Bắt buộc cho Android 8.0+)
        var manager = GetSystemService(NotificationService) as NotificationManager;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High);
            // Tắt rung/đèn để đỡ phiền người dùng (tùy chọn)
            channel.EnableVibration(false);
            manager.CreateNotificationChannel(channel);
        }

        // 3. Tạo Thông báo (Notification)
        // Lưu ý: Icon nên dùng icon hệ thống (IcMenuMyLocation) để tránh lỗi crash do không tìm thấy ảnh
        var notification = new NotificationCompat.Builder(this, channelId)
            .SetContentTitle("Street Food đang chạy")
            .SetContentText("Đang quét vị trí quán ăn gần bạn...")
            .SetSmallIcon(global::Android.Resource.Drawable.IcMenuMyLocation)
            .SetOngoing(true) // Không cho quẹt xóa
            .Build();

        // 4. Bắt đầu chạy Foreground (Chống Crash Android 14)
        // ID 1001 là mã định danh của thông báo này
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
        {
            StartForeground(1001, notification, ForegroundService.TypeLocation);
        }
        else
        {
            StartForeground(1001, notification);
        }

        // Sticky: Nếu App bị giết do thiếu RAM, Android sẽ tự khởi động lại Service này khi có RAM
        return StartCommandResult.Sticky;
    }
}