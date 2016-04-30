namespace PoMo.Server
{
    public struct PriceChange
    {
        public PriceChange(string ticker, decimal price)
        {
            this.Ticker = ticker;
            this.Price = price;
        }

        public decimal Price
        {
            get;
        }

        public string Ticker
        {
            get;
        }
    }
}