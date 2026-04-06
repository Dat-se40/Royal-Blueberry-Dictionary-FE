using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    /// <summary>
    /// Dialog để tạo tag mới
    /// </summary>
    public partial class TagPickerDialog : Window
    {
        private readonly ITagRepository _tagRepository;
        private readonly string _userId;
        public string CreatedTagId { get; private set; } = string.Empty;

        private string _selectedIcon = "🏷️";
        private string _selectedColor = "#2D4ACC";

        private readonly string[] _availableIcons = new[]
        {
            "🏷️", "📚", "🎯", "💼", "💬", "🎓", "🌟", "💡",
            "📝", "⭐", "🔥", "💎", "🚀", "🎨", "🎮", "📖",
            "🌈", "🎵", "🏆", "🌸", "🎪", "🎭", "🎬", "📱"
        };

        private readonly string[] _availableColors = new[]
        {
            "#2D4ACC", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6",
            "#EC4899", "#06B6D4", "#84CC16", "#F97316", "#6366F1"
        };

        public TagPickerDialog(string? userId = null)
        {
            InitializeComponent();
            _tagRepository = App.serviceProvider.GetRequiredService<ITagRepository>();
            _userId = string.IsNullOrWhiteSpace(userId) ? "GUEST" : userId;

            LoadIcons();
            LoadColors();
            ApplyGlobalFont();
            UpdatePreview();
        }

        private void LoadIcons()
        {
            IconsPanel.Children.Clear();

            foreach (var icon in _availableIcons)
            {
                var btn = CreateIconButton(icon);
                IconsPanel.Children.Add(btn);
            }
            UpdateIconSelection();
        }

        private Button CreateIconButton(string icon)
        {
            var btn = new Button
            {
                Content = icon,
                Width = 50,
                Height = 50,
                FontSize = 24,
                Margin = new Thickness(5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = icon
            };
            btn.SetResourceReference(Button.ForegroundProperty, "TextColor");
            btn.Style = FindResource("IconButtonStyle") as Style;

            btn.Click += (s, e) =>
            {
                _selectedIcon = icon;
                UpdateIconSelection();
                UpdatePreview();
            };

            return btn;
        }

        private void UpdateIconSelection()
        {
            foreach (Button btn in IconsPanel.Children)
            {
                if (btn.Tag?.ToString() == _selectedIcon)
                {
                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_selectedColor));
                    btn.BorderThickness = new Thickness(3);
                }
                else
                {
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
                    btn.BorderThickness = new Thickness(2);
                }
            }
        }

        private void LoadColors()
        {
            ColorsPanel.Children.Clear();

            foreach (var color in _availableColors)
            {
                var btn = CreateColorButton(color);
                ColorsPanel.Children.Add(btn);
            }
            UpdateColorSelection();
        }

        private Button CreateColorButton(string colorHex)
        {
            var btn = new Button
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(2),
                Tag = colorHex
            };

            btn.Template = CreateColorButtonTemplate();

            btn.Click += (s, e) =>
            {
                _selectedColor = colorHex;
                UpdateColorSelection();
                UpdatePreview();
            };

            return btn;
        }

        private ControlTemplate CreateColorButtonTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            factory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            factory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));

            var template = new ControlTemplate(typeof(Button));
            template.VisualTree = factory;

            return template;
        }

        private void UpdateColorSelection()
        {
            foreach (Button btn in ColorsPanel.Children)
            {
                if (btn.Tag?.ToString() == _selectedColor)
                {
                    btn.BorderBrush = Brushes.Black;
                    btn.BorderThickness = new Thickness(3);
                }
                else
                {
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
                    btn.BorderThickness = new Thickness(2);
                }
            }

            UpdateIconSelection(); // Update icon border color
        }

        private void UpdatePreview()
        {
            PreviewIcon.Text = _selectedIcon;
            PreviewName.Text = string.IsNullOrWhiteSpace(TagNameInput.Text)
                ? "Tag name"
                : TagNameInput.Text.Trim();
            PreviewBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_selectedColor));
        }

        private void TagNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            ValidateInput();
        }

        private void ValidateInput()
        {
            bool isValid = !string.IsNullOrWhiteSpace(TagNameInput.Text);
            CreateButton.IsEnabled = isValid;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var tagName = TagNameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(tagName))
            {
                MessageBox.Show("Please enter a tag name!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = await _tagRepository.GetAllTagsAsync(_userId);
            if (existing.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This tag already exists!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newTag = new Tag
            {
                UserId = _userId,
                Name = tagName,
                Icon = _selectedIcon,
                Color = _selectedColor,
                LastModifiedAt = DateTime.UtcNow,
                IsDirty = true
            };

            await _tagRepository.AddTagAsync(newTag);
            await _tagRepository.SaveChangesAsync();
            CreatedTagId = newTag.Id;

            MessageBox.Show($"✅ Tag '{tagName}' created successfully!", "Completed successfully",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Thêm font chữ
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }
    }
}
