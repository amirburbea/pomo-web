using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PoMo.Common.DataObjects;
using PoMo.Data;

namespace PoMo.Server.Web.WebApi
{
    public sealed class PortfoliosController : ApiController
    {
        private readonly IDataContext _dataContext;

        public PortfoliosController(IDataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        public IEnumerable<PortfolioModel> GetPortfolios()
        {
            return this._dataContext.Portfolios
                .AsEnumerable()
                .Select(portfolio => new PortfolioModel { Id = portfolio.Id, Name = portfolio.Name });
        }
    }
}