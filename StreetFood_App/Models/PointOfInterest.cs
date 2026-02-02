using SQLite;

namespace StreetFood_App.Models;

public class PointOfInterest
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CategoryId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; } = 20.0;
    public double AveragePrice { get; set; }
    public string ImageThumbnail { get; set; }
    public string Status { get; set; } = "Active";

    // Trường cũ (nếu bạn còn dùng file nhạc)
    public string AudioFileName { get; set; }

    // [MỚI] Trạng thái Yêu thích (True/False)
    public bool IsFavorite { get; set; }

    // [MỚI] Điểm người dùng tự đánh giá (1 đến 5)
    public int UserRating { get; set; }
}