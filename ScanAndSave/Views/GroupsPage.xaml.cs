using ScanAndSave.Models;
using ScanAndSave.ViewModels;

namespace ScanAndSave.Views
{
    public partial class GroupsPage : ContentPage
    {
        private readonly GroupsViewModel _vm;

        public GroupsPage(GroupsViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadGroupsAsync();
        }

        private async void OnAddGroup(object? sender, EventArgs e)
        {
            // Trin 1: Navn
            string? name = await DisplayPromptAsync(
                "Ny gruppe",
                "Hvad skal gruppen hedde?",
                placeholder: "Fx Køleskab, Fryser, Skuffe...",
                accept: "Næste",
                cancel: "Annuller",
                maxLength: 40,
                keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(name))
                return;

            // Trin 2: Billede (valgfrit)
            string? imagePath = null;
            bool takePicture = await DisplayAlert(
                "Billede af gruppen",
                $"Vil du tage et billede af '{name.Trim()}'?",
                "Tag billede",
                "Spring over");

            if (takePicture)
            {
                imagePath = await TakePhotoAsync();
            }

            // Trin 3: Gem
            await _vm.AddGroupAsync(name.Trim(), imagePath);
        }

        private async Task<string?> TakePhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo is null) return null;

                var localPath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    $"group_{DateTime.Now.Ticks}.jpg");

                using var stream = await photo.OpenReadAsync();
                using var fileStream = File.OpenWrite(localPath);
                await stream.CopyToAsync(fileStream);

                return localPath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fejl", $"Kunne ikke tage billede: {ex.Message}", "OK");
                return null;
            }
        }

        private async void OnGroupTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is StorageGroup group)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(ProductsPage)}?groupId={group.Id}&groupName={group.Name}");
            }
        }

        private async void OnDeleteGroup(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is StorageGroup group)
            {
                bool confirm = await DisplayAlert(
                    "Slet gruppe",
                    $"Er du sikker pa at du vil slette '{group.Name}' og alle dens varer?",
                    "Slet",
                    "Annuller");

                if (confirm)
                {
                    if (!string.IsNullOrWhiteSpace(group.ImagePath) && File.Exists(group.ImagePath))
                        File.Delete(group.ImagePath);

                    await _vm.DeleteGroupAsync(group);
                }
            }
        }

        private async void OnDeleteGroupButton(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is StorageGroup group)
            {
                bool confirm = await DisplayAlert(
                    "Slet gruppe",
                    $"Er du sikker pa at du vil slette '{group.Name}' og alle dens varer?",
                    "Slet",
                    "Annuller");

                if (confirm)
                {
                    if (!string.IsNullOrWhiteSpace(group.ImagePath) && File.Exists(group.ImagePath))
                        File.Delete(group.ImagePath);

                    await _vm.DeleteGroupAsync(group);
                }
            }
        }
    }
}