using System;

/// <summary>
/// Шина событий для уменьшения связанности между модулями
/// </summary>
public static class GameEventBus
{
    public static event Action OnGameOver;
    public static event Action OnGameRestart;
    public static event Action<float> OnPriceUpdated;
    public static event Action OnPortfolioChanged;

    public static void TriggerGameOver() => OnGameOver?.Invoke();
    public static void TriggerGameRestart() => OnGameRestart?.Invoke();
    public static void TriggerPriceUpdated(float price) => OnPriceUpdated?.Invoke(price);
    public static void TriggerPortfolioChanged() => OnPortfolioChanged?.Invoke();
    
    /// <summary>
    /// Очищает все подписки (при перезагрузке)
    /// </summary>
    public static void ClearAll()
    {
        OnGameOver = null;
        OnGameRestart = null;
        OnPriceUpdated = null;
        OnPortfolioChanged = null;
    }
}