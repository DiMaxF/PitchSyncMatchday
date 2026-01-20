using System;
using UnityEngine;

[Serializable]
public class ParticipantModel
{
    public int id;                    
    public int? playerId;            
    public string name;            
    public float paidAmount;   
    public float owedAmount;     
}