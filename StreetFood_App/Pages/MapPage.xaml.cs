using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using StreetFood_App.Models;
using StreetFood_App.ViewModels;
using StreetFood_App.Services;

// --- ALIAS CHO GỌN ---
using MBrush = Mapsui.Styles.Brush;
using MColor = Mapsui.Styles.Color;
using MPen = Mapsui.Styles.Pen;
using MStyle = Mapsui.Styles.SymbolStyle;
using MLabelStyle = Mapsui.Styles.LabelStyle;

namespace StreetFood_App.Pages;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;
    private readonly LocationService _locationService;

    // Các Layer của Mapsui
    private MemoryLayer _userLocationLayer;
    private MemoryLayer _poiLayer;

    public MapPage(MapViewModel vm, LocationService locationService)
    {
        InitializeComponent();
        _viewModel = vm;
        _locationService = locationService;
        BindingContext = _viewModel;

        InitializeMapStructure();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadingMap.IsVisible = true;
        LoadingMap.IsRunning = true;

        await _viewModel.LoadPoisAsync();
        DrawPins(_viewModel.Pois);

        LoadingMap.IsRunning = false;
        LoadingMap.IsVisible = false;
    }

    void InitializeMapStructure()
    {
        var map = new Mapsui.Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        _poiLayer = new MemoryLayer
        {
            Name = "PinLayer",
            Style = null
        };
        map.Layers.Add(_poiLayer);

        _userLocationLayer = new MemoryLayer { Name = "UserLocation" };
        map.Layers.Add(_userLocationLayer);

        // Center mặc định (Vĩnh Khánh)
        var center = SphericalMercator.FromLonLat(106.7020, 10.7626);
        map.Navigator.CenterOn(center.x, center.y);
        map.Navigator.ZoomTo(2);

        MyMapControl.Map = map;
        MyMapControl.Map.Tapped += Map_Tapped;
    }

    // [FIX LỖI] Đã thêm tham số Layers vào GetMapInfo
    private void Map_Tapped(object sender, MapEventArgs e)
    {
        var screenPosition = e.ScreenPosition;

        // [SỬA TẠI ĐÂY] Thêm tham số thứ 2: MyMapControl.Map.Layers
        // Ý nghĩa: Kiểm tra thông tin click trên TẤT CẢ các layer đang có
        var mapInfo = MyMapControl.GetMapInfo(screenPosition, MyMapControl.Map.Layers);

        if (mapInfo != null && mapInfo.Feature != null)
        {
            if (mapInfo.Feature["PoiData"] is PointOfInterest selectedPoi)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool answer = await DisplayAlert(selectedPoi.Name,
                        "Xem chi tiết quán này?", "Xem ngay", "Đóng");

                    if (answer)
                    {
                        var navParam = new Dictionary<string, object>
                        {
                            { "SelectedPoi", selectedPoi },
                            { "AutoPlay", false }
                        };
                        await Shell.Current.GoToAsync(nameof(DetailPage), navParam);
                    }
                });
            }
        }
    }

    private void DrawPins(IEnumerable<PointOfInterest> pois)
    {
        if (pois == null) return;
        var features = new List<IFeature>();

        foreach (var item in pois)
        {
            var coords = SphericalMercator.FromLonLat(item.Longitude, item.Latitude);
            var feature = new PointFeature(new MPoint(coords.x, coords.y));
            feature["PoiData"] = item;

            feature.Styles.Add(new MStyle
            {
                Fill = new MBrush(MColor.Red),
                Outline = new MPen(MColor.White, 2),
                SymbolScale = 0.6f,
                SymbolType = Mapsui.Styles.SymbolType.Ellipse
            });

            feature.Styles.Add(new MLabelStyle
            {
                Text = item.Name,
                ForeColor = MColor.Black,
                Halo = new MPen(MColor.White, 2),
                HorizontalAlignment = Mapsui.Styles.LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = Mapsui.Styles.LabelStyle.VerticalAlignmentEnum.Top,
                Offset = new Mapsui.Styles.Offset(0, -15)
            });

            features.Add(feature);
        }
        _poiLayer.Features = features;
        _poiLayer.DataHasChanged();
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        if (MyMapControl.Map?.Navigator != null)
            MyMapControl.Map.Navigator.ZoomIn();
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        if (MyMapControl.Map?.Navigator != null)
            MyMapControl.Map.Navigator.ZoomOut();
    }

    private async void OnMyLocationClicked(object sender, EventArgs e)
    {
        var location = await _locationService.GetCurrentLocation();

        if (location != null)
        {
            var coords = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var userPoint = new MPoint(coords.x, coords.y);

            MyMapControl.Map.Navigator.CenterOn(userPoint);
            MyMapControl.Map.Navigator.ZoomTo(1);

            var feature = new PointFeature(userPoint);
            feature.Styles.Add(new MStyle
            {
                Fill = new MBrush(MColor.Cyan),
                Outline = new MPen(MColor.White, 3),
                SymbolScale = 0.8f,
                SymbolType = Mapsui.Styles.SymbolType.Ellipse
            });

            _userLocationLayer.Features = new List<IFeature> { feature };
            _userLocationLayer.DataHasChanged();
        }
        else
        {
            await DisplayAlert("Lỗi GPS", "Không lấy được vị trí. Hãy kiểm tra quyền truy cập.", "OK");
        }
    }
}