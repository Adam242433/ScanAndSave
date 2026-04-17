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

    /// ViewModel for visning af produkter inden i en gruppe.
    /// Binder til ProductsPage i XAML.

    public class ProductsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;

        public ObservableCollection<Product> Products { get; set; } = new();

        private int _currentGroupId;
        public int CurrentGroupId
        {
            get => _currentGroupId;
            set { _currentGroupId = value; OnPropertyChanged(); }
        }

        private string _currentGroupName = string.Empty;
        public string CurrentGroupName
        {
            get => _currentGroupName;
            set { _currentGroupName = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Søg automatisk når teksten ændres
                _ = SearchAsync();
            }
        }

        public ProductsViewModel(DatabaseService db)
        {
            _db = db;
        }

        /// Indlæser alle produkter for den valgte gruppe

        public async Task LoadProductsAsync(int groupId, string groupName)
        {
            CurrentGroupId = groupId;
            CurrentGroupName = groupName;
            IsLoading = true;

            try
            {
                var products = await _db.GetProductsByGroupAsync(groupId);
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// Søger i produkter inden for gruppen

        public async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadProductsAsync(CurrentGroupId, CurrentGroupName);
                return;
            }

            IsLoading = true;
            try
            {
                var results = await _db.SearchProductsInGroupAsync(CurrentGroupId, SearchText);
                Products.Clear();
                foreach (var product in results)
                {
                    Products.Add(product);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// Opdaterer antal af et produkt (f.eks. +1 / -1)

        public async Task UpdateQuantityAsync(Product product, int change)
        {
            product.Quantity += change;

            if (product.Quantity <= 0)
            {
                // Slet varen hvis antal når 0
                await _db.DeleteProductAsync(product.Id);
                Products.Remove(product);
            }
            else
            {
                await _db.UpdateProductAsync(product);
                // Opdater i listen
                var index = Products.IndexOf(product);
                if (index >= 0)
                {
                    Products[index] = product;
                }
            }
        }


        /// Sletter et produkt

        public async Task DeleteProductAsync(Product product)
        {
            await _db.DeleteProductAsync(product.Id);
            Products.Remove(product);
        }


        /// Opdaterer et produkt (navn, kategori, antal, udløbsdato)

        public async Task UpdateProductAsync(Product product)
        {
            await _db.UpdateProductAsync(product);
            await LoadProductsAsync(CurrentGroupId, CurrentGroupName);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
