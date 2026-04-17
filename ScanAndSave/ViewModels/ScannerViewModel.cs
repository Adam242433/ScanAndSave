using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScanAndSave.Models;
using ScanAndSave.Services;

namespace ScanAndSave.ViewModels
{
    /// ViewModel for startskærmen med scanner.
    /// Håndterer scanning af stregkoder og tilføjelse af nye varer.
    /// Binder til ScannerPage / MainPage i XAML.
    public class ScannerViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;

        // ===== Scannede data =====

        private string _scannedBarcode = string.Empty;
        public string ScannedBarcode
        {
            get => _scannedBarcode;
            set { _scannedBarcode = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasScannedBarcode)); }
        }

        public bool HasScannedBarcode => !string.IsNullOrEmpty(ScannedBarcode);

        // ===== Formular-felter til ny vare =====

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(); }
        }

        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        private DateTime? _expiryDate;
        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set { _expiryDate = value; OnPropertyChanged(); }
        }

        private int _selectedGroupId;
        public int SelectedGroupId
        {
            get => _selectedGroupId;
            set { _selectedGroupId = value; OnPropertyChanged(); }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        // ===== Grupper til dropdown =====

        public ObservableCollection<StorageGroup> AvailableGroups { get; set; } = new();

        // ===== Kategorier forslag =====

        public ObservableCollection<string> SuggestedCategories { get; set; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ScannerViewModel(DatabaseService db)
        {
            _db = db;
        }

        /// Indlæser grupper og kategorier til formularen

        public async Task InitializeAsync()
        {
            var groups = await _db.GetAllGroupsAsync();
            AvailableGroups.Clear();
            foreach (var group in groups)
            {
                AvailableGroups.Add(group);
            }

            var categories = await _db.GetAllCategoriesAsync();
            SuggestedCategories.Clear();
            foreach (var cat in categories)
            {
                SuggestedCategories.Add(cat);
            }
        }

        /// Kaldes når ZXing scanneren registrerer en stregkode.
        /// Sæt denne som event handler for BarcodeDetected.
        public void OnBarcodeScanned(string barcode)
        {
            ScannedBarcode = barcode;
            StatusMessage = $"Stregkode scannet: {barcode}";
        }


        /// Tilføjer den scannede vare til den valgte gruppe.
        /// Validerer input inden tilføjelse.

        /// True hvis varen blev tilføjet, false ved fejl
        public async Task<(bool Success, string Message)> AddProductAsync()
        {
            // ===== VALIDERING =====

            if (string.IsNullOrWhiteSpace(ScannedBarcode))
                return (false, "Scan venligst en stregkode først.");

            if (string.IsNullOrWhiteSpace(ProductName))
                return (false, "Indtast venligst et navn på varen.");

            if (SelectedGroupId <= 0)
                return (false, "Vælg venligst en opbevaringsgruppe.");

            if (Quantity <= 0)
                return (false, "Antal skal være mindst 1.");

            // ===== OPRET PRODUKT =====

            IsLoading = true;
            try
            {
                var product = new Product
                {
                    Barcode = ScannedBarcode,
                    Name = ProductName.Trim(),
                    Category = Category?.Trim() ?? string.Empty,
                    Quantity = Quantity,
                    ExpiryDate = ExpiryDate,
                    StorageGroupId = SelectedGroupId,
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
                };

                await _db.AddProductAsync(product);

                // Nulstil formularen
                ResetForm();

                return (true, $"'{product.Name}' blev tilføjet!");
            }
            catch (Exception ex)
            {
                return (false, $"Fejl ved tilføjelse: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// Nulstiller alle formular-felter efter tilføjelse

        public void ResetForm()
        {
            ScannedBarcode = string.Empty;
            ProductName = string.Empty;
            Category = string.Empty;
            Quantity = 1;
            ExpiryDate = null;
            Notes = string.Empty;
            StatusMessage = string.Empty;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
