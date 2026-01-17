using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

public class ScreensManager : MonoBehaviour
{
    [SerializeField] private List<UIScreen> screens = new List<UIScreen>();

    private NavigationDataManager Navigation => DataManager.Navigation;

    private UIScreen _currentScreen;

    private void Start()
    {
        foreach (var screen in screens)
        {
            screen.gameObject.SetActive(false);
            screen.Init(this);
        }

        Navigation.SelectedScreen.Subscribe(ShowScreen).AddTo(this);
        ShowScreen(Navigation.SelectedScreen.Value);
    }

    public void Show(Screens screen)
    {
        Navigation.SelectScreen(screen);
    }

    private async void ShowScreen(Screens screen)
    {
        var targetScreen = screens.FirstOrDefault(s => s.ScreenType == screen);

        if (targetScreen == null) return;

        if (_currentScreen != null && _currentScreen != targetScreen)
        {
            await _currentScreen.HideAsync();
        }

        await targetScreen.ShowAsync();
        _currentScreen = targetScreen;
    }

}