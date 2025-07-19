using PortfolioApp.Application.Fees;
using PortfolioApp.Domain;

namespace PortfolioApp.Application;


public sealed record FeeRegistration(
    IFeeCalculator Calculator,
    FeeDirection     Direction);
