namespace StreetFood_App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    // Hàm này quyết định màn hình nào hiện lên đầu tiên (AppShell chứa TabBar)
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}