namespace ScanAndSave;

public partial class App : Application // MUST be Application, NOT MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}