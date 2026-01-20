using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public class Navbar : UIView<ReactiveCollection<object>>
{
    [SerializeField] private ScreensManager screensManager;
    [SerializeField] private ListContainer listView;
    [SerializeField] private CanvasGroup canvasGroup;

    private NavigationDataManager Navigation => DataManager.Navigation;
    private Screens _previousScreen;

    protected override void OnEnable()
    {
        base.OnEnable();
        var collection = Navigation.ButtonsAsObject;
        Init(collection);
        if (listView != null)
        {
            listView.Init(collection); 
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        _previousScreen = Navigation.SelectedScreen.Value;
    }

    protected override void Subscribe()
    {
        UIManager.SubscribeToView(listView, (NavbarButtonModel data) =>
        {
            screensManager.Show(data.screen);
        }, persistent: true);

        Navigation.SelectedScreen
            .Where(screen => screen != _previousScreen)
            .Subscribe(screen =>
            {
                _previousScreen = screen;
                BlockNavbarTemporarily().Forget();
            })
            .AddTo(this);
    }

    private async UniTaskVoid BlockNavbarTemporarily()
    {
        if (canvasGroup == null) return;

        canvasGroup.interactable = false;
        await UniTask.Delay(700);
        canvasGroup.interactable = true;
    }
}