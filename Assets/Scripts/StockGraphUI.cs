using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StockGraphUI : Graphic
{
    [SerializeField] private StockData stock;
    [SerializeField] private Color lineColor = Color.green;
    [SerializeField] private float lineThickness = 3f;
    
    [Header("Отступы")]
    [SerializeField] private float marginLeft = 50f;      // Место для подписей оси Y
    [SerializeField] private float marginBottom = 30f;    // Место для подписей оси X
    [SerializeField] private float marginRight = 15f;
    [SerializeField] private float marginTop = 15f;
    
    [Header("Настройки отображения")]
    [SerializeField] private int maxVisiblePoints = 50;   // Сколько последних точек показывать

    private List<float> history => stock?.priceHistory;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (history == null || history.Count < 2) return;

        // Получаем последние N точек
        List<float> visibleHistory = GetVisibleHistory();
        if (visibleHistory.Count < 2) return;

        Rect rect = GetPixelAdjustedRect();
        float width = rect.width;
        float height = rect.height;
        
        float graphWidth = width - marginLeft - marginRight;
        float graphHeight = height - marginBottom - marginTop;
        
        if (graphWidth <= 0 || graphHeight <= 0) return;

        // Нормализация цен
        float min = Mathf.Min(visibleHistory.ToArray());
        float max = Mathf.Max(visibleHistory.ToArray());
        float range = max - min;
        if (range < 0.001f) range = 1f;
        
        float padding = range * 0.1f;
        min -= padding;
        max += padding;
        range = max - min;

        int pointCount = visibleHistory.Count;
        float xStep = graphWidth / (pointCount - 1);

        // 1. Рисуем сетку
        DrawGrid(vh, graphWidth, graphHeight, pointCount);
        
        // 2. Рисуем оси
        DrawAxes(vh, graphWidth, graphHeight);
        
        // 3. Рисуем линию графика
        DrawGraphLine(vh, visibleHistory, min, range, graphWidth, graphHeight, xStep, pointCount);
    }

    // ===== ОСИ =====
    private void DrawAxes(VertexHelper vh, float graphWidth, float graphHeight)
    {
        float left = marginLeft;
        float bottom = marginBottom;
        float right = marginLeft + graphWidth;
        float top = marginBottom + graphHeight;
        
        // Ось X (горизонтальная)
        DrawLine(vh, new Vector2(left, bottom), new Vector2(right, bottom), Color.white, 2f);
        
        // Ось Y (вертикальная)
        DrawLine(vh, new Vector2(left, bottom), new Vector2(left, top), Color.white, 2f);
    }

    // ===== СЕТКА =====
    private void DrawGrid(VertexHelper vh, float graphWidth, float graphHeight, int pointCount)
    {
        float left = marginLeft;
        float bottom = marginBottom;
        float right = marginLeft + graphWidth;
        float top = marginBottom + graphHeight;
        
        // Горизонтальные линии (5 линий = 4 промежутка)
        int hLines = 4;
        for (int i = 1; i <= hLines; i++)
        {
            float y = bottom + (graphHeight / hLines) * i;
            DrawLine(vh, new Vector2(left, y), new Vector2(right, y), 
                     new Color(1f, 1f, 1f, 0.15f), 1f);
        }
        
        // Вертикальные линии (5 линий)
        int vLines = 4;
        for (int i = 1; i <= vLines; i++)
        {
            float x = left + (graphWidth / vLines) * i;
            DrawLine(vh, new Vector2(x, bottom), new Vector2(x, top), 
                     new Color(1f, 1f, 1f, 0.1f), 1f);
        }
    }

    // ===== ЛИНИЯ ГРАФИКА =====
    private void DrawGraphLine(VertexHelper vh, List<float> data, float min, float range,
                               float graphWidth, float graphHeight, float xStep, int pointCount)
    {
        for (int i = 0; i < pointCount - 1; i++)
        {
            float x1 = marginLeft + i * xStep;
            float y1 = marginBottom + ((data[i] - min) / range) * graphHeight;
            
            float x2 = marginLeft + (i + 1) * xStep;
            float y2 = marginBottom + ((data[i + 1] - min) / range) * graphHeight;
            
            // Цвет: зелёный при росте, красный при падении
            Color color = lineColor;
            if (data[i + 1] > data[i])
                color = Color.green;
            else if (data[i + 1] < data[i])
                color = Color.red;
            
            DrawLine(vh, new Vector2(x1, y1), new Vector2(x2, y2), color, lineThickness);
        }
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====
    private List<float> GetVisibleHistory()
    {
        if (history.Count <= maxVisiblePoints)
            return new List<float>(history);
        
        return history.GetRange(history.Count - maxVisiblePoints, maxVisiblePoints);
    }

    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * thickness * 0.5f;
        
        UIVertex v1 = UIVertex.simpleVert;
        v1.position = start + perpendicular;
        v1.color = color;
        
        UIVertex v2 = UIVertex.simpleVert;
        v2.position = start - perpendicular;
        v2.color = color;
        
        UIVertex v3 = UIVertex.simpleVert;
        v3.position = end + perpendicular;
        v3.color = color;
        
        UIVertex v4 = UIVertex.simpleVert;
        v4.position = end - perpendicular;
        v4.color = color;
        
        int idx = vh.currentVertCount;
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddVert(v4);
        
        vh.AddTriangle(idx, idx + 1, idx + 2);
        vh.AddTriangle(idx + 2, idx + 1, idx + 3);
    }

    private void Update()
    {
        SetVerticesDirty();
    }
}