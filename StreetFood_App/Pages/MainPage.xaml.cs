using StreetFood_App.ViewModels;
using StreetFood_App.Models;

namespace StreetFood_App.Pages;

public partial class MainPage : ContentPage
{
    // [FIX 1] Lưu biến ViewModel để dùng lại ở các hàm khác
    private readonly HomeViewModel _viewModel;

    // Constructor: Inject ViewModel
    public MainPage(HomeViewModel vm)
    {
        InitializeComponent();

        // [FIX 2] Gán vào biến private
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    // [FIX 3 - QUAN TRỌNG NHẤT] 
    // Khi màn hình hiện lên -> Gọi ViewModel load dữ liệu & Seed Data
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Gọi hàm kiểm tra và tạo dữ liệu (nếu DB trống)
        await _viewModel.InitializeAsync();
    }

    // 1. Xử lý khi bấm nút "Search" (hoặc Enter) trên bàn phím
    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        // Unfocus: Bỏ chọn thanh tìm kiếm -> Bàn phím sẽ tự động ẩn đi
        if (sender is SearchBar sb) sb.Unfocus();
        else MySearchBar.Unfocus();
    }

    // 2. Xử lý khi bấm vào vùng trống (Background)
    private void OnBackgroundClicked(object sender, TappedEventArgs e)
    {
        // Kiểm tra: Nếu thanh tìm kiếm đang được chọn (bàn phím đang mở)
        if (MySearchBar.IsFocused)
        {
            // Thì tắt nó đi
            MySearchBar.Unfocus();
        }
    }

    // 3. Xử lý khi bấm nút Camera (Scan QR)
    private async void OnScanClicked(object sender, EventArgs e)
    {
        // Điều hướng sang trang Scan
        await Shell.Current.GoToAsync(nameof(ScanPage));
    }
}