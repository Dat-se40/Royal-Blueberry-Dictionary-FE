using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        public MainWindow()
        {
            InitializeComponent();
            //using (HttpClient httpClient = new HttpClient() 
            //{
            //    BaseAddress = new Uri("http://localhost:8080/api/"),
            //    Timeout = TimeSpan.FromSeconds(30)
            //}) 
            //{
            //    var response =  httpClient.GetAsync("packages");
            //    var t = response.Result;
            //     var json = t.Content.ReadAsStringAsync().Result;
            //    Console.Write(json);
            //}
            
        }
        async void testSearch(string word) 
        {
            TimeSpan startTime = DateTime.Now.TimeOfDay;
            var tool = App.serviceProvider.GetRequiredService<SearchService>();
            var res = await tool.searchAWord(word);
            TimeSpan endTime = DateTime.Now.TimeOfDay;
            Console.WriteLine((endTime - startTime).TotalSeconds + "seconds");
            Console.WriteLine($"Word: {res.Word}, Definition: {res.Meanings}");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            testSearch("hello");
            
        }
    }
}