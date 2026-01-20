using System;
using UnityEngine;

[Serializable]
public class ExpenseModel
{
    public int id;
    public string name;            
    public decimal amount;           
    public ExtraType type;           
    public DateTime? createdAt;     
}