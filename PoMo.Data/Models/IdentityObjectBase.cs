using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace PoMo.Data.Models
{
    public abstract class IdentityObjectBase
    {
        public int Id
        {
            get;
            set;
        }

        protected abstract class IdentityTypeConfiguration<T> : EntityTypeConfiguration<T>
            where T : IdentityObjectBase
        {
            protected IdentityTypeConfiguration()
            {
                this.ToTable(typeof(T).Name);
                this.HasKey(model => model.Id);
                this.Property(model => model.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            }
        }
    }
}