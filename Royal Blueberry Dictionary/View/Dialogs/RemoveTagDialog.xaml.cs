using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class RemoveTagDialog : Window
    {
        #region Fields

        private readonly ITagRepository _tagRepository;
        private readonly string _userId;
        private List<Tag> _allTags = new();

        #endregion

        #region Properties

        public List<string> RemovedTagIds { get; private set; } = new();

        public static event Action? OnTagsDeleted;

        #endregion

        #region Constructor

        public RemoveTagDialog(string? userId = null)
        {
            InitializeComponent();

            _tagRepository = App.serviceProvider.GetRequiredService<ITagRepository>();
            _userId = string.IsNullOrWhiteSpace(userId) ? "GUEST" : userId;

            ApplyGlobalFont();
            UpdateSelectedCount();

            Loaded += RemoveTagDialog_Loaded;
        }

        #endregion

        #region Load Data

        private async void RemoveTagDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTagsAsync();
        }

        private async Task LoadTagsAsync()
        {
            try
            {
                _allTags = await _tagRepository.GetAllTagsAsync();

                if (_allTags == null || _allTags.Count == 0)
                {
                    TagsList.ItemsSource = null;
                    UpdateSelectedCount();

                    MessageBox.Show(
                        "There are no tags to delete yet!",
                        "Notification",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                TagsList.ItemsSource = _allTags.OrderBy(t => t.Name).ToList();
                UpdateSelectedCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load tags: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Search

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allTags == null || _allTags.Count == 0)
            {
                TagsList.ItemsSource = null;
                return;
            }

            string keyword = SearchTextBox.Text?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                TagsList.ItemsSource = _allTags.OrderBy(t => t.Name).ToList();
                return;
            }

            var filtered = _allTags
                .Where(t =>
                    (!string.IsNullOrWhiteSpace(t.Name) && t.Name.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrWhiteSpace(t.Icon) && t.Icon.ToLower().Contains(keyword)))
                .OrderBy(t => t.Name)
                .ToList();

            TagsList.ItemsSource = filtered;
        }

        #endregion

        #region Selection

        private void TagCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in FindVisualChildren<CheckBox>(TagsList))
            {
                checkBox.IsChecked = true;
            }

            UpdateSelectedCount();
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in FindVisualChildren<CheckBox>(TagsList))
            {
                checkBox.IsChecked = false;
            }

            UpdateSelectedCount();
        }

        private List<string> GetSelectedTagIds()
        {
            var selectedIds = new List<string>();

            foreach (var item in TagsList.Items)
            {
                if (TagsList.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container)
                {
                    var check = FindDescendant<CheckBox>(container);
                    if (check != null && check.IsChecked == true && check.Tag is string id)
                    {
                        selectedIds.Add(id);
                    }
                }
            }

            return selectedIds;
        }

        private void UpdateSelectedCount()
        {
            int count = 0;

            foreach (var checkBox in FindVisualChildren<CheckBox>(TagsList))
            {
                if (checkBox.IsChecked == true)
                {
                    count++;
                }
            }

            if (SelectedCountText != null)
            {
                SelectedCountText.Text = count == 1
                    ? "1 tag selected"
                    : $"{count} tags selected";
            }

            if (DeleteButton != null)
            {
                DeleteButton.IsEnabled = count > 0;
            }
        }

        #endregion

        #region Delete

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIds = GetSelectedTagIds();

                if (selectedIds.Count == 0)
                {
                    MessageBox.Show(
                        "Please select at least one tag to delete.",
                        "Notification",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete {selectedIds.Count} tag(s)?\n\nThis action cannot be undone.",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes)
                    return;

                DeleteButton.IsEnabled = false;

                foreach (string id in selectedIds)
                {
                    await _tagRepository.DeleteTagAsync(id);
                    RemovedTagIds.Add(id);
                }

                await _tagRepository.SaveChangesAsync();

                MessageBox.Show(
                    $"Successfully deleted {selectedIds.Count} tag(s).",
                    "Completed successfully",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                OnTagsDeleted?.Invoke();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                DeleteButton.IsEnabled = true;

                MessageBox.Show(
                    $"❌ Error deleting tag: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Window Actions

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Helpers

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

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t)
                    yield return t;

                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
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

        #endregion
    }
}
