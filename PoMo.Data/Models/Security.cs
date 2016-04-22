using System.Data.Entity.ModelConfiguration;

namespace PoMo.Data.Models
{
    public sealed class Security : IdentityObjectBase
    {
        internal static readonly EntityTypeConfiguration<Security> EntityTypeConfiguration = new TypeConfiguration();

        public string Description
        {
            get;
            set;
        }

        public string Ticker
        {
            get;
            set;
        }

        public decimal OpeningPrice
        {
            get;
            set;
        }

        private sealed class TypeConfiguration : IdentityTypeConfiguration<Security>
        {
            public TypeConfiguration()
            {
                this.Property(model => model.Description).HasMaxLength(255);
                this.Property(model => model.Ticker).HasMaxLength(255);
                this.Property(model => model.OpeningPrice).HasPrecision(18, 8);
            }
        }
    }
}