using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using PoMo.Common;
using PoMo.Common.DataObjects;
using PoMo.Data;
using PoMo.Data.Models;

namespace PoMo.Server
{
    public interface IFirmData : ISubscribable
    {
        event EventHandler<DataEventArgs<RowChangeBase[]>> SummaryChanged;

        IReadOnlyCollection<IPortfolioData> Portfolios
        {
            get;
        }

        DataTable GetSummary();

        bool TryGetPortfolio(string portfolioId, out IPortfolioData data);
    }

    internal sealed class FirmData : IFirmData, IDisposable
    {
        private readonly Dictionary<string, DataTable> _dataTables = new Dictionary<string, DataTable>();
        private readonly IMarketDataProvider _marketDataProvider;
        private readonly PortfolioDataCollection _portfolios = new PortfolioDataCollection();
        private readonly List<RowChangeBase> _rowChanges = new List<RowChangeBase>();
        private readonly HashSet<string> _subscribers = new HashSet<string>();
        private readonly DataTable _summary;
        private readonly Timer _timer;
        private readonly ITradeFactory _tradeFactory;

        public FirmData(IDataContext dataContext, IMarketDataProvider marketDataProvider, ITradeFactory tradeFactory)
        {
            this._marketDataProvider = marketDataProvider;
            this._tradeFactory = tradeFactory;
            this._summary = new DataTable
            {
                TableName = nameof(FirmData),
                Columns =
                {
                    { "PortfolioId", typeof(string) },
                    { "PortfolioName", typeof(string) },
                    { "Pnl", typeof(decimal) }
                }
            };
            this._summary.PrimaryKey = new[] { this._summary.Columns[0] };
            foreach (Portfolio portfolio in dataContext.Portfolios.OrderBy(portfolio => portfolio.Id))
            {
                PortfolioData portfolioData = new PortfolioData(portfolio, dataContext);
                DataTable dataTable = portfolioData.GetData();
                DataRow summaryRow = this._summary.NewRow();
                summaryRow.SetField("PortfolioId", portfolio.Id);
                summaryRow.SetField("PortfolioName", portfolio.Name);
                summaryRow.SetField("Pnl", dataTable.Rows.Cast<DataRow>().Sum(dataRow => dataRow.Field<decimal>("Pnl")));
                this._summary.Rows.Add(summaryRow);
                this._dataTables.Add(portfolio.Id, dataTable);
                portfolioData.DataChanged += this.PortfolioData_DataChanged;
                this._portfolios.Add(portfolioData);
            }
            this._tradeFactory.TradesCreated += this.TradeFactory_TradesCreated;
            this._marketDataProvider.PricesChanged += this.MarketDataProvider_PricesChanged;
            this._timer = new Timer(this.Timer_Elapsed, null, 5000, 4000);
        }

        public event EventHandler<DataEventArgs<RowChangeBase[]>> SummaryChanged;

        IReadOnlyCollection<IPortfolioData> IFirmData.Portfolios => this._portfolios;

