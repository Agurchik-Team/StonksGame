/// <summary>
/// Интерфейс для получения информации о цене акции
/// </summary>
public interface IPriceProvider
{
    float CurrentPrice { get; }
    float ElapsedTime { get; }
    float CurrentVolatility { get; }
    event System.Action OnPriceUpdated;
}