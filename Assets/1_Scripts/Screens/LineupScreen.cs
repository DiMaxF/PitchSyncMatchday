using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LineupScreen : UIScreen
{
    [SerializeField] private ListContainer draftModes;
    [SerializeField] private SquadPanel squadBluePanel;
    [SerializeField] private SquadPanel squadRedPanel;
    [SerializeField] private Button playersButton;
    [SerializeField] private Button autoBalanceButton;
    [SerializeField] private Button saveButton;

    private LineupDataManager Lineup => DataManager.Lineup;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (draftModes != null)
        {
            draftModes.Init(Lineup.DraftModesAsObject);

            AddToDispose(UIManager.SubscribeToView(draftModes, (ToggleButtonModel data) =>
            {
                string modeName = data.name.Split(' ')[0];
                if (Enum.TryParse<LineupMode>(modeName, out var mode))
                {
                    Lineup.SelectDraftMode(mode);
                }
            }));
        }


        if (squadBluePanel != null)
        {
            AddToDispose(Lineup.SquadGreen.ObserveAdd().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadGreen.ObserveRemove().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadGreen.ObserveReplace().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadGreen.ObserveReset().Subscribe(_ => UpdateSquadPanels()));
        }

        if (squadRedPanel != null)
        {
            AddToDispose(Lineup.SquadRed.ObserveAdd().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadRed.ObserveRemove().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadRed.ObserveReplace().Subscribe(_ => UpdateSquadPanels()));
            AddToDispose(Lineup.SquadRed.ObserveReset().Subscribe(_ => UpdateSquadPanels()));
        }

        if (squadBluePanel != null && squadBluePanel.playersList != null)
        {
            AddToDispose(UIManager.SubscribeToView(squadBluePanel.playersList, (int playerId) =>
            {
                Lineup.RemovePlayerFromTeam(playerId, TeamSide.Green);
            }));

            AddToDispose(UIManager.SubscribeToView(squadBluePanel.playersList, (SquadPlayerModel player) =>
            {
                if (player != null)
                {
                    Lineup.ToggleCaptain(player.playerId, TeamSide.Green);
                }
            }));
        }

        if (squadRedPanel != null && squadRedPanel.playersList != null)
        {
            AddToDispose(UIManager.SubscribeToView(squadRedPanel.playersList, (int playerId) =>
            {
                Lineup.RemovePlayerFromTeam(playerId, TeamSide.Red);
            }));

            AddToDispose(UIManager.SubscribeToView(squadRedPanel.playersList, (SquadPlayerModel player) =>
            {
                if (player != null)
                {
                    Lineup.ToggleCaptain(player.playerId, TeamSide.Red);
                }
            }));
        }

        if (playersButton != null)
        {
            AddToDispose(playersButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.PlayersManagementScreen);
                }));
        }

        if (autoBalanceButton != null)
        {
            AddToDispose(autoBalanceButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Lineup.AutoBalance();
                }));
        }

        if (saveButton != null)
        {
            AddToDispose(saveButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Lineup.SaveLineup();
                    //ScreenManager?.Show(Screens.LineupViewScreen);
                }));
        }
    }

    private void UpdateSquadPanels()
    {
        if (squadBluePanel != null)
        {
            var panelModel = Lineup.BuildSquadPanelModel(TeamSide.Green);
            squadBluePanel.Init(panelModel);
            
            if (squadBluePanel.playersList != null)
            {
                squadBluePanel.playersList.Init(Lineup.SquadBlueAsObject);
            }
        }

        if (squadRedPanel != null)
        {
            var panelModel = Lineup.BuildSquadPanelModel(TeamSide.Red);
            squadRedPanel.Init(panelModel);
            
            if (squadRedPanel.playersList != null)
            {
                squadRedPanel.playersList.Init(Lineup.SquadRedAsObject);
            }
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();

        if (squadBluePanel != null)
        {
            var panelModel = Lineup.BuildSquadPanelModel(TeamSide.Green);
            squadBluePanel.Init(panelModel);
            
            if (squadBluePanel.playersList != null)
            {
                squadBluePanel.playersList.Init(Lineup.SquadBlueAsObject);
            }
        }

        if (squadRedPanel != null)
        {
            var panelModel = Lineup.BuildSquadPanelModel(TeamSide.Red);
            squadRedPanel.Init(panelModel);
            
            if (squadRedPanel.playersList != null)
            {
                squadRedPanel.playersList.Init(Lineup.SquadRedAsObject);
            }
        }
    }
}
