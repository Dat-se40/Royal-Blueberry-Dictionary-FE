using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class RemoveTagDialog : Window
    {
        private readonly ITagRepository _tagRepository;
        private readonly string _userId;
        public List<string> RemovedTagIds { get; private set; } = new();

        public static event Action? OnTagsDeleted;

        public RemoveTagDialog(string? userId = null)
        {
            InitializeComponent();
            _tagRepository = App.serviceProvider.GetRequiredService<ITagRepository>();
            _userId = string.IsNullOrWhiteSpace(userId) ? "GUEST" : userId;

            _ = LoadTagsAsync();
            ApplyGlobalFont();
        }

        private async Task LoadTagsAsync()
        {
            var allTags = await _tagRepository.GetAllTagsAsync(_userId);
            if (allTags == null || allTags.Count == 0)
            {
                MessageBox.Show("There are no tags to delete yet!", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TagsList.ItemsSource = allTags;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIds = new List<string>();

                foreach (var item in TagsList.Items)
                {
                    if (TagsList.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container)
                    {
                        var check = FindDescendant<CheckBox>(container);
                        if (check != null && check.IsChecked == true && check.Tag is string id)
                            selectedIds.Add(id);
                    }
                }

                if (selectedIds.Count == 0)
                {
                    MessageBox.Show("Please select at least one tag!", "Notification",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete {selectedIds.Count} tag(s)?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                foreach (string id in selectedIds)
                {
                    await _tagRepository.DeleteTagAsync(id);
                    RemovedTagIds.Add(id);
                }
                await _tagRepository.SaveChangesAsync();

                MessageBox.Show($"Successfully deleted {selectedIds.Count} tag(s).",
                    "Completed successfully", MessageBoxButton.OK, MessageBoxImage.Information);

                OnTagsDeleted?.Invoke();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error deleting tag: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private T? FindDescendant<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;

                var desc = FindDescendant<T>(child);
                if (desc != null)
                    return desc;
            }

            return null;
        }

        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }
    }
}
