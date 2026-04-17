using ScanAndSave.Models;
using ScanAndSave.ViewModels;

namespace ScanAndSave.Views
{
    [QueryProperty(nameof(GroupId), "groupId")]
    [QueryProperty(nameof(GroupName), "groupName")]
    public partial class ProductsPage : ContentPage
    {
        private readonly ProductsViewModel _vm;

        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        public ProductsPage(ProductsViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (int.TryParse(GroupId, out int id))
            {
                await _vm.LoadProductsAsync(id, Uri.UnescapeDataString(GroupName));
            }
        }

        /// <summary>
        /// Reducer antal med 1 (sletter varen ved 0)
        /// </summary>
        private async void OnQuantityDown(object? sender, EventArgs e)
        {
            if (GetProductFromSender(sender) is Product product)
            {
                if (product.Quantity <= 1)
                {
                    bool confirm = await DisplayAlert(
                        "Slet vare",
                        $"'{product.Name}' har kun 1 stk. Vil du slette den?",
                        "Slet", "Annuller");
                    if (!confirm) return;
                }
                await _vm.UpdateQuantityAsync(product, -1);
            }
        }

        /// <summary>
        /// Forøg antal med 1
        /// </summary>
        private async void OnQuantityUp(object? sender, EventArgs e)
        {
            if (GetProductFromSender(sender) is Product product)
            {
                await _vm.UpdateQuantityAsync(product, 1);
            }
        }

        /// <summary>
        /// Slet produkt via swipe
        /// </summary>
        private async void OnDeleteProduct(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Product product)
            {
                bool confirm = await DisplayAlert(
                    "Slet vare",
                    $"Er du sikker på at du vil slette '{product.Name}'?",
                    "Slet", "Annuller");

                if (confirm)
                {
                    await _vm.DeleteProductAsync(product);
                }
            }
        }

        /// <summary>
        /// Gå tilbage til scanneren
        /// </summary>
        private async void OnGoToScanner(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("../..");
        }

        /// <summary>
        /// Hjælpemetode til at hente Product fra button's CommandParameter
        /// </summary>
        private static Product? GetProductFromSender(object? sender)
        {
            if (sender is Button button && button.CommandParameter is Product product)
                return product;
            return null;
        }
    }
}