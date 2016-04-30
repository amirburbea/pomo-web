using System;
using PoMo.Common;

namespace PoMo.Server
{
    public sealed class Position : NotifierBase
    {
        private decimal _cash;
        private decimal _costBasis;
        private decimal _lastPrice;
        private decimal _markToMarket;
        private decimal _pnl;
        private int _quantity;

        public decimal Cash
        {
            get
            {
                return this._cash;
            }
            set
            {
                this.SetValue(ref this._cash, value);
            }
        }

        public decimal CostBasis
        {
            get
            {
                return this._costBasis;
            }
            set
            {
                this.SetValue(ref this._costBasis, value);
            }
        }

        public string Description
        {
            get;
            set;
        }

        public decimal LastPrice
        {
            get
            {
                return this._lastPrice;
            }
            set
            {
                this.SetValue(ref this._lastPrice, value);
            }
        }

        public decimal MarkToMarket
        {
            get
            {
                return this._markToMarket;
            }
            set
            {
                this.SetValue(ref this._markToMarket, value);
            }
        }

        public decimal Pnl
        {
            get
            {
                return this._pnl;
            }
            set
            {
                this.SetValue(ref this._pnl, value);
            }
        }

        public int Quantity
        {
            get
            {
                return this._quantity;
            }
            set
            {
                this.SetValue(ref this._quantity, value);
            }
        }

        public string Ticker
        {
            get;
            set;
        }

        public object this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case nameof(this.Cash):
                        return this.Cash;
                    case nameof(this.CostBasis):
                        return this.CostBasis;
                    case nameof(this.Description):
                        return this.Description;
                    case nameof(this.Pnl):
                        return this.Pnl;
                    case nameof(this.MarkToMarket):
                        return this.MarkToMarket;
                    case nameof(this.Quantity):
                        return this.Quantity;
                    case nameof(this.Ticker):
                        return this.Ticker;
                    case nameof(this.LastPrice):
                        return this.LastPrice;
                }
                throw new ArgumentException($"Property {propertyName} not handled.", nameof(propertyName));
            }
        }
    }
}