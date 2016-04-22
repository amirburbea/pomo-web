using System;
using System.Data.Entity.ModelConfiguration;

namespace PoMo.Data.Models
{
    public sealed class Trade : IdentityObjectBase
    {
        internal static readonly EntityTypeConfiguration<Trade> EntityTypeConfiguration = new TypeConfiguration();

        public string PortfolioId
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }

        public int Quantity
        {
            get;
            set;
        }

        public Security Security
        {
            get;
            set;
        }

        public int SecurityId
        {
            get;
            set;
        }

        public DateTime TradeDate
        {
            get;
            set;
        }

        private sealed class TypeConfiguration : IdentityTypeConfiguration<Trade>
        {
            public TypeConfiguration()
            {
                this.Property(model => model.PortfolioId).HasMaxLength(255);
                this.HasRequired(model => model.Security).WithMany().HasForeignKey(trade => trade.SecurityId);
            }
        }
    }
}