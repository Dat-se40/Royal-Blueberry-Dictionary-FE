using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Royal_Blueberry_Dictionary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isSidebarOpen = false;
        public MainWindow()
        {
            //InitializeComponent();
            //using (HttpClient httpClient = new HttpClient()
            //{
            //    BaseAddress = new Uri("http://localhost:8080/api/"),
            //    Timeout = TimeSpan.FromSeconds(30)
            //})
            //{
            //    testClient(httpClient); 
            //}
            //SearchService ss = App.serviceProvider.GetRequiredService<SearchService>();
            testPackage();
        }
        async void testClient(HttpClient httpClient) 
        {
            var response = httpClient.GetAsync("packages");
            var t = await response;
            var json = t.Content.ReadAsStringAsync().Result;
            Console.Write(json);
            try
            {
                var packages = JsonSerializer.Deserialize<List<Package>>(json, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
                packages?.ForEach(p =>
                {
                    if (p != null) Console.WriteLine($"Package: {p}");
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        async void testSearch(string word) 
        {
            TimeSpan startTime = DateTime.Now.TimeOfDay;
            var tool = App.serviceProvider.GetRequiredService<SearchService>();
            var res = await tool.searchAWord(word);
            TimeSpan endTime = DateTime.Now.TimeOfDay;
            Console.WriteLine((endTime - startTime).TotalSeconds + "seconds");
            Console.WriteLine($"Word: {res.Word}, Definition: {res.AudioUk}");
        }
        async void testPackage() 
        {
            try
            {
                PackageService packageService = App.serviceProvider.GetRequiredService<PackageService>();
                if (packageService != null) 
                {
                    var packages = await packageService.getAllPackages();
                    packages?.ForEach(p =>
                    {
                        if (p != null) Console.WriteLine($"Package: {p}");
                    });
                }else 
                {
                    Console.WriteLine("PackageService is null");
                }
                
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }

        }
        /// <summary>
        /// Toggle Sidebar (Hamburger button)
        /// </summary>
        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isSidebarOpen)
                CloseSidebar();
            else
                OpenSidebar();
        }

        /// <summary>
        /// Open Sidebar with animation
        /// </summary>
        private void OpenSidebar()
        {
            _isSidebarOpen = true;
            Overlay.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation
            {
                From = -280,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        /// <summary>
        /// Close Sidebar with animation
        /// </summary>
        private void CloseSidebar()
        {
            _isSidebarOpen = false;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = -280,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            animation.Completed += (s, e) => Overlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        /// <summary>
        /// Close sidebar when clicking overlay
        /// </summary>
        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseSidebar();
        }
    }
}