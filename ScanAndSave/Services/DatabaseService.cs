using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;
using ScanAndSave.Models;

namespace ScanAndSave.Services
{
    /// Central database-service der håndterer al CRUD-logik for grupper og produkter.
    /// Bruger SQLite via sqlite-net-pcl NuGet-pakken.
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            // Databasen gemmes i appens lokale data-mappe
            _dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "scanandsave.db3"
            );
        }

        /// Initialiserer databasen og opretter tabellerne hvis de ikke findes.
        /// Kaldes automatisk før enhver operation.
        private async Task InitAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

            // Opret tabellerne
            await _database.CreateTableAsync<StorageGroup>();
            await _database.CreateTableAsync<Product>();
        }

        // ======================================================================
        // STORAGE GROUP CRUD
        // ======================================================================

        /// Henter alle opbevaringsgrupper (køleskab, skuffe osv.)
        public async Task<List<StorageGroup>> GetAllGroupsAsync()
        {
            await InitAsync();
            return await _database!.Table<StorageGroup>()
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        /// Henter en enkelt gruppe ud fra dens Id
        public async Task<StorageGroup?> GetGroupByIdAsync(int id)
        {
            await InitAsync();
            return await _database!.Table<StorageGroup>()
                .Where(g => g.Id == id)
                .FirstOrDefaultAsync();
        }

        /// Opretter en ny gruppe (CREATE)
        public async Task<int> AddGroupAsync(StorageGroup group)
        {
            await InitAsync();
            group.CreatedAt = DateTime.Now;
            return await _database!.InsertAsync(group);
        }

        /// Opdaterer en eksisterende gruppe (UPDATE)
        public async Task<int> UpdateGroupAsync(StorageGroup group)
        {
            await InitAsync();
            return await _database!.UpdateAsync(group);
        }

        /// Sletter en gruppe og ALLE dens tilhørende varer (DELETE)

        public async Task DeleteGroupAsync(int groupId)
        {
            await InitAsync();
            // Slet først alle produkter i gruppen
            await _database!.ExecuteAsync(
                "DELETE FROM Product WHERE StorageGroupId = ?", groupId);
            // Slet derefter selve gruppen
            await _database!.DeleteAsync<StorageGroup>(groupId);
        }

        /// Tæller antal varer i en given gruppe

        public async Task<int> GetProductCountInGroupAsync(int groupId)
        {
            await InitAsync();
            return await _database!.Table<Product>()
                .Where(p => p.StorageGroupId == groupId)
                .CountAsync();
        }

        // ======================================================================
        // PRODUCT CRUD
        // ======================================================================


        /// Henter alle produkter i en bestemt gruppe

        public async Task<List<Product>> GetProductsByGroupAsync(int groupId)
        {
            await InitAsync();
            return await _database!.Table<Product>()
                .Where(p => p.StorageGroupId == groupId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// Henter et enkelt produkt ud fra Id

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            await InitAsync();
            return await _database!.Table<Product>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        /// Finder et produkt ud fra stregkode i en bestemt gruppe
        public async Task<Product?> GetProductByBarcodeInGroupAsync(string barcode, int groupId)
        {
            await InitAsync();
            return await _database!.Table<Product>()
                .Where(p => p.Barcode == barcode && p.StorageGroupId == groupId)
                .FirstOrDefaultAsync();
        }


        /// Tilføjer et nyt produkt (CREATE).
        /// Hvis varen allerede eksisterer i gruppen (samme stregkode),
        /// opdateres antallet i stedet.

        public async Task<int> AddProductAsync(Product product)
        {
            await InitAsync();

            // Tjek om varen allerede findes i denne gruppe
            var existing = await GetProductByBarcodeInGroupAsync(
                product.Barcode, product.StorageGroupId);

            if (existing is not null)
            {
                // Varen findes allerede – opdater antallet
                existing.Quantity += product.Quantity;
                // Opdater udløbsdato hvis den nye er sat
                if (product.ExpiryDate.HasValue)
                    existing.ExpiryDate = product.ExpiryDate;
                return await _database!.UpdateAsync(existing);
            }

            product.AddedDate = DateTime.Now;
            return await _database!.InsertAsync(product);
        }

        /// Opdaterer et eksisterende produkt (UPDATE)

        public async Task<int> UpdateProductAsync(Product product)
        {
            await InitAsync();
            return await _database!.UpdateAsync(product);
        }


        /// Sletter et produkt (DELETE)

        public async Task<int> DeleteProductAsync(int productId)
        {
            await InitAsync();
            return await _database!.DeleteAsync<Product>(productId);
        }

        // ======================================================================
        // SØGNING
        // ======================================================================

        /// Søger efter produkter på tværs af alle grupper (navn eller stregkode)

        public async Task<List<Product>> SearchProductsAsync(string searchText)
        {
            await InitAsync();
            var lowerSearch = searchText.ToLower();
            return await _database!.Table<Product>()
                .Where(p =>
                    p.Name.ToLower().Contains(lowerSearch) ||
                    p.Barcode.Contains(searchText) ||
                    p.Category.ToLower().Contains(lowerSearch))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// Søger efter produkter inden for en bestemt gruppe
        public async Task<List<Product>> SearchProductsInGroupAsync(int groupId, string searchText)
        {
            await InitAsync();
            var lowerSearch = searchText.ToLower();
            return await _database!.Table<Product>()
                .Where(p =>
                    p.StorageGroupId == groupId &&
                    (p.Name.ToLower().Contains(lowerSearch) ||
                     p.Barcode.Contains(searchText) ||
                     p.Category.ToLower().Contains(lowerSearch)))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // ======================================================================
        // UDLØBSDATO-FUNKTIONER
        // ======================================================================

        /// Henter alle produkter der er udløbet
        public async Task<List<Product>> GetExpiredProductsAsync()
        {
            await InitAsync();
            var now = DateTime.Now;
            return await _database!.Table<Product>()
                .Where(p => p.ExpiryDate != null && p.ExpiryDate < now)
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();
        }


        /// Henter produkter der udløber inden for et givet antal dage

        public async Task<List<Product>> GetProductsExpiringSoonAsync(int withinDays = 3)
        {
            await InitAsync();
            var now = DateTime.Now;
            var deadline = now.AddDays(withinDays);
            return await _database!.Table<Product>()
                .Where(p => p.ExpiryDate != null && p.ExpiryDate >= now && p.ExpiryDate <= deadline)
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();
        }

        // ======================================================================
        // STATISTIK
        // ======================================================================

        /// Henter det samlede antal varer på tværs af alle grupper

        public async Task<int> GetTotalProductCountAsync()
        {
            await InitAsync();
            return await _database!.Table<Product>().CountAsync();
        }

        /// Henter alle unikke kategorier
        public async Task<List<string>> GetAllCategoriesAsync()
        {
            await InitAsync();
            var products = await _database!.Table<Product>().ToListAsync();
            return products
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
}