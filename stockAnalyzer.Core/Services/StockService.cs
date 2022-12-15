using Newtonsoft.Json;
using stockAnalyzer.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.Core.Services;

public interface IStockService
{
    Task<IEnumerable<Price>> GetStockPricesFor(string stockIdentifier,
        CancellationToken cancellationToken);
}

public class StockService : IStockService
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
    private int i = 0;

    public async Task<IEnumerable<Price>>
        GetStockPricesFor(string stockIdentifier,
                          CancellationToken cancellationToken)
    {
        // Simulate that each time this method is called
        // it takes a little bit longer.
        //
        // DO NOT DO THIS IN PRODUCTION...
        await Task.Delay((i++) * 1000);

        using (var client = new HttpClient())
        {
            var result = await client.GetAsync($"{API_URL}/{stockIdentifier}",
                cancellationToken);

            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<Price>>(content);
        }
    }
}

public class MockStockService : IStockService
{
    public Task<IEnumerable<Price>> GetStockPricesFor(string stockIdentifier, CancellationToken cancellationToken)
    {
        var stocks = new List<Price>()
        {
            new()
            {
                Identifier = "MSFT",
                Change = 0.5m,
                ChangePercent = 0.7m
            },
            new()
            {
                Identifier = "MSFT",
                Change = 0.4m,
                ChangePercent = 0.4m
            },
            new()
            {
                Identifier = "MSFT",
                Change = 0.2m,
                ChangePercent = 0.3m
            },
            new()
            {
                Identifier = "GOOGL",
                Change = 0.22m,
                ChangePercent = 0.73m
            },
        };

        var task = Task.FromResult(stocks.Where(s => s.Identifier == stockIdentifier));
        return task;

    }
}
