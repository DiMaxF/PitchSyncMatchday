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
    private readonly Stack<Screens> _screenHistory = new Stack<Screens>();
    private bool _isNavigatingBack = false;

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
        if (!_isNavigatingBack && _currentScreen != null && _currentScreen.ScreenType != screen)
        {
            _screenHistory.Push(_currentScreen.ScreenType);
        }
        _isNavigatingBack = false;
        Navigation.SelectScreen(screen);
    }

    public void Back()
    {
        if (_screenHistory.Count == 0) return;

        _isNavigatingBack = true;
        var previousScreen = _screenHistory.Pop();
        Navigation.SelectScreen(previousScreen);
    }

    public bool CanGoBack()
    {
        return _screenHistory.Count > 0;
    }

    public void ClearHistory()
    {
        _screenHistory.Clear();
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