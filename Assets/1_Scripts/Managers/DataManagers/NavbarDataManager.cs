using UniRx;
using UnityEngine;
using System.Linq;

public class NavbarDataManager : IDataManager
{
    AppConfig _config;

    public ReactiveProperty<NavbarButtonModel> selectedScreen = new ReactiveProperty<NavbarButtonModel>(null);

    public NavbarDataManager(AppConfig config) 
    {
        _config = config;
    }

    public void SelectScreen(NavbarScreens screen) 
    {
        selectedScreen = new ReactiveProperty<NavbarButtonModel>(
            GetNavbarData().FirstOrDefault(navbar => navbar.screen == screen)
        );
    }

    public NavbarButtonModel[] GetNavbarData()
    {
        var navbarConfigs = _config.navbarData;
        NavbarButtonModel[] navbarData = new NavbarButtonModel[navbarConfigs.Count];
        for (int i = 0; i < navbarConfigs.Count; i++)
        {
            bool isSelected = (i == 0);
            navbarData[i] = new NavbarButtonModel(navbarConfigs[i], isSelected);
        }
        return navbarData;
    }   
}
