using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioApp.Application
{
    public record BuyRequestDto(Guid assetId, int qty, decimal price, DateTime date);
    public record SellRequestDto(Guid assetId, int qty, decimal price, CostBasisMethod method, DateTime date);
}
