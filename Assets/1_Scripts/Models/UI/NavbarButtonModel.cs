using UnityEngine;

public class NavbarButtonModel
{
    public string label;
    public Sprite icon;
    public Screens screen;
    public bool selected;

    public NavbarButtonModel(NavigationButtonConfig config, bool isSelected)
    {
        label = config.buttonName;
        icon = config.icon;
        screen = config.screen;
        selected = isSelected;
    }

    public NavbarButtonModel(string label, Sprite icon, Screens screen, bool selected)
    {
        this.label = label;
        this.icon = icon;
        this.screen = screen;
        this.selected = selected;
    }
}
