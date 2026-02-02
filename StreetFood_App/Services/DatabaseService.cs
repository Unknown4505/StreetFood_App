using SQLite;
using StreetFood_App.Models;

namespace StreetFood_App.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    async Task Init()
    {
        if (_database is not null) return;

        // [QUAN TRỌNG] Đổi tên DB thành v7 để chắc chắn tạo mới
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "StreetFood_Final_v7.db3");

        _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<PointOfInterest>();
        await _database.CreateTableAsync<Food>();
    }

    public async Task<List<PointOfInterest>> GetPOIsAsync()
    {
        await Init();
        return await _database.Table<PointOfInterest>().ToListAsync();
    }

    public async Task<List<Food>> GetFoodsAsync(int poiId)
    {
        await Init();
        return await _database.Table<Food>().Where(f => f.PoiId == poiId).ToListAsync();
    }

    public async Task UpdatePoiAsync(PointOfInterest poi)
    {
        await Init();
        await _database.UpdateAsync(poi);
    }

    public async Task<List<PointOfInterest>> GetFavoritePoisAsync()
    {
        await Init();
        return await _database.Table<PointOfInterest>().Where(p => p.IsFavorite).ToListAsync();
    }

    // --- HÀM TẠO DỮ LIỆU MẪU (Đã thay link online bằng ảnh offline) ---
    public async Task SeedDataAsync()
    {
        await Init();

        // Nếu đã có dữ liệu thì không tạo nữa
        if (await _database.Table<PointOfInterest>().CountAsync() > 0) return;

        // 1. Tạo Danh Mục
        // Lưu ý: Các file icon này cũng cần có trong Resources/Images (nếu chưa có thì dùng tạm default_food.png)
        var catSeafood = new Category { Name = "Hải sản", Icon = "default_food.png" };
        var catSnack = new Category { Name = "Ăn vặt", Icon = "default_food.png" };
        var catHotpot = new Category { Name = "Lẩu", Icon = "default_food.png" };
        await _database.InsertAllAsync(new List<Category> { catSeafood, catSnack, catHotpot });

        // 2. Tạo Quán Ăn
        var pois = new List<PointOfInterest>
        {
            new PointOfInterest
            {
                Name = "Ốc Oanh",
                CategoryId = catSeafood.Id,
                Description = "Quán ốc nổi tiếng nhất khu Vĩnh Khánh. Ốc hương trứng muối là món 'best seller'.",
                // [FIX ẢNH] Dùng ảnh offline (đã thêm vào Resources/Images)
                ImageThumbnail = "default_food.png",
                AveragePrice = 150000,
                Latitude = 10.7626, Longitude = 106.7020,
                IsFavorite = false, UserRating = 5
            },
            new PointOfInterest
            {
                Name = "Súp Cua Hạnh",
                CategoryId = catSnack.Id,
                Description = "Súp cua óc heo chất lượng, đặc biệt nhiều topping, ăn là ghiền.",
                // [FIX ẢNH]
                ImageThumbnail = "default_food.png",
                AveragePrice = 35000,
                Latitude = 10.7620, Longitude = 106.7015,
                IsFavorite = true, UserRating = 4
            },
            new PointOfInterest
            {
                Name = "Lẩu Bò Nhà Gỗ",
                CategoryId = catHotpot.Id,
                Description = "Lẩu bò đậm đà hương vị Đà Lạt giữa lòng Sài Gòn.",
                // [FIX ẢNH]
                ImageThumbnail = "default_food.png",
                AveragePrice = 200000,
                Latitude = 10.7630, Longitude = 106.7030,
                IsFavorite = false, UserRating = 0
            }
        };
        await _database.InsertAllAsync(pois);

        // 3. Tạo MENU Món Ăn
        var allPois = await _database.Table<PointOfInterest>().ToListAsync();
        var oanh = allPois.FirstOrDefault(p => p.Name == "Ốc Oanh");
        var hanh = allPois.FirstOrDefault(p => p.Name == "Súp Cua Hạnh");

        var foods = new List<Food>();

        if (oanh != null)
        {
            foods.Add(new Food { PoiId = oanh.Id, Name = "Ốc Hương Trứng Muối", Price = 120000, Description = "Sốt trứng muối béo ngậy.", ImageUrl = "default_food.png" });
            foods.Add(new Food { PoiId = oanh.Id, Name = "Càng Ghẹ Rang Muối", Price = 150000, Description = "Càng ghẹ tươi, cay nồng.", ImageUrl = "default_food.png" });
        }

        if (hanh != null)
        {
            foods.Add(new Food { PoiId = hanh.Id, Name = "Súp Cua Đặc Biệt", Price = 45000, Description = "Full topping óc heo.", ImageUrl = "default_food.png" });
            foods.Add(new Food { PoiId = hanh.Id, Name = "Chân Gà Sả Tắc", Price = 50000, Description = "Chua cay giòn rụm.", ImageUrl = "default_food.png" });
        }

        await _database.InsertAllAsync(foods);
    }
}