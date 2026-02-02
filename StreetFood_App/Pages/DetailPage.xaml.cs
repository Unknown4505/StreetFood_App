using StreetFood_App.Models;
using StreetFood_App.Services;
using Microsoft.Maui.Media; // Thư viện chứa TextToSpeech
using System.Threading;     // Thư viện chứa CancellationToken

namespace StreetFood_App.Pages;

[QueryProperty(nameof(SelectedPoi), "SelectedPoi")]
[QueryProperty(nameof(AutoPlay), "AutoPlay")]
public partial class DetailPage : ContentPage
{
    private readonly DatabaseService _dbService;

    // Biến dùng để quản lý việc Đọc/Dừng Audio
    private bool _isSpeaking = false;
    private CancellationTokenSource _cts; // Token để hủy việc đọc

    public bool AutoPlay { get; set; }

    private PointOfInterest _poi;
    public PointOfInterest SelectedPoi
    {
        get => _poi;
        set
        {
            _poi = value;
            OnPropertyChanged();
            LoadMenuAsync(); // Load menu ngay khi có dữ liệu
        }
    }

    public DetailPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateUiState();

        // Xử lý tự động phát (từ Geofence hoặc Scan QR)
        if (AutoPlay)
        {
            AutoPlay = false; // Reset cờ để không lặp lại
            await Task.Delay(500); // Chờ UI ổn định
            await SpeakNow();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Khi rời trang -> Hủy đọc ngay lập tức
        CancelSpeech();
        _isSpeaking = false;
    }

    // --- LOGIC XỬ LÝ AUDIO (ĐÃ FIX RÈ) ---

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        if (_isSpeaking)
        {
            // Đang đọc -> Bấm thì Dừng
            CancelSpeech();
            _isSpeaking = false;
            UpdateSpeakerButtonState(false);
        }
        else
        {
            // Đang im -> Bấm thì Đọc
            await SpeakNow();
        }
    }

    private async Task SpeakNow()
    {
        if (SelectedPoi == null) return;

        // 1. [QUAN TRỌNG] Hủy âm thanh cũ trước khi bắt đầu cái mới
        CancelSpeech();

        // 2. Tạo Token mới
        _cts = new CancellationTokenSource();
        _isSpeaking = true;
        UpdateSpeakerButtonState(true);

        string textToRead = $"Chào mừng bạn đến với {SelectedPoi.Name}. {SelectedPoi.Description}";

        try
        {
            // Cấu hình giọng đọc tiếng Việt
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var viLocale = locales.FirstOrDefault(l => l.Language == "vi");
            var options = new SpeechOptions
            {
                Locale = viLocale,
                Pitch = 1.0f,
                Volume = 1.0f
            };

            // 3. Đọc (kèm Token để có thể hủy)
            await TextToSpeech.Default.SpeakAsync(textToRead, options, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Bị hủy chủ động -> Không làm gì cả
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Lỗi TTS: " + ex.Message);
        }
        finally
        {
            // Kết thúc đọc (hoặc bị hủy) -> Reset trạng thái nút
            _isSpeaking = false;
            MainThread.BeginInvokeOnMainThread(() => UpdateSpeakerButtonState(false));
        }
    }

    private void CancelSpeech()
    {
        // Hủy Token -> TextToSpeech sẽ dừng lại ngay
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void UpdateSpeakerButtonState(bool isReading)
    {
        if (isReading)
        {
            BtnSpeak.Text = "🤫 Dừng";
            BtnSpeak.BackgroundColor = Color.FromArgb("#FF5252");
            BtnSpeak.TextColor = Colors.White;
            LblStatus.Text = "Đang đọc...";
        }
        else
        {
            BtnSpeak.Text = "🔊 Nghe";
            BtnSpeak.BackgroundColor = Color.FromArgb("#E3F2FD");
            BtnSpeak.TextColor = Color.FromArgb("#1565C0");
            LblStatus.Text = "Nhấn loa để nghe thuyết minh";
        }
    }

    // --- CÁC LOGIC KHÁC ---

    private async void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (SelectedPoi == null) return;
        SelectedPoi.IsFavorite = !SelectedPoi.IsFavorite;
        await _dbService.UpdatePoiAsync(SelectedPoi);
        UpdateUiState();
    }

    private async void OnRateClicked(object sender, EventArgs e)
    {
        if (SelectedPoi == null) return;
        string action = await DisplayActionSheet("Đánh giá quán này:", "Hủy", null, "5 ⭐", "4 ⭐", "3 ⭐", "2 ⭐", "1 ⭐");
        if (string.IsNullOrEmpty(action) || action == "Hủy") return;

        int rating = int.Parse(action.Substring(0, 1));
        SelectedPoi.UserRating = rating;
        await _dbService.UpdatePoiAsync(SelectedPoi);
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        if (SelectedPoi == null) return;

        // Cập nhật nút Tim
        BtnFavorite.Text = SelectedPoi.IsFavorite ? "❤️ Đã thích" : "🤍 Thích";
        BtnFavorite.BackgroundColor = SelectedPoi.IsFavorite ? Color.FromArgb("#FFCDD2") : Color.FromArgb("#FFEBEE");
        BtnFavorite.TextColor = SelectedPoi.IsFavorite ? Colors.Red : Color.FromArgb("#C62828");

        // Cập nhật nút Sao
        if (SelectedPoi.UserRating > 0)
        {
            BtnRate.Text = $"{SelectedPoi.UserRating} ⭐";
            LblUserRating.Text = $"Bạn chấm {SelectedPoi.UserRating} sao.";
        }
        else
        {
            BtnRate.Text = "⭐ Chấm điểm";
            LblUserRating.Text = "";
        }
    }

    async void LoadMenuAsync()
    {
        if (SelectedPoi == null) return;
        var foods = await _dbService.GetFoodsAsync(SelectedPoi.Id);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // [FIX UI] Dùng BindableLayout để hiện danh sách mà không bị lỗi cuộn
            // Lưu ý: Trong XAML phải đặt tên StackLayout là x:Name="MenuContainer"
            BindableLayout.SetItemsSource(MenuContainer, foods);
        });
    }

    async void OnDirectionsClicked(object sender, EventArgs e)
    {
        if (SelectedPoi == null) return;
        try
        {
            var location = new Location(SelectedPoi.Latitude, SelectedPoi.Longitude);
            var options = new MapLaunchOptions { Name = SelectedPoi.Name, NavigationMode = NavigationMode.Driving };
            await Map.OpenAsync(location, options);
        }
        catch { }
    }
}