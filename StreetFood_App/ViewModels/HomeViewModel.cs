using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using StreetFood_App.Models;
using StreetFood_App.Services;
using StreetFood_App.Pages;

namespace StreetFood_App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    // List gốc lưu tất cả dữ liệu (để khi tìm kiếm thì lọc từ đây)
    private List<PointOfInterest> _allPois = new();

    // List hiển thị lên màn hình (đã qua lọc)
    [ObservableProperty]
    ObservableCollection<PointOfInterest> hotRestaurants = new();

    // Biến Search Text
    [ObservableProperty]
    string searchText;

    // Biến Ẩn/Hiện khung Filter
    [ObservableProperty]
    bool isFilterVisible = false;

    // Danh sách Category
    [ObservableProperty]
    ObservableCollection<SelectableItem> filterCategories;

    // Biến giá tiền tối đa
    [ObservableProperty]
    double currentMaxPrice = 500000;

    // Text hiển thị giá (Binding lên UI)
    public string PriceDisplay => string.Format("{0:N0} đ", CurrentMaxPrice);

    public HomeViewModel(DatabaseService dbService)
    {
        _dbService = dbService;

        // Khởi tạo các danh mục
        FilterCategories = new ObservableCollection<SelectableItem>
        {
            new SelectableItem { Name = "Tất cả", Value = "", IsSelected = true }, // [FIX] Thêm nút tất cả
            new SelectableItem { Name = "🐚 Ốc", Value = "Ốc", IsSelected = false },
            new SelectableItem { Name = "🍲 Lẩu", Value = "Lẩu", IsSelected = false },
            new SelectableItem { Name = "🍢 Vặt", Value = "Vặt", IsSelected = false },
            new SelectableItem { Name = "🍰 Bánh", Value = "Bánh", IsSelected = false }
        };

        // [FIX 1] BỎ Task.Run ở Constructor đi. 
        // Constructor chỉ nên khởi tạo biến, không nên chạy tác vụ nặng/async.
        // Việc load dữ liệu sẽ chuyển sang hàm InitializeAsync.
    }

    // [FIX 2] Hàm khởi tạo an toàn - Gọi từ MainPage.OnAppearing
    [RelayCommand]
    public async Task InitializeAsync()
    {
        try
        {
            // Kiểm tra DB trước, chỉ tạo data mẫu nếu DB đang trống
            var checkData = await _dbService.GetPOIsAsync();
            if (checkData.Count == 0)
            {
                await _dbService.SeedDataAsync();
                _allPois = await _dbService.GetPOIsAsync();
            }
            else
            {
                _allPois = checkData;
            }

            // Hiển thị lên giao diện
            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi Load Data: {ex.Message}");
        }
    }

    // Hook khi SearchText thay đổi
    partial void OnSearchTextChanged(string value) => ApplyFilters();

    // Hook khi Giá thay đổi
    partial void OnCurrentMaxPriceChanged(double value) => OnPropertyChanged(nameof(PriceDisplay));

    [RelayCommand]
    void ToggleFilter() => IsFilterVisible = !IsFilterVisible;

    [RelayCommand]
    void ToggleCategory(SelectableItem item)
    {
        if (item == null) return;

        // Logic chọn kiểu Radio Button (Chỉ chọn 1 cái) hoặc Checkbox (Chọn nhiều)
        // Ở đây tui làm kiểu Toggle đơn giản: Bấm vào thì đổi trạng thái
        item.IsSelected = !item.IsSelected;

        // Gọi lọc ngay lập tức cho mượt
        ApplyFilters();
    }

    [RelayCommand]
    void ApplyFilters()
    {
        // Nếu chưa có dữ liệu thì thôi
        if (_allPois == null) return;

        var result = _allPois.AsEnumerable();

        // 1. Lọc theo Text (Dùng ToLower để không phân biệt hoa thường)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var keyword = SearchText.ToLower().Trim();
            result = result.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(keyword)) ||
                (p.Description != null && p.Description.ToLower().Contains(keyword))
            );
        }

        // 2. Lọc theo Giá
        if (CurrentMaxPrice < 500000)
        {
            result = result.Where(p => p.AveragePrice <= CurrentMaxPrice);
        }

        // 3. Lọc theo Danh mục
        // [FIX 3] Logic lọc Category chính xác hơn
        var selectedValues = FilterCategories
                            .Where(c => c.IsSelected && !string.IsNullOrEmpty(c.Value))
                            .Select(c => c.Value.ToLower()) // Đưa về chữ thường
                            .ToList();

        if (selectedValues.Count > 0)
        {
            result = result.Where(p => selectedValues.Any(val =>
                (p.Name != null && p.Name.ToLower().Contains(val)) ||
                (p.Description != null && p.Description.ToLower().Contains(val))
            ));
        }

        // Cập nhật lên UI (ObservableCollection tự báo cho View biết)
        HotRestaurants.Clear();
        foreach (var item in result)
        {
            HotRestaurants.Add(item);
        }
    }

    [RelayCommand]
    void ResetFilter()
    {
        CurrentMaxPrice = 500000;
        SearchText = "";
        foreach (var item in FilterCategories)
        {
            // Reset về mặc định (chỉ chọn nút "Tất cả" nếu có logic đó, hoặc bỏ chọn hết)
            item.IsSelected = (item.Value == "");
        }
        ApplyFilters();
    }

    [RelayCommand]
    async Task GoToDetail(PointOfInterest poi)
    {
        if (poi == null) return;
        var navParam = new Dictionary<string, object> { { "SelectedPoi", poi }, { "AutoPlay", false } };
        await Shell.Current.GoToAsync(nameof(DetailPage), navParam);
    }
}