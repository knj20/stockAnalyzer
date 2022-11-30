
using Newtonsoft.Json;
using stockAnalyzer.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace stockAnalyzer.Windowss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
        private Stopwatch stopwatch = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            BeforeLoadingStockData();

            await GetStocks();


            AfterLoadingStockData();

            /* exemple about diffrence between Task.Result and await 
                Debug.WriteLine($"Task started");

              var x = Go().Result; USING Task.Result or Task.Wait() caused a deadlock  it blocks the thread untill the task is completed

                 var x = await Go()  await des not bolck the current thrad  ;


                Debug.WriteLine("$Task completed ");

            **/
        }

        private async Task GetStocks()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var respsonse = await client.GetAsync($"{API_URL}/{StockIdentifier.Text}");
                    var content = await respsonse.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<IEnumerable<Price>>(content);
                    Stocks.ItemsSource = data;
                }
                catch (Exception ex)
                {
                    Notes.Text = ex.Message;
                }
            }

            /* var client = new WebClient();

           var content = client.DownloadString($"{API_URL}/{StockIdentifier.Text}");

           // Simulate that the web call takes a very long time
           Thread.Sleep(10000);
          */

            // var data = JsonConvert.DeserializeObject<IEnumerable<Price>>(content);

            // Stocks.ItemsSource = data;
        }

        private async Task<HttpResponseMessage> Go()
        {
            var client = new HttpClient();
            var response =   await client.GetAsync($"{API_URL}/{StockIdentifier.Text}");
            return response;
        }








        private void BeforeLoadingStockData()
        {
            stopwatch.Restart();
            StockProgress.Visibility = Visibility.Visible;
            StockProgress.IsIndeterminate = true;
        }

        private void AfterLoadingStockData()
        {
            StocksStatus.Text = $"Loaded stocks for {StockIdentifier.Text} in {stopwatch.ElapsedMilliseconds}ms";
            StockProgress.Visibility = Visibility.Hidden;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });

            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
