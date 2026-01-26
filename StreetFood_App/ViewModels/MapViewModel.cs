using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using StreetFood_App.Models;

namespace StreetFood_App.ViewModels;

public partial class MapViewModel : ObservableObject
{
    public ObservableCollection<Pin> Pins { get; set; } = new ObservableCollection<Pin>();

    public MapViewModel()
    {
        LoadPins();
    }

    // Hàm tạo giả dữ liệu các quán ốc trên bản đồ
    void LoadPins()
    {
        // Giả sử đây là tọa độ khu phố ốc Vĩnh Khánh (Quận 4)
        var vinhKhanhLocation = new Location(10.7607, 106.7009);

        // Thêm quán ốc ví dụ
        var pin1 = new Pin
        {
            Label = "Ốc Oanh",
            Address = "534 Vĩnh Khánh, Q.4",
            Type = PinType.Place,
            Location = new Location(10.7607, 106.7009)
        };

        var pin2 = new Pin
        {
            Label = "Ốc Thảo",
            Address = "383 Vĩnh Khánh, Q.4",
            Type = PinType.Place,
            Location = new Location(10.7615, 106.7015)
        };

        Pins.Add(pin1);
        Pins.Add(pin2);
    }
}