using JinxApp.DataService;
using Microsoft.Extensions.Logging;

namespace JinxApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Party Vibes.otf", "Party Vibes");
            });

        string saveDir = Path.Combine(FileSystem.AppDataDirectory, "saves");
        JinxData.dataService = new XmlDataService(saveDir);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}