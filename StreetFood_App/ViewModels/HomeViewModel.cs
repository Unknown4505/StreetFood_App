using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using StreetFood_App.Models;

namespace StreetFood_App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    public ObservableCollection<Food> Foods { get; set; }

    public HomeViewModel()
    {
        Foods = new ObservableCollection<Food>
        {
            new Food { Name = "Ốc Hương Xào Bơ", Price = "120k", ImageUrl = "https://cdn.tgdd.vn/2021/04/CookProduct/1-1200x676-17.jpg" },
            new Food { Name = "Sò Điệp Mỡ Hành", Price = "90k", ImageUrl = "https://cdn.tgdd.vn/2020/09/CookProduct/1260-1200x676-17.jpg" },
            new Food { Name = "Càng Ghẹ Muối", Price = "150k", ImageUrl = "https://cdn.tgdd.vn/2021/05/CookProduct/1-1200x676-15.jpg" }
        };
    }
}