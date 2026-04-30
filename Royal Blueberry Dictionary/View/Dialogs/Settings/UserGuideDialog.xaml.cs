using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    public partial class UserGuideDialog : Window
    {
        public UserGuideDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
            LoadIntroContent();
        }

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string tag)
                return;

            foreach (var child in TabsPanel.Children)
            {
                if (child is Button btn)
                {
                    btn.Style = (Style)FindResource("TabButtonStyle");
                }
            }

            button.Style = (Style)FindResource("ActiveTabStyle");
            LoadContent(tag);
        }

        private void LoadContent(string tag)
        {
            ContentPanel.Children.Clear();

            switch (tag)
            {
                case "intro":
                    LoadIntroContent();
                    break;
                case "search":
                    LoadSearchContent();
                    break;
                case "manage":
                    LoadManageContent();
                    break;
                case "history":
                    LoadHistoryContent();
                    break;
                case "favourite":
                    LoadFavouriteContent();
                    break;
                case "theme":
                    LoadThemeContent();
                    break;
                case "sync":
                    LoadSyncContent();
                    break;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void LoadIntroContent()
        {
            AddSectionHeader("🎯 INTRODUCTION");

            AddBodyText("Royal Blueberry Dictionary is a smart English dictionary app designed to provide an effective vocabulary lookup and learning experience. The app is not only a lookup tool but also a companion that helps users build and manage their personal vocabulary library in a structured way.");

            AddBodyText("With a modern interface and useful learning features, the application supports daily study, vocabulary review, and customized learning workflows for different users.");

            AddSubHeader("✨ Key Features");
            AddBullet("✅ Look up words from reliable sources");
            AddBullet("✅ US 🇺🇸 and UK 🇬🇧 pronunciation");
            AddBullet("✅ Save words and manage meanings");
            AddBullet("✅ Organize vocabulary with tags");
            AddBullet("✅ Search history and favourite words");
            AddBullet("✅ Interface customization");
            AddBullet("✅ Account login and synchronization support");
            AddBullet("✅ Offline usage support");

            AddSubHeader("👥 Development Team");
            AddBodyText("Subject: Nhập môn công nghệ phần mềm");
            AddBodyText("Lecturer: ThS. Huỳnh Ngọc Tín");
            AddBodyText("Semester: Kỳ 2 - Năm học 2025-2026");
            AddBodyText("Members:");
            AddBullet("• Nguyễn Tấn Đạt");
            AddBullet("• Võ Nguyễn Thanh Hương");
            AddBullet("• Ngô Phương Hiền");
            AddBullet("• Nguyễn Quốc An");
            AddBullet("• Võ Văn Hải");
        }

        private void LoadSearchContent()
        {
            AddSectionHeader("🔍 WORD SEARCH");

            AddSubHeader("📖 Basic Lookup");
            AddBodyText("The search feature is the core of Royal Blueberry Dictionary. Users can enter a word in the search box and quickly receive matching suggestions and detailed results.");

            AddStep("Step 1: Enter the word you want to search");
            AddStep("Step 2: Select a suggestion or press Enter");
            AddBullet("• Suggestions appear while typing");
            AddBullet("• Results are based on matching words and similarity");

            AddStep("Step 3: View detailed word information");
            AddBullet("✅ Pronunciation: US 🇺🇸 and UK 🇬🇧");
            AddBullet("✅ Meanings by word type");
            AddBullet("✅ Examples and usage");
            AddBullet("✅ Related information if available");

            AddSubHeader("🔊 Pronunciation");
            AddBodyText("You can listen to pronunciation in different accents depending on available data.");
            AddBullet("• Click the speaker icon next to the pronunciation section");
            AddBullet("• US and UK pronunciation may both be available");

            AddSubHeader("💾 Save Words");
            AddBodyText("After searching, you can save the word to your personal vocabulary collection.");
            AddBullet("• Save the full word");
            AddBullet("• Save selected meanings only");
            AddBullet("• Assign tags during saving if needed");

            AddSubHeader("❤️ Favourite");
            AddBullet("• Click the heart icon to add or remove a word from favourites");
            AddBullet("• Favourite words are stored separately for fast review");

            AddSubHeader("🌐 Offline Use");
            AddBodyText("Some words can be stored locally for later offline access depending on your app flow and downloaded data.");
        }

        private void LoadManageContent()
        {
            AddSectionHeader("📚 MANAGE WORDS");

            AddSubHeader("📖 My Words");
            AddBodyText("My Words is your personal vocabulary library. Saved words are listed here for long-term learning and review.");

            AddStep("Access: Sidebar → My Words");

            AddSubHeader("🏷️ Tags");
            AddBodyText("Tags help group vocabulary into meaningful categories such as IELTS, Business, Daily Use, or custom topics.");

            AddStep("Create a tag");
            AddBullet("1. Open the tag creation action");
            AddBullet("2. Enter tag name");
            AddBullet("3. Choose icon and color");
            AddBullet("4. Save the tag");

            AddStep("Assign a tag");
            AddBullet("• Assign while saving a word");
            AddBullet("• Or assign later inside vocabulary management");

            AddStep("Delete a tag");
            AddBullet("• Remove unwanted tags from the tag management area");
            AddBullet("• Existing words remain, only the tag is removed");

            AddSubHeader("🔍 Filter and Search");
            AddBullet("• Filter by first letter");
            AddBullet("• Filter by word type");
            AddBullet("• Filter by tag");
            AddBullet("• Search quickly in saved words");

            AddSubHeader("🗑️ Delete Words");
            AddBullet("• Remove individual words from your personal library when no longer needed");
        }

        private void LoadHistoryContent()
        {
            AddSectionHeader("📜 SEARCH HISTORY");

            AddBodyText("The History page stores recently searched words so users can easily revisit them later.");

            AddStep("Access: Sidebar → History");

            AddSubHeader("✨ Features");
            AddBullet("✅ View recently searched words");
            AddBullet("✅ Open a word again from history");
            AddBullet("✅ Delete one item");
            AddBullet("✅ Clear all history");

            AddSubHeader("🗑️ Delete History");
            AddBullet("• Delete a single word from the list");
            AddBullet("• Or use the clear-all action to remove the entire history");
        }

        private void LoadFavouriteContent()
        {
            AddSectionHeader("❤️ FAVOURITE WORDS");

            AddBodyText("Favourite Words contains vocabulary you want to prioritize for review, such as difficult words or words you use often.");

            AddStep("Access: Sidebar → Favourite Words");

            AddSubHeader("✨ Features");
            AddBullet("✅ View all favourited words");
            AddBullet("✅ Search and filter");
            AddBullet("✅ Open word details quickly");

            AddSubHeader("💡 Usage Suggestion");
            AddBodyText("Use My Words as the main storage area and Favourite Words as a short review list for priority learning.");
        }

        private void LoadThemeContent()
        {
            AddSectionHeader("🎨 INTERFACE CUSTOMIZATION");

            AddBodyText("The application supports interface customization so users can adjust appearance based on their preference.");

            AddStep("Access: Sidebar → Settings");

            AddSubHeader("🌓 Theme Mode");
            AddBullet("• Light mode");
            AddBullet("• Dark mode");
            AddBullet("• System mode");

            AddSubHeader("🎨 Color Theme");
            AddBullet("• Use the default theme");
            AddBullet("• Choose preset themes");
            AddBullet("• Create custom color themes");

            AddSubHeader("🔤 Font");
            AddBullet("• Choose a different font family");
            AddBullet("• Adjust font size");
            AddBullet("• Apply changes across the app");

            AddSubHeader("♻️ Reset");
            AddBullet("• Reset colors or font back to default when needed");
        }

        private void LoadSyncContent()
        {
            AddSectionHeader("🔐 LOGIN & SYNC");

            AddSubHeader("🌟 Why Sign In?");
            AddBodyText("Signing in allows your application data and profile-related features to be associated with your account and supported by the backend flow.");

            AddSubHeader("🔑 Sign In");
            AddBullet("1. Open the welcome or account flow");
            AddBullet("2. Choose login");
            AddBullet("3. Complete authentication");
            AddBullet("4. Return to the app with your session");

            AddSubHeader("☁️ Synchronization");
            AddBodyText("Depending on the implemented backend features, saved data and account-related information can be restored or synchronized for continued usage.");

            AddSubHeader("👤 Account");
            AddBullet("• View your account information");
            AddBullet("• Access profile-related actions");
            AddBullet("• Sign out when needed");
        }

        private void AddSectionHeader(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("SectionHeaderStyle")
            });
        }

        private void AddSubHeader(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("SubHeaderStyle")
            });
        }

        private void AddBodyText(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("BodyTextStyle")
            });
        }

        private void AddStep(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("StepTextStyle")
            });
        }

        private void AddBullet(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("BulletStyle")
            });
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
