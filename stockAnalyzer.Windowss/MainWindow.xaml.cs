
using Newtonsoft.Json;
using stockAnalyzer.Core.Domain.Entities;
using stockAnalyzer.Core.Services;
using StockAnalyzer.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        CancellationTokenSource? cancellationTokenSource;
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BeforeLoadingStockData();

                var identifiers = StockIdentifier.Text.Split(',', ' ');
                var data = new ObservableCollection<Price>();
                Stocks.ItemsSource = data;

                var service = new DiskStreamStockService();
                var prices = service.GetAllPrices();

                await foreach(var price in prices.WithCancellation(CancellationToken.None))
                {
                    if(identifiers.Contains(price.Identifier))
                    {
                        data.Add(price);
                    }
                }
                // var data = await GetStocks(StockIdentifier.Text);
                // Stocks.ItemsSource = data;

            }
            catch(Exception ex)
            {
                Notes.Text = ex.Message;
            }
            finally
            {
                AfterLoadingStockData();
                // cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private async Task<IEnumerable<Price>> GetStocks(string identifier)
        {
            var service = new StockService();
            var data = await service.GetStockPricesFor(identifier, CancellationToken.None);

            Notes.Text = "Stocks loaded!";

            return data.Take(5);
        }


        private async Task SearchUsingTaskParallelLib()
        {
            if (cancellationTokenSource is not null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;

                Search.Content = "Search";
                return;
            }

            cancellationTokenSource = new();
            cancellationTokenSource.Token.Register(() =>
            {
                Notes.Text = "Cancellation requested";
            });

            Search.Content = "Cancel";
            BeforeLoadingStockData();

            try
            {
                var identifiers = StockIdentifier.Text.Split(',', ' ');

                var service = new StockService();

                var loadingTasks = new List<Task<IEnumerable<Price>>>();

                var stocks = new ConcurrentBag<Price>();

                foreach (var identifier in identifiers)
                {
                    var loadTask = service.GetStockPricesFor(identifier, cancellationTokenSource.Token);

                    loadTask = loadTask.ContinueWith(t =>
                    {
                        var firstStocks = t.Result.Take(5);

                        foreach (var stock in firstStocks)
                        {
                            stocks.Add(stock);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            Stocks.ItemsSource = stocks.ToArray();
                        });

                        return firstStocks;
                    }, cancellationTokenSource.Token);

                    loadingTasks.Add(loadTask);
                    var allStocks = await Task.WhenAll(loadingTasks);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                AfterLoadingStockData();
                // cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            /*
            try {

            var identifiers = StockIdentifier.Text.Split(',', ' ');

            var service = new MockStockService();

            var loadingTasks = new List<Task<IEnumerable<Price>>>();


            foreach(var identifier in identifiers)
            {
                var loadTask = service.GetStockPricesFor(identifier, cancellationTokenSource.Token);
                loadingTasks.Add(loadTask);
            }

            var timeout = Task.Delay(120000);
            var allStocks =  Task.WhenAll(loadingTasks);

            var completedTask = await Task.WhenAny(timeout, allStocks);

            if(completedTask == timeout)
            {
                cancellationTokenSource.Cancel();
                throw new OperationCanceledException("Time out!");
            }

            Stocks.ItemsSource = allStocks.Result.SelectMany(x => x);
            }
            catch
            {

            }
            finally
            {
                AfterLoadingStockData();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            } */


            /* var loadLinesTask = SearchForStocks(cancellationTokenSource.Token);

              loadLinesTask.ContinueWith(t =>
              {
                  Dispatcher.Invoke(() =>
                  {
                      Notes.Text = t.Exception?.InnerException?.Message;
                  });
              }, TaskContinuationOptions.OnlyOnFaulted);

              var processTask = loadLinesTask.ContinueWith((completedTask) =>
              {
                  var lines = completedTask.Result;

                  var data = new List<Price>();

                  foreach (var line in lines.Skip(1))
                  {
                      var price = Price.FromCSV(line);
                      data.Add(price);
                  }

                  Dispatcher.Invoke(() =>
                  {
                      Stocks.ItemsSource = data.Where(p => p.Identifier == StockIdentifier.Text);

                  });

              }, TaskContinuationOptions.OnlyOnRanToCompletion);
             

            processTask.ContinueWith((_) =>
            {
                Dispatcher.Invoke(() =>
                {
                    AfterLoadingStockData();
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                });

            });

             */

            // await GetStocks();





            /* exemple about diffrence between Task.Result and await 
                Debug.WriteLine($"Task started");

              var x = Go().Result; USING Task.Result or Task.Wait() caused a deadlock  it blocks the thread untill the task is completed

                 var x = await Go()  await des not bolck the current thrad  ;


                Debug.WriteLine("$Task completed ");

            **/
        }

        private Task<List<string>> SearchForStocks(CancellationToken cancellationToken)
        {
            return Task.Run(async() =>
            {
                // var lines = File.ReadAllLines("C:\\Learning\\DotnetCore\\stockAnalyzer\\stockAnalyzer.Windowss\\StockPrices_Small.csv");
                using var stream = new StreamReader(File.OpenRead("C:\\Learning\\DotnetCore\\stockAnalyzer\\stockAnalyzer.Windowss\\StockPrices_Small.csv"));

                var lines = new List<string>();

                while(await stream.ReadLineAsync() is string line)
                {
                    if(cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    lines.Add(line);
                }

                return lines;
            }, cancellationToken);
        }

        private async Task GetStocks(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var respsonse = await client.GetAsync($"{API_URL}/{StockIdentifier.Text}", cancellationToken);
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
