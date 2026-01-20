using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MatchCenterScreen : UIScreen
{
    [SerializeField] private Text statusMatchText;
    [SerializeField] private Text subtitle;
    [SerializeField] private ListContainer screenTabs;

    [SerializeField] private UIView tabLineup;
    [SerializeField] private UIView tabScoreboard;
    [SerializeField] private UIView tabNotes;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (screenTabs != null)
        {
            screenTabs.Init(MatchCenter.TabsAsObject);

            AddToDispose(UIManager.SubscribeToView(screenTabs, (ToggleButtonModel data) =>
            {
                if (Enum.TryParse<MatchCenterTabs>(data.name, out var tab))
                {
                    MatchCenter.SelectTab(tab);
                }
            }));
        }

        AddToDispose(MatchCenter.SelectedTab.Subscribe(tab => UpdateActiveTab(tab)));

        
    }

    private void UpdateActiveTab(MatchCenterTabs tab)
    {
        if (tabLineup != null)
        {
            bool isActive = tab == MatchCenterTabs.Lineup;
            tabLineup.gameObject.SetActive(isActive);
            if (isActive && tabLineup is UIView lineupView)
            {
                lineupView.UpdateUI();
            }
        }

        if (tabScoreboard != null)
        {
            bool isActive = tab == MatchCenterTabs.Scoreboard;
            tabScoreboard.gameObject.SetActive(isActive);
            if (isActive && tabScoreboard is UIView scoreboardView)
            {
                scoreboardView.UpdateUI();
            }
        }

        if (tabNotes != null)
        {
            bool isActive = tab == MatchCenterTabs.Notes;
            tabNotes.gameObject.SetActive(isActive);
            if (isActive && tabNotes is UIView notesView)
            {
                notesView.UpdateUI();
            }
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        UpdateActiveTab(MatchCenter.SelectedTab.Value);
    }
}
