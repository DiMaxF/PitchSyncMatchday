using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StadiumModel
{
    public int id;             
    public string name;          
    public string address;        
    public Vector2 location;       
    public float rating;           
    public int reviewsCount;        
    public float basePricePerHour; 
    public string photoUrl;        
    public List<PitchSize> supportedSizes;
}
