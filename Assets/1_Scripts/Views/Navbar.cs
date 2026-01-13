using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class Navbar : UIView<ReactiveCollection<object>>
{
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
        UIManager.SubscribeToView(this, (NavbarButtonModel data) =>
        {
            DataManager.Navigation.SelectScreen(data.screen);
        }, persistent: true);
    }
}