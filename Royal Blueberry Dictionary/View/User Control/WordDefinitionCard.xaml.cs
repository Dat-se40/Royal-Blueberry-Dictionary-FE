using BlueBerryDictionary.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NavigationService = Royal_Blueberry_Dictionary.Service.NavigationService;

namespace Royal_Blueberry_Dictionary.View.User_Control
{
    public partial class WordDefinitionCard : UserControl
    {
        // =========================
        // Dependency Properties
        // =========================
        public static readonly DependencyProperty WordProperty =
            DependencyProperty.Register(nameof(Word), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PronunciationProperty =
            DependencyProperty.Register(nameof(Pronunciation), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PartOfSpeechProperty =
            DependencyProperty.Register(nameof(PartOfSpeech), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefinitionProperty =
            DependencyProperty.Register(nameof(Definition), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.Register(nameof(TimeStamp), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ViewCountProperty =
            DependencyProperty.Register(nameof(ViewCount), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RegionProperty =
            DependencyProperty.Register(nameof(Region), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Example1Property =
            DependencyProperty.Register(nameof(Example1), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Example2Property =
            DependencyProperty.Register(nameof(Example2), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DataSourceProperty =
        DependencyProperty.Register(nameof(DataSource), typeof(WordEntry), typeof(WordDefinitionCard),
            new PropertyMetadata(null, OnDataSourceChanged));
        public static readonly DependencyProperty DeleteCommandProperty =
             DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(WordDefinitionCard));
        public static readonly DependencyProperty FavoriteCommandProperty =
            DependencyProperty.Register(nameof(FavoriteCommand), typeof(ICommand), typeof(WordDefinitionCard)); 
        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }
        public ICommand FavoriteCommand 
        {
            get => (ICommand)GetValue(FavoriteCommandProperty); 
            set => SetValue (FavoriteCommandProperty, value);   
        }
        // =========================
        // CLR Wrappers
        // =========================
        public WordEntry DataSource
        {
            get => (WordEntry)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }
        public string Word
        {
            get => (string)GetValue(WordProperty);
            set => SetValue(WordProperty, value);
        }

        public string Pronunciation
        {
            get => (string)GetValue(PronunciationProperty);
            set => SetValue(PronunciationProperty, value);
        }

        public string PartOfSpeech
        {
            get => (string)GetValue(PartOfSpeechProperty);
            set => SetValue(PartOfSpeechProperty, value);
        }

        public string Definition
        {
            get => (string)GetValue(DefinitionProperty);
            set => SetValue(DefinitionProperty, value);
        }

        public string TimeStamp
        {
            get => (string)GetValue(TimeStampProperty);
            set => SetValue(TimeStampProperty, value);
        }

        public string ViewCount
        {
            get => (string)GetValue(ViewCountProperty);
            set => SetValue(ViewCountProperty, value);
        }

        public string Region
        {
            get => (string)GetValue(RegionProperty);
            set => SetValue(RegionProperty, value);
        }

        public string Example1
        {
            get => (string)GetValue(Example1Property);
            set => SetValue(Example1Property, value);
        }

        public string Example2
        {
            get => (string)GetValue(Example2Property);
            set => SetValue(Example2Property, value);
        }
        // =========================
        // Constructor
        // =========================
        
        public WordDefinitionCard()
        {
            InitializeComponent();
        }
        public void LoadData(WordEntry wordEntry) 
        {
            if (wordEntry == null) return;
            this.Word  = wordEntry.Word ?? "NULL";
            this.Pronunciation = wordEntry.Phonetic ?? "NULL";  
            this.PartOfSpeech = wordEntry.PartOfSpeech ?? "NULL";   
            this.Definition = wordEntry.Definition ?? "NULL";   
            this.Region = "US";
            //this.Example1 = wordEntry.Example ?? string.Empty;    
            //this.Example2 = wordEntry.Note ?? string.Empty;
            UpdateExampleSection(wordEntry.Example,wordEntry.Note);
        }
        // =========================
        // UI Helper Methods
        // =========================

        public void UpdateExampleSection(
            string example1Text,
            string example2Text = "",
            string example1LabelText = "Example:",
            string example2LabelText = "Note:")
        {
            Example1 = example1Text ?? string.Empty;
            Example2 = example2Text ?? string.Empty;

            if (Example1Label != null)
                Example1Label.Text = example1LabelText;

            if (Example2Label != null)
                Example2Label.Text = example2LabelText;

            if (Example2Container != null)
            {
                Example2Container.Visibility =
                    string.IsNullOrWhiteSpace(Example2)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public void HideExample2()
        {
            if (Example2Container != null)
                Example2Container.Visibility = Visibility.Collapsed;
        }

        public void ShowExample2()
        {
            if (Example2Container != null)
                Example2Container.Visibility = Visibility.Visible;
        }

        public void SetExample1Label(string text)
        {
            if (Example1Label != null && Example1 != "")
                Example1Label.Text = text;
        }

        public void SetExample2Label(string text)
        {
            if (Example2Label != null && Example2 != "")
                Example2Label.Text = text;
        }

        private void btnFav_Click(object sender, RoutedEventArgs e)
        {
            DataSource.IsFavorited = !DataSource.IsFavorited; 
        }
        private async void GoToDetailPage(object sender, RoutedEventArgs e)
        {
            if (DataSource == null) return;

            var navService = App.serviceProvider.GetRequiredService<NavigationService>();
            var searchService = App.serviceProvider.GetRequiredService<SearchService>();

            try
            {
                var fullDetail = await searchService.searchAWord(DataSource.Word);
                navService.NavigateTo<DetailsPage, DetailsPageViewModel>(fullDetail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải từ: {ex.Message}");
            }
        }
        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WordDefinitionCard card && e.NewValue is WordEntry word)
            {
                card.LoadData(word);
            }
        }

    }
}
