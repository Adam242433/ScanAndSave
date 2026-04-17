using ScanAndSave.ViewModels;
using ZXing.Net.Maui;

namespace ScanAndSave.Views
{
    public partial class ScannerPage : ContentPage
    {
        private readonly ScannerViewModel _vm;

        public ScannerPage(ScannerViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitializeAsync();
            BarcodeReader.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.OneDimensional,
                AutoRotate = true,
                Multiple = false
            };
        }

        /// <summary>
        /// Kaldes når ZXing registrerer en stregkode
        /// </summary>
        private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first is null) return;

            // Kør på UI-tråden
            Dispatcher.Dispatch(() =>
            {
                _vm.OnBarcodeScanned(first.Value);
                // Stop scanning mens formularen udfyldes
                BarcodeReader.IsDetecting = false;
            });
        }

        private void OnClearBarcode(object? sender, EventArgs e)
        {
            _vm.ResetForm();
            // Genaktiver scanning
            BarcodeReader.IsDetecting = true;
        }

        private void OnQuantityMinus(object? sender, EventArgs e)
        {
            if (_vm.Quantity > 1)
                _vm.Quantity--;
        }

        private void OnQuantityPlus(object? sender, EventArgs e)
        {
            _vm.Quantity++;
        }

        private void OnExpiryDateSelected(object? sender, DateChangedEventArgs e)
        {
            _vm.ExpiryDate = e.NewDate;
        }

        private void OnGroupSelected(object? sender, EventArgs e)
        {
            if (GroupPicker.SelectedItem is Models.StorageGroup group)
            {
                _vm.SelectedGroupId = group.Id;
            }
        }

        private async void OnAddProduct(object? sender, EventArgs e)
        {
            var (success, message) = await _vm.AddProductAsync();

            if (success)
            {
                await DisplayAlert("Tilføjet!", message, "OK");
                // Genaktiver scanning
                BarcodeReader.IsDetecting = true;
                GroupPicker.SelectedIndex = -1;
            }
            else
            {
                await DisplayAlert("Fejl", message, "OK");
            }
        }

        private async void OnGroupsClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(GroupsPage));
        }
    }
}