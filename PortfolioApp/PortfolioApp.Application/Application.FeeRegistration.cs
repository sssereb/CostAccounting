using PortfolioApp.Domain;

namespace PortfolioApp.Application;


public sealed record FeeRegistration(
    FeeType       Type,     // selects the fee calculator
    decimal       Amount,   // fixed $, $ per share or percent rate
    FeeDirection  Direction);