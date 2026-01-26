using SQLite;
using StreetFood_App.Models;

namespace StreetFood_App.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    async Task Init()
    {
        if (_database is not null)
            return;

        // Tạo đường dẫn file data.db trong thư mục local của App
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "StreetFood.db3");

        _database = new SQLiteAsyncConnection(dbPath);

        // Tạo bảng PointOfInterest dựa trên Model bạn đã viết
        await _database.CreateTableAsync<PointOfInterest>();
    }

    public async Task<List<PointOfInterest>> GetPOIsAsync()
    {
        await Init();
        return await _database.Table<PointOfInterest>().ToListAsync();
    }

    // Hàm thêm dữ liệu mẫu (để test)
    public async Task AddSampleDataAsync()
    {
        await Init();
        var existing = await GetPOIsAsync();
        if (existing.Count == 0)
        {
            var poi = new PointOfInterest
            {
                Name = "Ốc Oanh",
                Latitude = 10.7607,
                Longitude = 106.7009,
                Description = "Quán ốc nổi tiếng nhất Vĩnh Khánh...",
                Radius = 30 // 30 mét
            };
            await _database.InsertAsync(poi);
        }
    }
}