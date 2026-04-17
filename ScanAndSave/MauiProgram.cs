using Microsoft.Extensions.Logging;
using ScanAndSave.Services;
using ScanAndSave.ViewModels;
using ZXing.Net.Maui.Controls;

namespace ScanAndSave;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()    // <-- ZXing barcode reader 
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ===== BACKEND REGISTRERING =====

        // Registrer DatabaseService som Singleton (én instans i hele appen)
        builder.Services.AddSingleton<DatabaseService>();

        // Registrer ViewModels som Transient (ny instans per side)
        builder.Services.AddTransient<ScannerViewModel>();
        builder.Services.AddTransient<GroupsViewModel>();
        builder.Services.AddTransient<ProductsViewModel>();

        // Registrer Pages (så de kan modtage ViewModels via Dependency Injection)
        // OBS: Tilføj dine page-klasser her når Adam har lavet dem:
        // builder.Services.AddTransient<MainPage>();
        // builder.Services.AddTransient<GroupsPage>();
        // builder.Services.AddTransient<ProductsPage>();
        // builder.Services.AddTransient<AddProductPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}