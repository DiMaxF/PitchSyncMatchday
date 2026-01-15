using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class Navbar : UIView<ReactiveCollection<object>>
{
    [SerializeField] private ScreensManager screensManager;
    [SerializeField] private ListContainer listView;

    private NavigationDataManager Navigation => DataManager.Navigation;

    protected override void OnEnable()
    {
        base.OnEnable();
        var collection = Navigation.ButtonsAsObject;
        Init(collection);
        if (listView != null)
        {
            listView.Init(collection); 
        }
        new Log($"{DataProperty.Value.Count}", "Navbar");
    }

    protected override void Subscribe()
    {
        UIManager.SubscribeToView(listView, (NavbarButtonModel data) =>
        {
            new Log($"{data.screen}", "Navbar");
            screensManager.Show(data.screen);
            //DataManager.Navigation.SelectScreen(data.screen);
        }, persistent: true);
    }
}