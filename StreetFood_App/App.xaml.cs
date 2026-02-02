using StreetFood_App.Services;

namespace StreetFood_App;

public partial class App : Application
{
    // [SỬA 1] Không cần biến _dbService ở đây nữa
    // private readonly DatabaseService _dbService; 

    public App(DatabaseService dbService)
    {
        InitializeComponent();
        // _dbService = dbService; // [SỬA 2] Bỏ dòng này luôn
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        // [SỬA 3 - QUAN TRỌNG NHẤT] 
        // XÓA DÒNG NÀY ĐI: Task.Run(async () => await _dbService.SeedDataAsync());
        // Lý do: HomeViewModel đã lo việc này rồi. Để lại là bị xung đột gây sập App.

        return new Window(new AppShell());
    }
}