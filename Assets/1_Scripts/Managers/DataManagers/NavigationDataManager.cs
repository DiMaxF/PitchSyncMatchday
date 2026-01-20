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
        for (int i = 0; i < Buttons.Count; i++)
        {
            var model = Buttons[i];
            var newSelected = model.screen == SelectedScreen.Value;
            if (model.selected != newSelected)
            {
                Buttons[i] = new NavbarButtonModel(model.label, model.icon, model.screen, newSelected);
            }
        }
    }

    public void SelectScreen(Screens screen)
    {
        SelectedScreen.Value = screen;
    }
}