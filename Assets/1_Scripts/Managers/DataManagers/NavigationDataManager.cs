using UniRx;
using UnityEngine;

public class NavigationDataManager : IDataManager
{
    private readonly AppConfig _config;

    public ReactiveProperty<Screens> SelectedScreen { get; } = new ReactiveProperty<Screens>(Screens.HomeScreen);

    public ReactiveCollection<NavbarButtonModel> Buttons { get; } = new ReactiveCollection<NavbarButtonModel>();

    private readonly ReactiveCollection<object> _buttonsAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> ButtonsAsObject => _buttonsAsObject;

    public NavigationDataManager(AppConfig config)
    {
        _config = config;
        InitializeButtons();
        BindButtons();
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

    private void BindButtons()
    {
        _buttonsAsObject.Clear();
        foreach (var button in Buttons)
        {
            _buttonsAsObject.Add(button);
        }

        Buttons.ObserveAdd().Subscribe(e => _buttonsAsObject.Insert(e.Index, e.Value));
        Buttons.ObserveRemove().Subscribe(e => _buttonsAsObject.RemoveAt(e.Index));
        Buttons.ObserveReplace().Subscribe(e => _buttonsAsObject[e.Index] = e.NewValue);
        Buttons.ObserveMove().Subscribe(e => _buttonsAsObject.Move(e.OldIndex, e.NewIndex));
        Buttons.ObserveReset().Subscribe(_ =>
        {
            _buttonsAsObject.Clear();
            foreach (var button in Buttons) _buttonsAsObject.Add(button);
        });
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