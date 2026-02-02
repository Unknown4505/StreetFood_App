using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using StreetFood_App.Models;
using StreetFood_App.Services;

namespace StreetFood_App.ViewModels;

public partial class MapViewModel : ObservableObject
{
    // 1. Service để lấy dữ liệu từ SQLite
    private readonly DatabaseService _dbService;

    // 2. Danh sách dữ liệu chuẩn để View (MapPage) sử dụng
    // Dùng ObservableCollection để khi thêm/xóa thì UI tự cập nhật
    [ObservableProperty]
    ObservableCollection<PointOfInterest> pois = new();

    // 3. Constructor: Nhận DatabaseService từ Dependency Injection
    public MapViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    // 4. Command để load dữ liệu (Bất đồng bộ)
    // Page sẽ gọi lệnh này khi "OnAppearing"
    [RelayCommand]
    public async Task LoadPoisAsync()
    {
        try
        {
            // Lấy danh sách từ Database thật
            var list = await _dbService.GetPOIsAsync();

            // Xóa danh sách cũ (để tránh trùng lặp nếu load lại)
            Pois.Clear();

            // Thêm dữ liệu mới vào
            foreach (var item in list)
            {
                Pois.Add(item);
            }
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu có vấn đề về Database
            System.Diagnostics.Debug.WriteLine($"Lỗi load Map: {ex.Message}");
        }
    }
}