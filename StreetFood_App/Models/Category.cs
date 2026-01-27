using SQLite;

namespace StreetFood_App.Models;

public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } // Ví dụ: Quán Ốc, Trạm xe buýt
    public string Icon { get; set; } // Tên file ảnh (vd: "ic_snail.png")
}