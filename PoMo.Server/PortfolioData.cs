using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PoMo.Common;
using PoMo.Data;
using PoMo.Data.Models;

namespace PoMo.Server
{
    public interface IPortfolioData
    {
    }

    public sealed class Position : NotifierBase
    {
        private readonly Security _security;
        private decimal _costBasis;
        private int _quantity;

        public Position(Security security)
        {
            this._security = security;
        }

        [Display(Name = "Cost Basis", Order = 3)]
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

        [Display(Order = 1)]
        public string Description => this._security.Description;

        [Display(Order = 2)]
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

        [Display(Order = 0)]
        public string Ticker => this._security.Ticker;
    }

    internal sealed class PortfolioData : IPortfolioData
    {
        private readonly PortfolioDatum[] _data;

        public PortfolioData(IDataContext dataContext)
        {
            this._data = dataContext.Portfolios.AsEnumerable().Select(portfolio => new PortfolioDatum(portfolio, dataContext)).ToArray();
        }
    }

    internal sealed class PortfolioDatum
    {
        private static readonly DataTable _dataSchemaTable = PortfolioDatum.CreateDataSchemaTable();
        private static readonly Func<Position, object[]> _rowProjection = PortfolioDatum.CreateRowProjection();

        private readonly DataTable _dataTable;
        private readonly Portfolio _portfolio;

        public PortfolioDatum(Portfolio portfolio, IDataContext dataContext)
        {
            this._portfolio = portfolio;
            this._dataTable = PortfolioDatum._dataSchemaTable.Clone();
        }

        public string PortfolioId => this._portfolio.Id;

        private static DataTable CreateDataSchemaTable()
        {
            DataTable dataTable = new DataTable();
            foreach (DataColumn dataColumn in
                from property in typeof(Position).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                let displayAttribute = property.GetCustomAttribute<DisplayAttribute>()
                let caption = displayAttribute?.GetName() ?? property.Name
                orderby displayAttribute?.GetOrder() ?? int.MaxValue, caption
                select new DataColumn(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType)
                {
                    Caption = caption,
                    ExtendedProperties =
                    {
                        { nameof(DisplayFormatAttribute.DataFormatString), property.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString }
                    }
                })
            {
                dataTable.Columns.Add(dataColumn);
            }
            return dataTable;
        }

        private static Func<Position, object[]> CreateRowProjection()
        {
            ParameterExpression position = Expression.Parameter(typeof(Position));
            Expression<Func<Position, object[]>> lambda = Expression.Lambda<Func<Position, object[]>>(
                Expression.NewArrayInit(
                    typeof(object),
                    PortfolioDatum._dataSchemaTable.Columns.Cast<DataColumn>().Select(
                        column => Expression.Coalesce(
                            Expression.Convert(
                                Expression.Property(
                                    position,
                                    typeof(Position).GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance)
                                ),
                                typeof(object)
                            ),
                            Expression.Field(
                                null,
                                typeof(Convert),
                                nameof(Convert.DBNull)
                            )
                        )
                    )
                ),
                position
            );
            return lambda.Compile();
        }
    }
}