using Royal_Blueberry_Dictionary.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class PackageDetailsDialog : Window
    {
        public PackageDetailsDialog(Package package, PackageDetail detail)
        {
            InitializeComponent();
            DataContext = new PackageDetailsViewModel(package, detail, this);
            ApplyGlobalFont();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font error: {ex.Message}");
            }
        }
    }

    public class PackageDetailsViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private readonly Package _package;
        private readonly PackageDetail _detail;
        private readonly Window _owner;

        private ObservableCollection<WordItemViewModel> _filteredWords = new();
        public ObservableCollection<WordItemViewModel> FilteredWords
        {
            get => _filteredWords;
            private set => SetProperty(ref _filteredWords, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public string PackageName => _package.name;
        public string PackageDescription => _package.description;
        public int TotalWordsCount => _detail.TotalWords;

        public CommunityToolkit.Mvvm.Input.IRelayCommand CloseCommand { get; }
        public CommunityToolkit.Mvvm.Input.IRelayCommand SearchCommand { get; }

        public PackageDetailsViewModel(Package package, PackageDetail detail, Window owner)
        {
            _package = package;
            _detail = detail;
            _owner = owner;

            CloseCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CloseDialog);
            SearchCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ApplyFilter);

            LoadWords();
        }

        private void LoadWords()
        {
            var all = _detail.Words.Select(w => new WordItemViewModel(w)).ToList();
            FilteredWords = new ObservableCollection<WordItemViewModel>(all);
        }

        private void ApplyFilter()
        {
            IEnumerable<WordEntry> source = _detail.Words;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                source = source.Where(w =>
                    (!string.IsNullOrEmpty(w.Word) && w.Word.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(w.Definition) && w.Definition.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredWords = new ObservableCollection<WordItemViewModel>(source.Select(w => new WordItemViewModel(w)));
        }

        private void CloseDialog()
        {
            _owner.DialogResult = true;
            _owner.Close();
        }
    }

    public class WordItemViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public WordEntry Entry { get; }

        public WordItemViewModel(WordEntry entry)
        {
            Entry = entry;
        }

        public string Word => Entry.Word;
        public string Phonetic => Entry.Phonetic;
        public string ShortDefinition => Entry.Definition;
    }
}
