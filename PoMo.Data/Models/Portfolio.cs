using System.Data.Entity.ModelConfiguration;

namespace PoMo.Data.Models
{
    public sealed class Portfolio
    {
        internal static readonly EntityTypeConfiguration<Portfolio> EntityTypeConfiguration = new TypeConfiguration();

        public string Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        private sealed class TypeConfiguration : EntityTypeConfiguration<Portfolio>
        {
            public TypeConfiguration()
            {
                this.ToTable(nameof(Portfolio));
                this.HasKey(model => model.Id);
                this.Property(model => model.Id).HasMaxLength(255);
                this.Property(model => model.Name).HasMaxLength(255);
            }
        }
    }
}