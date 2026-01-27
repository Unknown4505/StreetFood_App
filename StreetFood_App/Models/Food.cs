using SQLite;

namespace StreetFood_App.Models;

public class Food
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PoiId { get; set; } // Khóa ngoại: Món này thuộc quán nào?

    public string Name { get; set; }        // Tên món (vd: Ốc Hương Xào Bơ)
    public decimal Price { get; set; }      // Giá tiền (vd: 120000)
    public string ImageUrl { get; set; }    // Hình ảnh món ăn
    public string Description { get; set; } // Mô tả ngắn (vd: "Thơm, béo, cay")
}