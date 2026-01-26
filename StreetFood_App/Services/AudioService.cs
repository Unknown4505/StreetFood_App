namespace StreetFood_App.Services;

public class AudioService
{
    // Đã xóa phần gây lỗi
    // Xử lý Text-to-Speech hoặc phát Audio 
    public async Task SpeakAsync(string text)
    {
        // TextToSpeech là tính năng có sẵn trong MAUI (Microsoft.Maui.Media)
        await TextToSpeech.Default.SpeakAsync(text);
    }
}