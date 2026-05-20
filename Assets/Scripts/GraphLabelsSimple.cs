using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GraphLabelsSimple : MonoBehaviour
{
    [SerializeField] private StockData stock;
    [SerializeField] private RectTransform graphRect;  // Ссылка на RectTransform графика
    [SerializeField] private int maxVisiblePoints = 50;
    
    [Header("Подписи оси Y (цена) - сверху вниз")]
    [SerializeField] private TMP_Text[] yLabels;
    
    [Header("Подписи оси X (время) - слева направо")]
    [SerializeField] private TMP_Text[] xLabels;
    
    private List<float> history => stock?.priceHistory;

    void Update()
    {
        if (history == null || history.Count < 2) return;
        if (graphRect == null) return;

        // Получаем видимые точки
        List<float> visibleHistory = GetVisibleHistory();
        if (visibleHistory.Count < 2) return;

        // Нормализация цен
        float min = Mathf.Min(visibleHistory.ToArray());
        float max = Mathf.Max(visibleHistory.ToArray());
        float range = max - min;
        if (range < 0.001f) range = 1f;
        float padding = range * 0.1f;
        min -= padding;
        max += padding;
        range = max - min;

        // Обновляем Y подписи (цена)
        if (yLabels != null)
        {
            for (int i = 0; i < yLabels.Length; i++)
            {
                if (yLabels[i] == null) continue;
                float normalizedY = (float)i / (yLabels.Length - 1);
                float price = min + normalizedY * range;
                yLabels[i].text = $"{price:F2}";
            }
        }

        // Обновляем X подписи (время)
        if (xLabels != null)
        {
            int totalPoints = history.Count;
            int visibleStart = totalPoints - visibleHistory.Count;
            
            for (int i = 0; i < xLabels.Length; i++)
            {
                if (xLabels[i] == null) continue;
                float normalizedX = (float)i / (xLabels.Length - 1);
                int pointIndex = Mathf.FloorToInt(normalizedX * (visibleHistory.Count - 1));
                pointIndex = Mathf.Clamp(pointIndex, 0, visibleHistory.Count - 1);
                int pointsAgo = totalPoints - 1 - (visibleStart + pointIndex);
                xLabels[i].text = $"t-{pointsAgo}";
            }
        }
    }

    private List<float> GetVisibleHistory()
    {
        if (history.Count <= maxVisiblePoints)
            return new List<float>(history);
        return history.GetRange(history.Count - maxVisiblePoints, maxVisiblePoints);
    }
}