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
            tabLineup.gameObject.SetActive(tab == MatchCenterTabs.Lineup);
        }

        if (tabScoreboard != null)
        {
            tabScoreboard.gameObject.SetActive(tab == MatchCenterTabs.Scoreboard);
        }

        if (tabNotes != null)
        {
            tabNotes.gameObject.SetActive(tab == MatchCenterTabs.Notes);
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        UpdateActiveTab(MatchCenter.SelectedTab.Value);
    }
}
