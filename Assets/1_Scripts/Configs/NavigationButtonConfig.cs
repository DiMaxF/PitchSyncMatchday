using UnityEngine;

[CreateAssetMenu(fileName = "NavigationButtonConfig", menuName = "Scriptable Objects/NavigationButtonConfig")]
public class NavigationButtonConfig : ScriptableObject
{
    public string buttonName;   
    public Sprite icon;
    public Screens screen;
}
