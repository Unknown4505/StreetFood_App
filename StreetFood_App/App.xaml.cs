using StreetFood_App.Services;

namespace StreetFood_App;

public partial class App : Application
{
    // Tạo biến để lưu Service dùng sau này
    private readonly DatabaseService _dbService;

    public App(DatabaseService dbService)
    {
        InitializeComponent();

        // Lưu service vào biến
        _dbService = dbService;

        // XÓA DÒNG NÀY: MainPage = new AppShell(); (Đây là nguyên nhân gây lỗi)
    }

    // GHI ĐÈ HÀM TẠO CỬA SỔ (Cách chuẩn của .NET 9)
    protected override Window CreateWindow(IActivationState activationState)
    {
        // 1. Gọi hàm tạo dữ liệu mẫu (Chạy ngầm khi cửa sổ được tạo)
        Task.Run(async () => await _dbService.AddSampleDataAsync());

        // 2. Trả về cửa sổ mới chứa AppShell (Giao diện chính)
        return new Window(new AppShell());
    }
}