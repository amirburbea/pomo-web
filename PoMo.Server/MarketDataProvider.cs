using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PoMo.Data;
using PoMo.Data.Models;

namespace PoMo.Server
{
    public interface IMarketDataProvider
    {
        event EventHandler<DataEventArgs<IReadOnlyCollection<PriceChange>>> PricesChanged;
    }

    internal sealed class MarketDataProvider : IMarketDataProvider, IDisposable
    {
        private readonly IList<decimal> _prices;
        private readonly Random _random;
        private readonly IList<string> _tickers;
        private readonly Timer _timer;

        public MarketDataProvider(IDataContext dataContext)
        {
            this._random = new Random();
            this._tickers = new List<string>();
            this._prices = new List<decimal>();
            foreach (Security security in dataContext.Securities.OrderBy(security => security.Ticker))
            {
                this._tickers.Add(security.Ticker);
                this._prices.Add(security.OpeningPrice);
            }
            this._timer = new Timer(this.Timer_Elapsed, null, 3000, 3000);
        }

        public event EventHandler<DataEventArgs<IReadOnlyCollection<PriceChange>>> PricesChanged;

        public void Dispose()
        {
            using (this._timer)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void OnPricesChanged(IReadOnlyCollection<PriceChange> priceChanges)
        {
            this.PricesChanged?.Invoke(this, DataEventArgs.Create(priceChanges));
        }

        private void Timer_Elapsed(object state)
        {
            int[] ordinals = new int[this._random.Next(5, 10)];
            this._random.PopulateRandomOrdinals(ordinals, this._tickers.Count);
            PriceChange[] priceChanges = new PriceChange[ordinals.Length];
            for (int index = 0; index < ordinals.Length; index++)
            {
                int ordinal = ordinals[index];
                decimal percent = this._random.Next(80, 125) / 100m;
                decimal newPrice = this._prices[ordinal] *= percent;
                priceChanges[index] = new PriceChange(this._tickers[ordinal], newPrice);
            }
            this.OnPricesChanged(priceChanges);
        }
    }
}