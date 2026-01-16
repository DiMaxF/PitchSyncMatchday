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
    }

    protected override void Subscribe()
    {
        UIManager.SubscribeToView(listView, (NavbarButtonModel data) =>
        {
            screensManager.Show(data.screen);
        }, persistent: true);
    }
}