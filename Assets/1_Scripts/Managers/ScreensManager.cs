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
    private bool _isTransitioning = false;
    private Screens? _pendingScreen;

    private void Start()
    {
        foreach (var screen in screens)
        {
            screen.gameObject.SetActive(false);
            screen.Init(this);
        }

        Navigation.SelectedScreen.Subscribe(ShowScreen).AddTo(this);
        ShowScreen(Navigation.SelectedScreen.Value);

        var loadingScreen = screens.FirstOrDefault(s => s.ScreenType == Screens.LoadingScreen);
        if (loadingScreen != null)
        {
            ShowScreen(Screens.LoadingScreen);
        }
        else
        {
            ShowScreen(Navigation.SelectedScreen.Value);
        }
    }

    public void Show(Screens screen)
    {
        NavigateTo(screen, true);
    }

    public void Back()
    {
        if (_currentScreen == null) return;
        while (_screenHistory.Count > 0)
        {
            var previousScreen = _screenHistory.Pop();
            if (previousScreen == _currentScreen.ScreenType) continue;
            if (screens.Any(s => s.ScreenType == previousScreen))
            {
                NavigateTo(previousScreen, false);
                return;
            }
        }
    }

    public bool CanGoBack()
    {
        return _screenHistory.Count > 0;
    }

    public void ClearHistory()
    {
        _screenHistory.Clear();
    }

    private void NavigateTo(Screens screen, bool addToHistory)
    {
        if (_currentScreen != null && _currentScreen.ScreenType == screen) return;
        if (addToHistory && _currentScreen != null)
        {
            _screenHistory.Push(_currentScreen.ScreenType);
        }
        Navigation.SelectScreen(screen);
    }

    private async void ShowScreen(Screens screen)
    {
        if (_isTransitioning)
        {
            _pendingScreen = screen;
            return;
        }

        var targetScreen = screens.FirstOrDefault(s => s.ScreenType == screen);

        if (targetScreen == null) return;

        _isTransitioning = true;

        if (_currentScreen != null && _currentScreen != targetScreen)
        {
            await _currentScreen.HideAsync();
        }

        await targetScreen.ShowAsync();
        _currentScreen = targetScreen;

        _isTransitioning = false;
        if (_pendingScreen.HasValue && _pendingScreen.Value != _currentScreen.ScreenType)
        {
            var next = _pendingScreen.Value;
            _pendingScreen = null;
            ShowScreen(next);
        }
        else
        {
            _pendingScreen = null;
        }
    }

}