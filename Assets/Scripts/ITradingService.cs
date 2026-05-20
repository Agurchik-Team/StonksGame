/// <summary>
/// Интерфейс для операций торговли
/// </summary>
public interface ITradingService
{
    float Cash { get; }
    int SharesOwned { get; }
    float Equity { get; }
    float CurrentLivingCost { get; }
    
    bool Buy();
    bool Sell();
    
    event System.Action OnPortfolioChanged;
    event System.Action OnBankrupt;
}