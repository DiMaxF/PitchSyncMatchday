using UniRx;
using UnityEngine;
using System.Linq;

public class NavigationDataManager : IDataManager
{
    private readonly AppConfig _config;

    public ReactiveProperty<Screens> SelectedScreen { get; } = new ReactiveProperty<Screens>(Screens.HomeScreen);

    public ReactiveCollection<NavbarButtonModel> Buttons { get; } = new ReactiveCollection<NavbarButtonModel>();

    public ReactiveCollection<object> ButtonsAsObject => Buttons.Select(m => (object)m).ToReactiveCollection();

    public NavigationDataManager(AppConfig config)
    {
        _config = config;
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        foreach (var cfg in _config.navbarData)
        {
            var model = new NavbarButtonModel(cfg, false);
            Buttons.Add(model);
        }

        UpdateButtonsSelection();
        SelectedScreen.Subscribe(_ => UpdateButtonsSelection());
    }

    private void UpdateButtonsSelection()
    {
        foreach (var model in Buttons)
        {
            model.selected = model.screen == SelectedScreen.Value;
        }
    }

    public void SelectScreen(Screens screen)
    {
        SelectedScreen.Value = screen;
    }
}