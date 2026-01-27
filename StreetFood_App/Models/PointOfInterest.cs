using SQLite;

namespace StreetFood_App.Models;

public class PointOfInterest
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CategoryId { get; set; }

    // --- THÊM DÒNG NÀY ĐỂ HẾT LỖI ---
    public string Name { get; set; }
    // --------------------------------

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; } = 20.0;

    public string ImageThumbnail { get; set; }
    public string Status { get; set; } = "Active";
}