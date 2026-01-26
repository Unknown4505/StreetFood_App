using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices;
using StreetFood_App.ViewModels;
using Microsoft.Maui.Maps;
namespace StreetFood_App.Pages;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    // Parameterless ctor for XAML/Shell instantiation (uses a local ViewModel fallback)
    public MapPage() : this(new MapViewModel())
    {
    }

    public MapPage(MapViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;

        // Di chuyển camera bản đồ về khu Vĩnh Khánh (Quận 4)
        var vinhKhanhLoc = new Location(10.7607, 106.7009);
        // Lưu ý: Distance và MapSpan lấy từ Microsoft.Maui.Maps
        var mapSpan = MapSpan.FromCenterAndRadius(vinhKhanhLoc, Distance.FromKilometers(0.5));
        FoodMap.MoveToRegion(mapSpan);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Thêm Pin từ ViewModel vào Bản đồ
        FoodMap.Pins.Clear();
        foreach (var pin in _viewModel.Pins)
        {
            FoodMap.Pins.Add(pin);
        }
    }
}