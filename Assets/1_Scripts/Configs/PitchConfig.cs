using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PitchConfig", menuName = "Scriptable Objects/PitchConfig")]
public class PitchConfig : ScriptableObject
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




