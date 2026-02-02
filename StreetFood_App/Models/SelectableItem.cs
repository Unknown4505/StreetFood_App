using CommunityToolkit.Mvvm.ComponentModel;

namespace StreetFood_App.Models;

// Class này đại diện cho một nút danh mục (VD: Ốc, Lẩu...)
public partial class SelectableItem : ObservableObject
{
    public string Name { get; set; } // Tên danh mục
    public string Value { get; set; } // Giá trị dùng để lọc

    [ObservableProperty]
    bool isSelected; // Trạng thái: True = Đang chọn (Màu đỏ), False = Chưa chọn (Màu xám)
}