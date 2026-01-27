using SQLite;

namespace StreetFood_App.Models;

public class Tour
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }

    // Lưu danh sách ID dưới dạng chuỗi "1,5,8" cho đơn giản
    public string ListPoiIds { get; set; }
}