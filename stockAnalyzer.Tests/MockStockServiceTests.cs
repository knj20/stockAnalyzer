using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.Core.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace stockAnalyzer.Tests
{
    [TestClass]
    public class MockStockServiceTests
    {
        [TestMethod]
        public async Task Can_load_all_MSFT_stocks()
        {
            var service = new MockStockService();
            var stocks = await service.GetStockPricesFor("MSFT", CancellationToken.None);

            Assert.AreEqual(3, stocks.Count());
        }
    }
}