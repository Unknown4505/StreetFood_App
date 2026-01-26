using SQLite;

namespace StreetFood_App.Models;

public class PointOfInterest
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }        // Tên quán/địa điểm
    public string Description { get; set; } // Nội dung thuyết minh text
    public string ImageUrl { get; set; }    // Hình ảnh

    // Đã xóa ở đây
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double Radius { get; set; } = 20.0; // Bán kính kích hoạt (mét)
    public string AudioFileName { get; set; }  // Tên file mp3 hoặc nội dung TTS 
}