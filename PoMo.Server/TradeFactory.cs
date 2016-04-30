using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PoMo.Common;
using PoMo.Data;
using PoMo.Data.Models;

namespace PoMo.Server
{
    public interface ITradeFactory
    {
        event EventHandler<DataEventArgs<IReadOnlyCollection<Trade>>> TradesCreated;
    }

    internal sealed class TradeFactory : ITradeFactory, IDisposable
    {
        private const int Interval = 20000;

        private readonly DataContext _dataContext;
        private readonly IMarketDataProvider _marketDataProvider;
        private readonly string[] _portfolioIds;
        private readonly Dictionary<int, decimal> _prices = new Dictionary<int, decimal>();
        private readonly Random _random = new Random();
        private readonly List<Security> _securities = new List<Security>();
        private readonly Timer _timer;

        public TradeFactory(DataContext dataContext, IMarketDataProvider marketDataProvider)
        {
            this._portfolioIds = dataContext.Portfolios.Select(portfolio => portfolio.Id).ToArray();
            foreach (Security security in dataContext.Securities.OrderBy(security => security.Ticker))
            {
                this._securities.Add(security);
                this._prices.Add(security.Id, security.OpeningPrice);
            }
            this._dataContext = dataContext;
            this._marketDataProvider = marketDataProvider;
            this._timer = new Timer(this.Timer_Elapsed, null, TradeFactory.Interval, TradeFactory.Interval);
            this._marketDataProvider.PricesChanged += this.MarketDataProvider_PricesChanged;
        }

        public event EventHandler<DataEventArgs<IReadOnlyCollection<Trade>>> TradesCreated;

        public void Dispose()
        {
            this._marketDataProvider.PricesChanged -= this.MarketDataProvider_PricesChanged;
            using (this._timer)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void MarketDataProvider_PricesChanged(object sender, DataEventArgs<IReadOnlyCollection<PriceChange>> e)
        {
            lock (this)
            {
                foreach (PriceChange priceChange in e.Data)
                {
                    int index = this._securities.BinarySearchByValue(priceChange.Ticker, security => security.Ticker);
                    this._prices[this._securities[index].Id] = priceChange.Price;
                }
            }
        }

        private void OnTradesCreated(IReadOnlyCollection<Trade> trades)
        {
            this.TradesCreated?.Invoke(this, DataEventArgs.Create(trades));
        }

        private void Timer_Elapsed(object state)
        {
            int tradeCount = this._random.Next(0, 10);
            if (tradeCount == 0)
            {
                return;
            }
            Trade[] trades = new Trade[tradeCount];
            lock (this)
            {
                DateTime tradeDate = DateTime.Now;
                for (int index = 0; index < trades.Length; index++)
                {
                    string portfolioId = this._portfolioIds[this._random.Next(0, this._portfolioIds.Length)];
                    Security security = this._securities[this._random.Next(0, this._securities.Count)];
                    int quantity = this._random.Next(-5000, 5000);
                    trades[index] = this._dataContext.Trades.Add(new Trade
                    {
                        Security = security,
                        SecurityId = security.Id,
                        Price = this._prices[security.Id],
                        PortfolioId = portfolioId,
                        Quantity = quantity,
                        TradeDate = tradeDate
                    });
                }
                this._dataContext.SaveChanges();
            }
            this.OnTradesCreated(trades);
        }
    }
}