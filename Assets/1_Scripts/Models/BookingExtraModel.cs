using UnityEngine;

public class BookingExtraModel
{
    public ExtraType type;
    public string name;
    public int maxQuantity;
    public float pricePerUnit;
    public Sprite icon;
    public int currentQuantity;

    public BookingExtraModel(BookingExtraConfig.ExtraConfigData config, int currentQuantity = 0)
    {
        type = config.type;
        name = config.type.ToString();
        maxQuantity = config.maxQuantity;
        pricePerUnit = config.pricePerUnit;
        icon = config.icon;
        this.currentQuantity = currentQuantity;
    }

    public float GetTotalPrice() => currentQuantity * pricePerUnit;
}


