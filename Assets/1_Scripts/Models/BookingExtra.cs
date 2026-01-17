using System;
using UnityEngine;

[Serializable]
public class BookingExtra
{
    public ExtraType type;
    public int quantity;         
    public float pricePerUnit;   

    public BookingExtra(ExtraType type, int quantity = 0, float pricePerUnit = 0f)
    {
        this.type = type;
        this.quantity = quantity;
        this.pricePerUnit = pricePerUnit;
    }

    public float GetTotalPrice() => quantity * pricePerUnit;
}