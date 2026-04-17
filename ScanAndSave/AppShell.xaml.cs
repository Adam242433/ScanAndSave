using ScanAndSave.Views;

namespace ScanAndSave
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrer ruter til navigation
            Routing.RegisterRoute(nameof(GroupsPage), typeof(GroupsPage));
            Routing.RegisterRoute(nameof(ProductsPage), typeof(ProductsPage));
        }
    }
}