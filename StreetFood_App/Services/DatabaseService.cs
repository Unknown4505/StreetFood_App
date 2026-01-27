using SQLite;
using StreetFood_App.Models;

namespace StreetFood_App.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    // 1. Khởi tạo Database và Tạo 6 Bảng (5 bảng cũ + 1 bảng Food)
    async Task Init()
    {
        if (_database is not null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "StreetFood.db3");

        // Thêm các cờ (Flags) để tối ưu hiệu năng và đa luồng
        _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        // Tạo bảng theo đúng mô hình
        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<PointOfInterest>();
        await _database.CreateTableAsync<PoiContent>();
        await _database.CreateTableAsync<Tour>();
        await _database.CreateTableAsync<UserAnalytics>();
        await _database.CreateTableAsync<Food>(); // <-- Đã có bảng Món ăn
    }

    // 2. Các hàm lấy dữ liệu (Truy vấn)

    // Lấy tất cả địa điểm (POI)
    public async Task<List<PointOfInterest>> GetPOIsAsync()
    {
        await Init();
        return await _database.Table<PointOfInterest>().ToListAsync();
    }

    // Lấy nội dung chi tiết theo ngôn ngữ
    public async Task<PoiContent> GetPoiContentAsync(int poiId, string langCode)
    {
        await Init();
        return await _database.Table<PoiContent>()
                            .Where(c => c.PoiId == poiId && c.LanguageCode == langCode)
                            .FirstOrDefaultAsync();
    }

    // Lấy danh sách món ăn của một quán (Dùng cho trang DetailPage)
    public async Task<List<Food>> GetFoodsAsync(int poiId)
    {
        await Init();
        return await _database.Table<Food>()
                            .Where(f => f.PoiId == poiId)
                            .ToListAsync();
    }

    // 3. Hàm tạo dữ liệu mẫu (Cập nhật thêm phần Món ăn)
    public async Task AddSampleDataAsync()
    {
        await Init();

        // Chỉ thêm nếu chưa có dữ liệu (Check bảng Category)
        if (await _database.Table<Category>().CountAsync() == 0)
        {
            // A. Tạo Danh mục (Category)
            var catOc = new Category { Name = "Quán Ốc", Icon = "ic_snail.png" };
            await _database.InsertAsync(catOc);

            // B. Tạo Địa điểm (POI) - Liên kết với Category
            var poi1 = new PointOfInterest
            {
                Name = "Ốc Oanh",
                CategoryId = catOc.Id,
                Latitude = 10.7607,
                Longitude = 106.7009,
                Radius = 25,
                ImageThumbnail = "https://cdn.tgdd.vn/2021/04/CookProduct/1-1200x676-17.jpg",
                Status = "Active"
            };
            await _database.InsertAsync(poi1);

            // C. Tạo Nội dung Đa ngôn ngữ (PoiContent)
            var contentVI = new PoiContent
            {
                PoiId = poi1.Id,
                LanguageCode = "vi-VN",
                Name = "Ốc Oanh Vĩnh Khánh",
                Description = "Quán ốc nổi tiếng nhất khu vực Quận 4 với món càng ghẹ rang muối ớt trứ danh.",
                TtsScript = "Chào bạn! Đây là Ốc Oanh, quán ốc đông vui nhất Vĩnh Khánh đây rồi!",
                AudioUrl = "ocoanh_vi.mp3"
            };

            var contentEN = new PoiContent
            {
                PoiId = poi1.Id,
                LanguageCode = "en-US",
                Name = "Oc Oanh Seafood",
                Description = "The most famous snail restaurant in District 4.",
                TtsScript = "Welcome to Oc Oanh! This is the most bustling spot in Vinh Khanh food street.",
                AudioUrl = "ocoanh_en.mp3"
            };

            await _database.InsertAllAsync(new List<PoiContent> { contentVI, contentEN });

            // D. TẠO MENU MÓN ĂN (Phần quan trọng vừa thêm vào)
            var foods = new List<Food>
            {
                new Food
                {
                    PoiId = poi1.Id, // Gắn vào quán Ốc Oanh
                    Name = "Ốc Hương Xào Bơ",
                    Price = 120000,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRz-7uQy-v4lM8g2w4g", // Link ảnh mẫu
                    Description = "Ốc hương tươi, sốt bơ tỏi béo ngậy chấm bánh mì."
                },
                new Food
                {
                    PoiId = poi1.Id,
                    Name = "Càng Ghẹ Rang Muối",
                    Price = 150000,
                    ImageUrl = "https://cdn.tgdd.vn/2021/04/CookProduct/1-1200x676-17.jpg",
                    Description = "Càng ghẹ loại 1, muối ớt cay nồng đậm vị."
                },
                new Food
                {
                    PoiId = poi1.Id,
                    Name = "Sò Điệp Mỡ Hành",
                    Price = 90000,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ",
                    Description = "Sò điệp nướng rắc đậu phộng thơm phức."
                }
            };
            await _database.InsertAllAsync(foods);
        }
    }
}