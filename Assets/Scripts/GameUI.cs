using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Данные")]
    [SerializeField] private StockData stock;
    
    [Header("Показатели")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text sharesText;
    [SerializeField] private TMP_Text equityText;
    [SerializeField] private TMP_Text timerText;
    
    [Header("Цвета")]
    [SerializeField] private Color priceUpColor = Color.green;
    [SerializeField] private Color priceDownColor = Color.red;
    [SerializeField] private Color priceNeutralColor = Color.white;
    [SerializeField] private Color negativeColor = Color.red;
    
    [Header("Кнопки")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private Button restartButton;

    private PlayerPortfolio portfolio;
    private StockManager stockManager;
    private bool isGameOver;
    private float lastPrice;

    private void Awake()
    {
        stockManager = FindAnyObjectByType<StockManager>();
        portfolio = FindAnyObjectByType<PlayerPortfolio>();
        isGameOver = false;
    }

    private void Start()
    {
        if (stock != null) lastPrice = stock.currentPrice;
        UpdateUI();
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Кнопки (используем Buy/Sell вместо BuyOne/SellOne)
        if (buyButton != null) buyButton.onClick.AddListener(() => portfolio?.Buy());
        if (sellButton != null) sellButton.onClick.AddListener(() => portfolio?.Sell());
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        
        // Подписка на события
        if (portfolio != null)
        {
            portfolio.OnPortfolioChanged += UpdateUI;
            portfolio.OnBankrupt += GameOver;
        }
        if (stockManager != null)
        {
            stockManager.OnPriceUpdated += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (buyButton != null) buyButton.onClick.RemoveAllListeners();
        if (sellButton != null) sellButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        
        if (portfolio != null)
        {
            portfolio.OnPortfolioChanged -= UpdateUI;
            portfolio.OnBankrupt -= GameOver;
        }
        if (stockManager != null)
        {
            stockManager.OnPriceUpdated -= UpdateUI;
        }
    }

    private void Update()
    {
        if (!isGameOver && stockManager != null && timerText != null)
        {
            float time = stockManager.ElapsedTime;
            timerText.text = $"ВРЕМЯ: {FormatTime(time)}";
        }
    }

    private void UpdateUI()
    {
        if (stock == null || portfolio == null) return;
        
        // Цена
        if (priceText != null)
        {
            float currentPrice = stock.currentPrice;
            if (currentPrice > lastPrice)
                priceText.color = priceUpColor;
            else if (currentPrice < lastPrice)
                priceText.color = priceDownColor;
            else
                priceText.color = priceNeutralColor;
            
            priceText.text = $"ЦЕНА: ${currentPrice:F2}";
            lastPrice = currentPrice;
        }
        
        // Баланс
        if (cashText != null)
        {
            float cash = portfolio.Cash;
            cashText.text = $"БАЛАНС: ${cash:F2}";
            cashText.color = cash < 0 ? negativeColor : priceNeutralColor;
        }
        
        // Акции
        if (sharesText != null)
            sharesText.text = $"АКЦИИ: {portfolio.SharesOwned}";
        
        // Капитал
        if (equityText != null)
        {
            float equity = portfolio.Equity;
            equityText.text = $"КАПИТАЛ: ${equity:F2}";
            equityText.color = equity <= 0 ? negativeColor : priceNeutralColor;
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        
        // Останавливаем StockManager через Shutdown()
        if (stockManager != null)
            stockManager.Shutdown();
        
        // Останавливаем музыку и включаем музыку поражения
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayGameOverMusic();
        
        // Блокируем кнопки
        if (buyButton != null) buyButton.interactable = false;
        if (sellButton != null) sellButton.interactable = false;
        
        // Показываем панель
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        // Финальное время
        if (finalTimeText != null && stockManager != null)
        {
            float totalTime = stockManager.ElapsedTime;
            finalTimeText.text = $"Вы продержались: {FormatTime(totalTime)}";
        }
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}