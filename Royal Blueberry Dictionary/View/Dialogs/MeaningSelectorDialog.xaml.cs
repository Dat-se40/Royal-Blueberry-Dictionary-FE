using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Royal_Blueberry_Dictionary.Repository.Interface;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class MeaningSelectorDialog : Window
    {
        public WordDetail Detail { get; }
        public ObservableCollection<Tag> Tags { get; }
        public string UserId { get; }

        public int SelectedMeaningIndex { get; private set; } = -1;
        public int SelectedDefinitionIndex { get; private set; } = 0;
        public Tag? SelectedTag { get; private set; }
        public WordEntry? SelectedEntry { get; private set; }

        public MeaningSelectorDialog(WordDetail detail, string? userId = null, IEnumerable<Tag>? tags = null)
        {
            InitializeComponent();

            Detail = detail;
            UserId = string.IsNullOrWhiteSpace(userId) ? "GUEST" : userId;
            Tags = new ObservableCollection<Tag>(LoadTags(tags));
            DataContext = this;

            WordTitleText.Text = Detail.Word?.ToUpperInvariant() ?? "WORD";
        }

        private IEnumerable<Tag> LoadTags(IEnumerable<Tag>? provided)
        {
            if (provided != null)
                return provided;

            try
            {
                var repo = App.serviceProvider.GetService<ITagRepository>();
                if (repo != null)
                {
                    return repo.GetAllTagsAsync().GetAwaiter().GetResult();
                }
            }
            catch
            {
                // fallback to empty if DI not available
            }
            return Enumerable.Empty<Tag>();
        }

        private void MeaningsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MeaningsList.SelectedItem is not Meaning meaning)
            {
                return;
            }

            SelectedMeaningIndex = Detail.Meanings.IndexOf(meaning);
            SelectedDefinitionIndex = 0;

            var definitionPreview = meaning.Definitions.FirstOrDefault()?.Text ?? "No definition available";
            SelectedMeaningText.Text = $"{meaning.PartOfSpeech}: {definitionPreview}";

            StepIndicator.Text = "Step 2/2: Select a tag (optional)";
            MeaningsPanel.Visibility = Visibility.Collapsed;
            TagsPanel.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            TagsPanel.Visibility = Visibility.Collapsed;
            MeaningsPanel.Visibility = Visibility.Visible;
            StepIndicator.Text = "Step 1/2: Select meaning";
            MeaningsList.SelectedItem = null;
            TagsList.SelectedItem = null;
            SelectedTag = null;
            SelectedMeaningIndex = -1;
            SelectedEntry = null;
        }

        private void SkipTagsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSelectionAndClose(applyTag: false);
        }

        private void TagsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedTag = TagsList.SelectedItem as Tag;
        }
        public async void CreateTagButton_Click(object sender, RoutedEventArgs e) 
        {
            TagPickerDialog tagPickerDialog = new TagPickerDialog(); 
            var result = tagPickerDialog.ShowDialog();   
            if(result != null && result == true) 
            {
                var tagService = App.serviceProvider.GetRequiredService<TagService>();
                var tags = await tagService.GetAllTagsAsync();
                Tags.Clear();
                Tags.AddRange(tags);    
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSelectionAndClose(applyTag: true);
        }

        private void SaveSelectionAndClose(bool applyTag)
        {
            if (SelectedMeaningIndex < 0 || SelectedMeaningIndex >= Detail.Meanings.Count)
            {
                MessageBox.Show("Please choose a meaning before saving.", "Missing selection");
                return;
            }

            if (Detail.Meanings[SelectedMeaningIndex].Definitions.Count == 0)
            {
                MessageBox.Show("The selected meaning does not contain any definition to save.", "No definition");
                return;
            }

            SelectedEntry = WordService.MapWordDetailToWordEntry(Detail, SelectedMeaningIndex, SelectedDefinitionIndex);

            if (applyTag && SelectedTag != null)
            {
                SelectedEntry.TagIdsJson = new List<string> { SelectedTag.Id };
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
