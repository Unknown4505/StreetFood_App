using StreetFood_App.Models;
using StreetFood_App.Services;
using Plugin.Maui.Audio; // Thư viện âm thanh
using Microsoft.Maui.Media; // Dùng cho TextToSpeech (Fallback)

namespace StreetFood_App.Pages;

[QueryProperty(nameof(SelectedPoi), "SelectedPoi")]
[QueryProperty(nameof(AutoPlay), "AutoPlay")]
public partial class DetailPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly IAudioManager _audioManager;

    // Biến quản lý trình phát nhạc MP3
    private IAudioPlayer _audioPlayer;

    // Biến quản lý Text-to-Speech (TTS)
    private CancellationTokenSource _cts;

    // Biến trạng thái chung
    private bool _isPlaying = false;

    public bool AutoPlay { get; set; }

    private PointOfInterest _poi;
    public PointOfInterest SelectedPoi
    {
        get => _poi;
        set
        {
            _poi = value;
            OnPropertyChanged();
            LoadMenuAsync();
        }
    }

    public DetailPage(DatabaseService dbService, IAudioManager audioManager)
    {
        InitializeComponent();
        _dbService = dbService;
        _audioManager = audioManager;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateUiState();

        if (AutoPlay)
        {
            AutoPlay = false;
            await Task.Delay(500);
            await PlayAudioLogic();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopAudio(); // Dừng mọi âm thanh khi thoát trang
    }

    // --- LOGIC XỬ LÝ AUDIO (HYBRID: MP3 -> TTS) ---

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        if (_isPlaying)
        {
            StopAudio();
        }
        else
        {
            await PlayAudioLogic();
        }
    }

    private async Task PlayAudioLogic()
    {
        if (SelectedPoi == null) return;

        // 1. Dừng nhạc cũ/TTS cũ trước khi phát mới
        StopAudio();

        try
        {
            // CASE 1: ƯU TIÊN PHÁT FILE MP3 (NẾU CÓ)
            if (!string.IsNullOrEmpty(SelectedPoi.AudioFile))
            {
                // Load file từ Resources/Raw
                var audioStream = await FileSystem.OpenAppPackageFileAsync(SelectedPoi.AudioFile);

                _audioPlayer = _audioManager.CreatePlayer(audioStream);

                _audioPlayer.PlaybackEnded += (s, e) =>
                {
                    _isPlaying = false;
                    MainThread.BeginInvokeOnMainThread(() => UpdateSpeakerButtonState(false));
                };

                _audioPlayer.Play();
                _isPlaying = true;
                UpdateSpeakerButtonState(true);
            }
            // CASE 2: KHÔNG CÓ FILE MP3 -> FALLBACK SANG TTS (CHỊ GOOGLE)
            else
            {
                _isPlaying = true;
                UpdateSpeakerButtonState(true);

                // Tạo Token hủy mới
                _cts = new CancellationTokenSource();

                // Cấu hình tiếng Việt
                var locales = await TextToSpeech.Default.GetLocalesAsync();
                var viLocale = locales.FirstOrDefault(l => l.Language == "vi");
                var options = new SpeechOptions { Locale = viLocale, Pitch = 1.0f, Volume = 1.0f };

                // Đọc văn bản mô tả (Truyền Token vào để có thể dừng)
                await TextToSpeech.Default.SpeakAsync(SelectedPoi.Description, options, _cts.Token);

                _isPlaying = false;
                UpdateSpeakerButtonState(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Bị dừng chủ động -> Không làm gì
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi Audio: {ex.Message}");
            // Fallback cuối cùng
            if (!_isPlaying) // Tránh loop
            {
                await TextToSpeech.Default.SpeakAsync("Xin lỗi, không thể phát nội dung này.");
            }
            _isPlaying = false;
            UpdateSpeakerButtonState(false);
        }
    }

    private void StopAudio()
    {
        // 1. Dừng MP3 (nếu đang phát)
        if (_audioPlayer != null && _audioPlayer.IsPlaying)
        {
            _audioPlayer.Stop();
            _audioPlayer.Dispose();
            _audioPlayer = null;
        }

        // 2. Dừng TTS (nếu đang đọc) -> [ĐÃ FIX LỖI Ở ĐÂY]
        // Thay vì gọi CancelAsync(), ta hủy Token
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // 3. Reset trạng thái
        _isPlaying = false;
        UpdateSpeakerButtonState(false);
    }

    private void UpdateSpeakerButtonState(bool isPlaying)
    {
        if (isPlaying)
        {
            BtnSpeak.Text = "🤫 Dừng";
            BtnSpeak.BackgroundColor = Color.FromArgb("#FF5252");
            BtnSpeak.TextColor = Colors.White;
            LblStatus.Text = "Đang phát thuyết minh...";
        }
        else
        {
            BtnSpeak.Text = "🔊 Nghe";
            BtnSpeak.BackgroundColor = Color.FromArgb("#E3F2FD");
            BtnSpeak.TextColor = Color.FromArgb("#1565C0");
            LblStatus.Text = "Nhấn loa để nghe thuyết minh";
        }
    }

    // --- CÁC LOGIC KHÁC GIỮ NGUYÊN ---

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
        BtnFavorite.Text = SelectedPoi.IsFavorite ? "❤️ Đã thích" : "🤍 Thích";
        BtnFavorite.BackgroundColor = SelectedPoi.IsFavorite ? Color.FromArgb("#FFCDD2") : Color.FromArgb("#FFEBEE");
        BtnFavorite.TextColor = SelectedPoi.IsFavorite ? Colors.Red : Color.FromArgb("#C62828");

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