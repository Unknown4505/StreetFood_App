namespace StreetFood_App;

public partial class App : Application
{
    // [SỬA LẠI] Bỏ tham số DatabaseService vì không còn dùng ở đây nữa
    public App()
    {
        InitializeComponent();

        // [QUAN TRỌNG]
        // Đã xóa bỏ đoạn DependencyService.Register cũ kỹ.
        // Bây giờ mọi thứ đã được MauiProgram lo liệu.

        // [SỬA LẠI] Không gán MainPage ở đây nữa, để CreateWindow lo.
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        // Chỉ tạo AppShell một lần duy nhất tại đây
        return new Window(new AppShell());
    }
}