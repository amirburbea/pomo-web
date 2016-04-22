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

    public sealed class DataContext : DbContext, IDataContext
    {
        public DataContext()
            : base(ConnectionStringMethods.GetConnectionString())
        {
            Database.SetInitializer<DataContext>(null);
            this.Configuration.LazyLoadingEnabled = this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Portfolio> Portfolios => this.Set<Portfolio>();

        public DbSet<Security> Securities => this.Set<Security>();

        public DbSet<Trade> Trades => this.Set<Trade>();

        IQueryable<Security> IDataContext.Securities => this.Securities;

        IQueryable<Trade> IDataContext.Trades => this.Trades;

        IQueryable<Portfolio> IDataContext.Portfolios => this.Portfolios;

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations
                .Add(Portfolio.EntityTypeConfiguration)
                .Add(Security.EntityTypeConfiguration)
                .Add(Trade.EntityTypeConfiguration);
        }
    }
}