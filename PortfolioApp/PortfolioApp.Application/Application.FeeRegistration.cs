using PortfolioApp.Application.Fees;
using PortfolioApp.Domain;

namespace PortfolioApp.Application;


public sealed record FeeRegistration(
    FeeType       Type,     // ← «какой» калькулятор создавать
    decimal       Amount,   // ← параметр (фикс $, $/шт, %)
    FeeDirection  Direction);