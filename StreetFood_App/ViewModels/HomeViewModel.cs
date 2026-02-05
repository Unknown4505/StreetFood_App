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

    // List gốc lưu tất cả dữ liệu
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

    public string PriceDisplay => string.Format("{0:N0} đ", CurrentMaxPrice);

    public HomeViewModel(DatabaseService dbService)
    {
        _dbService = dbService;

        // Khởi tạo các danh mục
        FilterCategories = new ObservableCollection<SelectableItem>
        {
            new SelectableItem { Name = "Tất cả", Value = "", IsSelected = true },
            new SelectableItem { Name = "🐚 Ốc", Value = "Ốc", IsSelected = false },
            new SelectableItem { Name = "🍲 Lẩu", Value = "Lẩu", IsSelected = false },
            new SelectableItem { Name = "🍢 Vặt", Value = "Vặt", IsSelected = false },
            new SelectableItem { Name = "🍰 Bánh", Value = "Bánh", IsSelected = false }
        };
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        try
        {
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

            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi Load Data: {ex.Message}");
        }
    }

    // Hook khi thay đổi Search/Price
    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnCurrentMaxPriceChanged(double value) => OnPropertyChanged(nameof(PriceDisplay));

    [RelayCommand]
    void ToggleFilter() => IsFilterVisible = !IsFilterVisible;

    [RelayCommand]
    void ToggleCategory(SelectableItem item)
    {
        if (item == null) return;
        item.IsSelected = !item.IsSelected;
        // ApplyFilters(); 
    }

    // Hàm dùng cho nút "Áp dụng" trên giao diện Filter
    [RelayCommand]
    void ConfirmFilter()
    {
        ApplyFilters();
        IsFilterVisible = false; // Đóng popup
    }

    [RelayCommand]
    void ApplyFilters()
    {
        if (_allPois == null) return;

        var result = _allPois.AsEnumerable();

        // 1. Lọc theo Text (Dùng Helper để tìm tiếng Việt không dấu)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            // Chuyển từ khóa tìm kiếm sang không dấu
            var keyword = VietnameseHelper.ConvertToUnSign(SearchText.Trim());

            result = result.Where(p =>
                // [ĐÃ UPDATE] CHỈ TÌM TRONG TÊN (NAME)
                (p.Name != null && VietnameseHelper.ConvertToUnSign(p.Name).Contains(keyword))

            // [ĐÃ TẮT] Tắt tìm trong mô tả để tránh tìm nhầm (VD: gõ "oc" không ra "óc heo")
            // || (p.Description != null && VietnameseHelper.ConvertToUnSign(p.Description).Contains(keyword))
            );
        }

        // 2. Lọc theo Giá
        if (CurrentMaxPrice < 500000)
        {
            result = result.Where(p => p.AveragePrice <= CurrentMaxPrice);
        }

        // 3. Lọc theo Danh mục
        var selectedValues = FilterCategories
                            .Where(c => c.IsSelected && !string.IsNullOrEmpty(c.Value))
                            .Select(c => VietnameseHelper.ConvertToUnSign(c.Value))
                            .ToList();

        if (selectedValues.Count > 0)
        {
            result = result.Where(p => selectedValues.Any(val =>
                (p.Name != null && VietnameseHelper.ConvertToUnSign(p.Name).Contains(val))
            // Với danh mục thì có thể giữ lại tìm trong mô tả hoặc tắt đi tùy bạn, ở đây tui cũng tắt cho đồng bộ
            // || (p.Description != null && VietnameseHelper.ConvertToUnSign(p.Description).Contains(val))
            ));
        }

        // Cập nhật UI
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