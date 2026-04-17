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

        /// <summary>
        /// Åbn dialog for at tilføje ny gruppe
        /// </summary>
        private async void OnAddGroup(object? sender, EventArgs e)
        {
            string name = await DisplayPromptAsync(
                "Ny gruppe",
                "Hvad skal gruppen hedde?",
                placeholder: "Fx Køleskab, Fryser, Skuffe...",
                accept: "Opret",
                cancel: "Annuller");

            if (string.IsNullOrWhiteSpace(name))
                return;

            // Spørg om brugeren vil tage et billede
            string? imagePath = null;
            bool takePicture = await DisplayAlert(
                "Billede",
                "Vil du tage et billede af gruppen?",
                "Ja", "Nej");

            if (takePicture)
            {
                try
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo is not null)
                    {
                        // Gem billedet lokalt
                        var localPath = Path.Combine(
                            FileSystem.AppDataDirectory, $"group_{DateTime.Now.Ticks}.jpg");
                        using var stream = await photo.OpenReadAsync();
                        using var fileStream = File.OpenWrite(localPath);
                        await stream.CopyToAsync(fileStream);
                        imagePath = localPath;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Fejl", $"Kunne ikke tage billede: {ex.Message}", "OK");
                }
            }

            await _vm.AddGroupAsync(name.Trim(), imagePath);
        }

        /// <summary>
        /// Åbn gruppen og vis dens produkter
        /// </summary>
        private async void OnGroupTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is StorageGroup group)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(ProductsPage)}?groupId={group.Id}&groupName={group.Name}");
            }
        }

        /// <summary>
        /// Slet en gruppe med bekræftelse
        /// </summary>
        private async void OnDeleteGroup(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is StorageGroup group)
            {
                bool confirm = await DisplayAlert(
                    "Slet gruppe",
                    $"Er du sikker på at du vil slette '{group.Name}' og alle dens varer?",
                    "Slet", "Annuller");

                if (confirm)
                {
                    await _vm.DeleteGroupAsync(group);
                }
            }
        }
    }
}