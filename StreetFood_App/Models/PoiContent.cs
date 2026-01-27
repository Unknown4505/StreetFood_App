using SQLite;

namespace StreetFood_App.Models;

public class PoiContent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PoiId { get; set; } // Khóa ngoại trỏ về PointOfInterest

    public string LanguageCode { get; set; } // "vi-VN", "en-US"

    public string Name { get; set; }         // Tên quán (theo ngôn ngữ)
    public string Description { get; set; }  // Bài viết dài
    public string TtsScript { get; set; }    // Kịch bản cho AI đọc (ngắn gọn)
    public string AudioUrl { get; set; }     // Link file mp3 (nếu có)
}