using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.User_Control
{
    /// <summary>
    /// Thẻ package: bind <see cref="ViewModel.OfflinePackageRowViewModel"/> (ItemsControl) hoặc set DependencyProperty khi dùng độc lập.
    /// </summary>
    public partial class PackageCard : UserControl
    {
        public PackageCard()
        {
            InitializeComponent();

            // Logic cũ:
            // Constructor cũ nhận TopicPackage + OfflineModeViewModel
            // rồi tạo PackageCardViewModel để xử lý command.
            // Tạm bỏ vì bản này chỉ giữ giao diện.
        }

        // =========================
        // Dependency Properties
        // =========================

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(nameof(Name), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TotalItemsProperty =
            DependencyProperty.Register(nameof(TotalItems), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SizeTextProperty =
            DependencyProperty.Register(nameof(SizeText), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(nameof(Level), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DownloadDateProperty =
            DependencyProperty.Register(nameof(DownloadDate), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty BadgeTextProperty =
            DependencyProperty.Register(nameof(BadgeText), typeof(string), typeof(PackageCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty BadgeColorProperty =
            DependencyProperty.Register(nameof(BadgeColor), typeof(Brush), typeof(PackageCard), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty IsDownloadedProperty =
            DependencyProperty.Register(nameof(IsDownloaded), typeof(bool), typeof(PackageCard), new PropertyMetadata(false));

        public static readonly DependencyProperty DownloadButtonTextProperty =
            DependencyProperty.Register(nameof(DownloadButtonText), typeof(string), typeof(PackageCard), new PropertyMetadata("💾 Load Data"));

        public static readonly DependencyProperty OpenButtonTextProperty =
            DependencyProperty.Register(nameof(OpenButtonText), typeof(string), typeof(PackageCard), new PropertyMetadata("👁️ Preview"));

        public static readonly DependencyProperty DeleteButtonTextProperty =
            DependencyProperty.Register(nameof(DeleteButtonText), typeof(string), typeof(PackageCard), new PropertyMetadata("🗑️ Delete"));

        // =========================
        // CLR Wrappers
        // =========================

        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string TotalItems
        {
            get => (string)GetValue(TotalItemsProperty);
            set => SetValue(TotalItemsProperty, value);
        }

        public string SizeText
        {
            get => (string)GetValue(SizeTextProperty);
            set => SetValue(SizeTextProperty, value);
        }

        public string Level
        {
            get => (string)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        public string DownloadDate
        {
            get => (string)GetValue(DownloadDateProperty);
            set => SetValue(DownloadDateProperty, value);
        }

        public string BadgeText
        {
            get => (string)GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        public Brush BadgeColor
        {
            get => (Brush)GetValue(BadgeColorProperty);
            set => SetValue(BadgeColorProperty, value);
        }

        public bool IsDownloaded
        {
            get => (bool)GetValue(IsDownloadedProperty);
            set => SetValue(IsDownloadedProperty, value);
        }

        public string DownloadButtonText
        {
            get => (string)GetValue(DownloadButtonTextProperty);
            set => SetValue(DownloadButtonTextProperty, value);
        }

        public string OpenButtonText
        {
            get => (string)GetValue(OpenButtonTextProperty);
            set => SetValue(OpenButtonTextProperty, value);
        }

        public string DeleteButtonText
        {
            get => (string)GetValue(DeleteButtonTextProperty);
            set => SetValue(DeleteButtonTextProperty, value);
        }

        // =========================
        // UI Helper Methods
        // =========================

        public void SetDisplay(
            string name,
            string description,
            string icon,
            string totalItems,
            string sizeText,
            string level,
            string badgeText,
            Brush badgeColor,
            bool isDownloaded = false,
            string downloadDate = "")
        {
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            Icon = icon ?? string.Empty;
            TotalItems = totalItems ?? string.Empty;
            SizeText = sizeText ?? string.Empty;
            Level = level ?? string.Empty;
            BadgeText = badgeText ?? string.Empty;
            BadgeColor = badgeColor ?? Brushes.Gray;
            IsDownloaded = isDownloaded;
            DownloadDate = downloadDate ?? string.Empty;
        }

        public void SetButtonTexts(
            string downloadText = "💾 Load Data",
            string openText = "👁️ Preview",
            string deleteText = "🗑️ Delete")
        {
            DownloadButtonText = downloadText ?? string.Empty;
            OpenButtonText = openText ?? string.Empty;
            DeleteButtonText = deleteText ?? string.Empty;
        }

        public void ClearDisplay()
        {
            Name = string.Empty;
            Description = string.Empty;
            Icon = string.Empty;
            TotalItems = string.Empty;
            SizeText = string.Empty;
            Level = string.Empty;
            BadgeText = string.Empty;
            BadgeColor = Brushes.Gray;
            IsDownloaded = false;
            DownloadDate = string.Empty;

            DownloadButtonText = "💾 Load Data";
            OpenButtonText = "👁️ Preview";
            DeleteButtonText = "🗑️ Delete";
        }

        // =========================
        // Commented Old Logic
        // =========================

        /*
        // Logic cũ:
        // Constructor cũ nhận model TopicPackage và parent ViewModel.
        //
        // public PackageCard(TopicPackage topicPackage, OfflineModeViewModel parentVM)
        // {
        //     InitializeComponent();
        //     packageCardViewModel = new PackageCardViewModel(topicPackage, parentVM);
        //     DataContext = packageCardViewModel;
        // }
        //
        // Tạm bỏ vì bản hiện tại chỉ giữ FE/UI.
        */

        /*
        // Logic cũ:
        // DependencyProperty Package để bind nguyên model từ ngoài vào.
        //
        // public static readonly DependencyProperty PackageProperty =
        //     DependencyProperty.Register("Package", typeof(TopicPackage),
        //         typeof(PackageCard), new PropertyMetadata(null, OnPackageChanged));
        //
        // public TopicPackage Package
        // {
        //     get => (TopicPackage)GetValue(PackageProperty);
        //     set => SetValue(PackageProperty, value);
        // }
        //
        // Tạm bỏ vì đang không dùng model business.
        */

        /*
        // Logic cũ:
        // Callback khi Package thay đổi.
        //
        // private static void OnPackageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        // {
        // }
        //
        // Tạm bỏ vì bản FE hiện tại không xử lý model.
        */

        /*
        // Logic cũ:
        // Có class PackageCardViewModel riêng để cung cấp dữ liệu bind cho UI.
        // Bao gồm các property:
        // - Name
        // - Description
        // - Icon
        // - TotalItems
        // - Level
        // - IsDownloaded
        // - SizeText
        // - DownloadDate
        // - BadgeText
        // - BadgeColor
        //
        // Tạm bỏ vì bản này dùng DependencyProperty trực tiếp trên UserControl.
        */

        /*
        // Logic cũ:
        // Các nút trong XAML từng bind Command:
        // - DownloadPackageCommand
        // - OpenPackageCommand
        // - DeletePackageCommand
        //
        // Tạm bỏ vì đây là hành vi chức năng, không thuộc FE-only.
        */

        /*
        // Logic cũ:
        // OpenPackage()
        // DownloadPackage()
        // DeletePackage()
        //
        // Các hàm này liên quan tới ViewModel/service/nghiệp vụ.
        // Tạm bỏ qua.
        */

        /*
        // Ghi chú:
        // Nếu sau này cần nối lại dữ liệu thật:
        // 1. Có thể bind lại bằng ViewModel ngoài
        // 2. Hoặc truyền dữ liệu từ ngoài qua các DependencyProperty hiện tại
        // 3. Các Command nên đặt ở layer cha hoặc ViewModel riêng
        */
    }
}
