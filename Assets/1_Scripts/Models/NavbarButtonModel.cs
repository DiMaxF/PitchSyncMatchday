using UnityEngine;

public class NavbarButtonModel : MonoBehaviour
{
    public string label;
    public Sprite icon;
    public NavbarScreens screen;
    public bool selected;

    public NavbarButtonModel(NavigationButtonConfig config, bool isSelected)
    {
        label = config.buttonName;
        icon = config.icon;
        screen = config.screen;
        selected = isSelected;
    }
}
