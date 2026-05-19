using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStock", menuName = "Stock Data")]
public class StockData : ScriptableObject
{
    public string stockName = "AAPL";
    [HideInInspector] public float currentPrice = 100f;
    [HideInInspector] public List<float> priceHistory = new();
}