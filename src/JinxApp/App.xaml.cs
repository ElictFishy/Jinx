namespace JinxApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine($"=== SAVE DIR : {FileSystem.AppDataDirectory} ===");

    }

    protected override void OnSleep()
    {
        if (JinxData.g != null && JinxData.gm != null && JinxData.gm.IsGameInProgress())
            JinxData.dataService?.SaveCurrentGame(JinxData.g);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}