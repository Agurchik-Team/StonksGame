using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Отвечает за генерацию цены акции с нарастающей сложностью
/// Наследует MonoBehaviour, реализует IPriceProvider и IGameService
/// </summary>
public class StockManager : MonoBehaviour, IPriceProvider, IGameService
{
    public static StockManager Instance { get; private set; }

    [SerializeField] private StockData stock;
    [SerializeField] private float basePrice = 100f;
    [SerializeField] private int maxHistory = 200;

    [Header("Волатильность")]
    [SerializeField] private float startVolatility = 0.02f;
    [SerializeField] private float volatilityIncreasePerSec = 0.002f;
    [SerializeField] private float maxVolatility = 0.3f;

    [Header("Тренд")]
    [SerializeField] private float startTrendChangeChance = 0.1f;
    [SerializeField] private float trendChanceIncreasePerSec = 0.005f;
    [SerializeField] private float maxTrendChangeChance = 0.8f;

    [Header("Обновление")]
    [SerializeField] private float updateInterval = 0.5f;

    private float elapsedTime;
    private float currentTrend;
    private float updateTimer;
    private bool isPaused;

    // Реализация интерфейса
    public float CurrentPrice => stock.currentPrice;
    public float ElapsedTime => elapsedTime;
    public float CurrentVolatility { get; private set; }
    public event System.Action OnPriceUpdated;

    public void Initialize()
    {
        stock.currentPrice = basePrice;
        stock.priceHistory.Clear();
        stock.priceHistory.Add(basePrice);
        updateTimer = updateInterval;
    }

    public void Shutdown() => isPaused = true;

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Update()
    {
        if (isPaused) return;

        elapsedTime += Time.deltaTime;
        UpdateDifficulty();

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            TickPrice();
        }
    }

    private void UpdateDifficulty()
    {
        CurrentVolatility = Mathf.Min(startVolatility + volatilityIncreasePerSec * elapsedTime, maxVolatility);
    }

    private void TickPrice()
    {
        float trendChance = Mathf.Min(startTrendChangeChance + trendChanceIncreasePerSec * elapsedTime, maxTrendChangeChance);

        if (Random.value < trendChance * updateInterval)
            currentTrend = Random.Range(-1f, 1f);

        float noise = Random.Range(-CurrentVolatility, CurrentVolatility);
        float trendEffect = currentTrend * CurrentVolatility * 0.5f;
        float newPrice = stock.currentPrice * (1f + noise + trendEffect);
        newPrice = Mathf.Max(newPrice, 0.01f);

        stock.currentPrice = newPrice;
        stock.priceHistory.Add(newPrice);
        if (stock.priceHistory.Count > maxHistory)
            stock.priceHistory.RemoveAt(0);

        OnPriceUpdated?.Invoke();
        GameEventBus.TriggerPriceUpdated(newPrice);
    }
}