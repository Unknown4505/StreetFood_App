using SQLite;

namespace StreetFood_App.Models;

public class UserAnalytics
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int PoiId { get; set; }
    public string ActionType { get; set; } // "ViewMap", "TriggerAudio", "ScanQR"
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; set; }
}