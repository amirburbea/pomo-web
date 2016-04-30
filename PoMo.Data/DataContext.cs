using System.Data.Entity;
using System.Linq;
using PoMo.Data.Models;

namespace PoMo.Data
{
    public interface IDataContext
    {
        IQueryable<Portfolio> Portfolios
        {
            get;
        }

        IQueryable<Security> Securities
        {
            get;
        }

        IQueryable<Trade> Trades
        {
            get;
        }
    }

    public interface ITradeRepository
    {
        Trade AddTrade(Trade trade);

        void SaveChanges();
    }

    public sealed class DataContext : DbContext, IDataContext, ITradeRepository
    {
        public DataContext()
            : base(ConnectionStringMethods.GetConnectionString())
        {
            Database.SetInitializer<DataContext>(null);
            this.Configuration.LazyLoadingEnabled = this.Configuration.ProxyCreationEnabled = false;
        }

        IQueryable<Security> IDataContext.Securities => this.Securities;

        IQueryable<Trade> IDataContext.Trades => this.Trades;

        IQueryable<Portfolio> IDataContext.Portfolios => this.Portfolios;

        public DbSet<Portfolio> Portfolios => this.Set<Portfolio>();

        public DbSet<Security> Securities => this.Set<Security>();

        public DbSet<Trade> Trades => this.Set<Trade>();

        Trade ITradeRepository.AddTrade(Trade trade)
        {
            return this.Trades.Add(trade);
        }

        void ITradeRepository.SaveChanges()
        {
            this.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations
                .Add(Portfolio.EntityTypeConfiguration)
                .Add(Security.EntityTypeConfiguration)
                .Add(Trade.EntityTypeConfiguration);
        }
    }
}