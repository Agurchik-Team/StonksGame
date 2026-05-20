using UnityEngine;

/// <summary>
/// Отвечает за финансы игрока и торговые операции
/// Наследует MonoBehaviour, реализует ITradingService и IGameService
/// </summary>
public class PlayerPortfolio : MonoBehaviour, ITradingService, IGameService
{
    public static PlayerPortfolio Instance { get; private set; }

    [SerializeField] private StockData stock;
    [SerializeField] private float startCash = 1000f;

    [Header("Плата за жизнь")]
    [SerializeField] private float startLivingCost = 0.5f;
    [SerializeField] private float livingCostIncreasePerSec = 0.01f;
    [SerializeField] private float maxLivingCost = 5f;

    [Header("Звуки")]
    [SerializeField] private AudioClip buySound;
    [SerializeField] private AudioClip sellSound;

    // Реализация интерфейса
    public float Cash { get; private set; }
    public int SharesOwned { get; private set; }
    public float Equity => Cash + SharesOwned * stock.currentPrice;
    public float CurrentLivingCost { get; private set; }

    public event System.Action OnPortfolioChanged;
    public event System.Action OnBankrupt;

    private bool isBankrupt;
    private float livingCostTimer = 1f;
    private AudioSource audioSource;

    public void Initialize()
    {
        Cash = startCash;
        SharesOwned = 0;
        isBankrupt = false;
        livingCostTimer = 1f;
        CurrentLivingCost = startLivingCost;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Shutdown()
    {
        isBankrupt = true;
    }

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Update()
    {
        if (isBankrupt) return;

        CurrentLivingCost = Mathf.Min(
            startLivingCost + livingCostIncreasePerSec * (StockManager.Instance?.ElapsedTime ?? 0),
            maxLivingCost
        );

        livingCostTimer -= Time.deltaTime;
        if (livingCostTimer <= 0f)
        {
            livingCostTimer = 1f;
            Cash -= CurrentLivingCost;
            CheckBankruptcy();
            OnPortfolioChanged?.Invoke();
            GameEventBus.TriggerPortfolioChanged();
        }
    }

    public bool Buy()
    {
        if (isBankrupt || Cash < stock.currentPrice) return false;

        Cash -= stock.currentPrice;
        SharesOwned++;
        PlaySound(buySound);
        OnPortfolioChanged?.Invoke();
        GameEventBus.TriggerPortfolioChanged();
        return true;
    }

    public bool Sell()
    {
        if (isBankrupt || SharesOwned <= 0) return false;

        Cash += stock.currentPrice;
        SharesOwned--;
        PlaySound(sellSound);
        OnPortfolioChanged?.Invoke();
        GameEventBus.TriggerPortfolioChanged();
        return true;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, 0.3f);
    }

    private void CheckBankruptcy()
    {
        if (Equity <= 0f)
        {
            isBankrupt = true;
            OnBankrupt?.Invoke();
            GameEventBus.TriggerGameOver();
        }
    }
}