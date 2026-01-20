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
    [SerializeField] private PlayerSearchPanel addPlayer;

    private LineupDataManager Lineup => DataManager.Lineup;


    protected override void OnEnable()
    {
        base.OnEnable();
        addPlayer.gameObject.SetActive(false);
    }
    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (draftModes != null)
        {
            draftModes.Init(Lineup.DraftModesAsObject);

            AddToDispose(UIManager.SubscribeToView(draftModes, (ToggleButtonModel data) =>
            {
                string modeName = data.name.Split(' ')[0];
                if (Enum.TryParse<LineupMod>(modeName, out var mode))
                {
                    Lineup.SelectDraftMode(mode);
                }
            }));
        }


        if (squadBluePanel != null)
        {
            AddToDispose(Lineup.SquadGreen.ObserveAdd().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));
            AddToDispose(Lineup.SquadGreen.ObserveRemove().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));
            AddToDispose(Lineup.SquadGreen.ObserveReplace().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));
            AddToDispose(Lineup.SquadGreen.ObserveReset().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));

            AddToDispose(UIManager.SubscribeToView(squadBluePanel, (SquadPanelModel data) =>
            {
                if (addPlayer != null && data != null)
                {
                    addPlayer.InitForTeam(data.teamSide);
                }
            }));
        }

        if (squadRedPanel != null)
        {
            AddToDispose(Lineup.SquadRed.ObserveAdd().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));
            AddToDispose(Lineup.SquadRed.ObserveRemove().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));
            AddToDispose(Lineup.SquadRed.ObserveReplace().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));
            AddToDispose(Lineup.SquadRed.ObserveReset().Subscribe(_ => { UpdateSquadPanels(); UpdateSaveButtonState(); }));

            AddToDispose(UIManager.SubscribeToView(squadRedPanel, (SquadPanelModel data) =>
            {
                if (addPlayer != null && data != null)
                {
                    addPlayer.InitForTeam(data.teamSide);
                }
            }));
        }

        if (squadBluePanel != null && squadBluePanel.playersList != null)
        {
            squadBluePanel.playersList.Init(Lineup.SquadBlueAsObject);
        }

        if (squadRedPanel != null && squadRedPanel.playersList != null)
        {
            squadRedPanel.playersList.Init(Lineup.SquadRedAsObject);
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
                    var savedLineup = Lineup.CurrentLineup.Value;
                    if (savedLineup != null)
                    {
                        if (DataManager.MatchCenter.HasPendingLineupRequest())
                        {
                            DataManager.MatchCenter.AttachLineupToCurrentMatch(savedLineup);
                        }
                        else
                        {
                            DataManager.MatchCenter.InitializeFromLineup(savedLineup.id);
                        }
                    }
                    ScreenManager?.Show(Screens.MatchCenterScreen);
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

        UpdateSaveButtonState();
    }

    private void UpdateSaveButtonState()
    {
        if (saveButton != null)
        {
            bool hasGreenPlayers = Lineup.SquadGreen.Count > 0;
            bool hasRedPlayers = Lineup.SquadRed.Count > 0;
            saveButton.interactable = hasGreenPlayers && hasRedPlayers;
        }
    }
}
