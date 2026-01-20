using System;
using System.Runtime.Serialization;
using UnityEngine;

public class BookingExtraModel
{
    public ExtraType type;
    public string name;
    public int maxQuantity;
    public float pricePerUnit;
    public string iconPath;
    public int currentQuantity;

    [NonSerialized]
    private Sprite _iconCache;

    public Sprite icon
    {
        get
        {
            if (_iconCache == null && !string.IsNullOrEmpty(iconPath))
            {
                _iconCache = FileUtils.LoadImageAsSprite(iconPath);
            }
            return _iconCache;
        }
        set
        {
            _iconCache = value;
        }
    }

    public BookingExtraModel(BookingExtraConfig.ExtraConfigData config, int currentQuantity = 0)
    {
        type = config.type;
        name = config.type.ToString();
        maxQuantity = config.maxQuantity;
        pricePerUnit = config.pricePerUnit;
        this.currentQuantity = currentQuantity;
        
        if (config.icon != null)
        {
            string fileName = $"extra_{type}_icon.png";
            if (!FileUtils.FileExists(fileName))
            {
                FileUtils.SaveImage(config.icon, fileName);
            }
            iconPath = fileName;
            _iconCache = config.icon;
        }
    }
}



