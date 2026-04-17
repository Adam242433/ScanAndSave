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
    /// ViewModel for menuen/oversigten med alle opbevaringsgrupper.
    /// Binder til GroupsPage i XAML.
    public class GroupsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;

        public ObservableCollection<StorageGroup> Groups { get; set; } = new();

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
            set { _searchText = value; OnPropertyChanged(); }
        }

        public GroupsViewModel(DatabaseService db)
        {
            _db = db;
        }

        /// Indlæser alle grupper fra databasen

        public async Task LoadGroupsAsync()
        {
            IsLoading = true;
            try
            {
                var groups = await _db.GetAllGroupsAsync();
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// Tilføjer en ny gruppe

        public async Task AddGroupAsync(string name, string? imagePath)
        {
            var group = new StorageGroup
            {
                Name = name,
                ImagePath = imagePath
            };
            await _db.AddGroupAsync(group);
            await LoadGroupsAsync();
        }


        /// Sletter en gruppe og alle dens varer

        public async Task DeleteGroupAsync(StorageGroup group)
        {
            await _db.DeleteGroupAsync(group.Id);
            Groups.Remove(group);
        }

 
        /// Opdaterer en gruppes navn eller billede

        public async Task UpdateGroupAsync(StorageGroup group)
        {
            await _db.UpdateGroupAsync(group);
            await LoadGroupsAsync();
        }


        /// Henter antal varer i en bestemt gruppe (til visning i UI)

        public async Task<int> GetProductCountAsync(int groupId)
        {
            return await _db.GetProductCountInGroupAsync(groupId);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}