        public void Dispose()
        {
            this._tradeFactory.TradesCreated -= this.TradeFactory_TradesCreated;
            this._marketDataProvider.PricesChanged -= this.MarketDataProvider_PricesChanged;
            foreach (PortfolioData portfolioData in this._portfolios)
            {
                portfolioData.DataChanged -= this.PortfolioData_DataChanged;
            }
            using (this._timer)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        DataTable IFirmData.GetSummary()
        {
            lock (this._summary)
            {
                return this._summary.FullClone();
            }
        }

        bool IFirmData.TryGetPortfolio(string portfolioId, out IPortfolioData data)
        {
            PortfolioData value;
            if (this._portfolios.TryGetPortfolio(portfolioId, out value))
            {
                data = value;
                return true;
            }
            data = null;
            return false;
        }

        string[] ISubscribable.GetSubscribers()
        {
            lock (this._subscribers)
            {
                return this._subscribers.Count == 0 ? (string[])Enumerable.Empty<string>() : this._subscribers.ToArray();
            }
        }

        bool ISubscribable.Subscribe(string subscriberId, out int subscriberCount)
        {
            lock (this._subscribers)
            {
                bool added = this._subscribers.Add(subscriberId);
                subscriberCount = this._subscribers.Count;
                return added;
            }
        }

        bool ISubscribable.Unsubscribe(string subscriberId, out int subscriberCount)
        {
            lock (this._subscribers)
            {
                bool removed = this._subscribers.Remove(subscriberId);
                subscriberCount = this._subscribers.Count;
                return removed;
            }
        }

        private void MarketDataProvider_PricesChanged(object sender, DataEventArgs<IReadOnlyCollection<PriceChange>> e)
        {
            foreach (PortfolioData portfolioData in this._portfolios)
            {
                portfolioData.ProcessPriceChanges(e.Data);
            }
        }

        private void PortfolioData_DataChanged(object sender, DataEventArgs<RowChangeBase[]> e)
        {
            PortfolioData portfolioData = (PortfolioData)sender;
            decimal pnl;
            lock (this._summary)
            {
                DataTable dataTable = this._dataTables[portfolioData.PortfolioId];
                DataRow summaryRow = this._summary.Rows.Find(portfolioData.PortfolioId);
                pnl = summaryRow.Field<decimal>("Pnl");
                foreach (RowChangeBase rowChange in e.Data)
                {
                    if (rowChange.ChangeType == RowChangeType.Added)
                    {
                        DataRow dataRow = dataTable.Rows.Add(((RowAdded)rowChange).Data);
                        pnl += dataRow.Field<decimal>("Pnl");
                    }
                    else
                    {
                        DataRow dataRow = dataTable.Rows.Find(rowChange.RowKey);
                        decimal rowPnl = dataRow.Field<decimal>("Pnl");
                        switch (rowChange.ChangeType)
                        {
                            case RowChangeType.Removed:
                                pnl -= rowPnl;
                                dataTable.Rows.Remove(dataRow);
                                break;
                            case RowChangeType.ColumnsChanged:
                                foreach (ColumnChange columnChange in ((RowColumnsChanged)rowChange).ColumnChanges)
                                {
                                    if (columnChange.ColumnName == "Pnl")
                                    {
                                        pnl += (decimal)columnChange.Value - rowPnl;
                                    }
                                    dataRow[columnChange.ColumnName] = columnChange.Value;
                                }
                                break;
                        }
                    }
                }
                dataTable.AcceptChanges();
                summaryRow.SetField("Pnl", pnl);
                summaryRow.AcceptChanges();
            }
            lock (this._rowChanges)
            {
                int index = this._rowChanges.BinarySearchByValue(portfolioData.PortfolioId, change => (string)change.RowKey);
                if (index < 0)
                {
                    this._rowChanges.Insert(~index, new RowColumnsChanged
                    {
                        RowKey = portfolioData.PortfolioId,
                        ColumnChanges =
                        {
                            new ColumnChange
                            {
                                ColumnName = "Pnl",
                                Value = pnl
                            }
                        }
                    });
                }
                else
                {
                    RowColumnsChanged rowChange = (RowColumnsChanged)this._rowChanges[index];
                    rowChange.ColumnChanges[0].Value = pnl;
                }
            }
        }

        private void Timer_Elapsed(object state)
        {
            RowChangeBase[] changes;
            lock (this._rowChanges)
            {
                if (this._rowChanges.Count == 0)
                {
                    return;
                }
                changes = this._rowChanges.ToArray();
                this._rowChanges.Clear();
            }
            this.SummaryChanged?.Invoke(this, DataEventArgs.Create(changes));
        }

        private void TradeFactory_TradesCreated(object sender, DataEventArgs<IReadOnlyCollection<Trade>> e)
        {
            foreach (IGrouping<string, Trade> group in e.Data.GroupBy(trade => trade.PortfolioId))
            {
                this._portfolios[group.Key].ProcessTrades(group);
            }
        }

        private sealed class PortfolioDataCollection : KeyedCollection<string, PortfolioData>
        {
            public PortfolioDataCollection()
                : base(StringComparer.Ordinal, 0)
            {
            }

            public bool TryGetPortfolio(string portfolioId, out PortfolioData data)
            {
                if (this.Dictionary != null)
                {
                    return this.Dictionary.TryGetValue(portfolioId, out data);
                }
                data = null;
                return false;
            }

            protected override string GetKeyForItem(PortfolioData item)
            {
                return item.PortfolioId;
            }
        }
    }
}