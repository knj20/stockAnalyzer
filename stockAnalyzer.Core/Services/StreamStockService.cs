using stockAnalyzer.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stockAnalyzer.Core.Services
{
    public interface IStreamStockService
    {
        public IAsyncEnumerable<Price> GetAllPrices(CancellationToken cancellationToken = default);
    }

    public class MockStteamStockService : IStreamStockService
    {
        public async IAsyncEnumerable<Price> GetAllPrices(CancellationToken cancellationToken = default)
        {
            await Task.Delay(500, cancellationToken);
            yield return new Price { Identifier = "MSFT", Change = 0.5m, ChangePercent = 0.7m };

            await Task.Delay(500, cancellationToken);
            yield return new Price { Identifier = "MSFT", Change = 0.55m, ChangePercent = 0.57m };

            await Task.Delay(500, cancellationToken);
            yield return new Price { Identifier = "MSFT", Change = 0.445m, ChangePercent = 0.37m };

            await Task.Delay(500, cancellationToken);
            yield return new Price { Identifier = "GOOGL", Change = 0.225m, ChangePercent = 0.547m };

            await Task.Delay(500, cancellationToken);
            yield return new Price { Identifier = "GOOGL", Change = 0.445m, ChangePercent = 0.027m };

        }
    }

    public class DiskStreamStockService : IStreamStockService
    {
        public async IAsyncEnumerable<Price> GetAllPrices(CancellationToken cancellationToken = default)
        {
            using var stream = new StreamReader(File.OpenRead("C:\\Learning\\DotnetCore\\stockAnalyzer\\stockAnalyzer.Windowss\\StockPrices_Small.csv"));

            await stream.ReadLineAsync();

            while(await stream.ReadLineAsync() is string line)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                yield return Price.FromCSV(line);
            }
        }
    }
}
