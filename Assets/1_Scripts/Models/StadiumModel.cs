using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stadium
{
    public string id;               // Уникальный ID (GUID или простой string)
    public string name;             // Название площадки, например "Arena Sport"
    public string district;         // Район/город
    public string address;          // Полный адрес (опционально)
    public Vector2 location;        // Координаты для расчёта расстояния (lat, lon)
    public float rating;            // Средний рейтинг (0-5)
    public int reviewsCount;        // Количество отзывов
    public float basePricePerHour;  // Базовая цена за час
    public string photoUrl;         // Локальный путь или placeholder (для офлайн)
    public List<PitchSize> supportedSizes; // Какие размеры поддерживает (6x6, 7x7, 11x11)
}
