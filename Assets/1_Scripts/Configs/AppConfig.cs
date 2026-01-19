using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AppConfig", menuName = "Scriptable Objects/AppConfig")]
public class AppConfig : ScriptableObject
{
    public List<NavigationButtonConfig> navbarData;
    public List<PitchConfig> pitches;
    public List<TeamConfig> teams;
    public BookingExtraConfig extrasConfig;
}
