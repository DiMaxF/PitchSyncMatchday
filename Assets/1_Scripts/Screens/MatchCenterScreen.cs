using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MatchCenterScreen : UIScreen
{
    [SerializeField] private Button backButton;
    [SerializeField] private Text statusMatchText;
    [SerializeField] private Text subtitle;
    [SerializeField] private ListContainer screenTabs;

    [SerializeField] private UIView tabLineup;
    [SerializeField] private UIView tabScoreboard;
    [SerializeField] private UIView tabNotes;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;
    private PitchFinderDataManager PitchFinder => DataManager.PitchFinder;
    private BookingDataManager Booking => DataManager.Booking;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();
        backButton.OnClickAsObservable()
        .Subscribe(_ =>
        {
            ScreenManager.Back();
        })
        .AddTo(this);
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
        AddToDispose(MatchCenter.CurrentMatch.Subscribe(_ => UpdateSubtitle()));
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
        UpdateSubtitle();
    }

    private void UpdateSubtitle()
    {
        if (subtitle == null || MatchCenter == null || MatchCenter.CurrentMatch.Value == null)
            return;

        var match = MatchCenter.CurrentMatch.Value;
        
        if (match.bookingId.HasValue)
        {
            var booking = Booking.AllBookings.FirstOrDefault(b => b.id == match.bookingId.Value);
            if (booking != null)
            {
                var stadium = PitchFinder.GetStadiumById(booking.stadiumId);
                string stadiumName = stadium != null ? stadium.name : "Unknown Stadium";
                
                string timeText = "";
                if (DateTime.TryParse(booking.dateTimeIso, out var dateTime))
                {
                    timeText = dateTime.ToString("h:mm tt");
                }
                
                if (!string.IsNullOrEmpty(timeText))
                {
                    subtitle.text = $"{stadiumName} - {timeText}";
                }
                else
                {
                    subtitle.text = stadiumName;
                }
                return;
            }
        }
        
        if (!string.IsNullOrEmpty(match.pitchName))
        {
            string timeText = "";
            if (DateTime.TryParse(match.startTimeIso, out var startTime))
            {
                timeText = startTime.ToString("h:mm tt");
            }
            
            if (!string.IsNullOrEmpty(timeText))
            {
                subtitle.text = $"{match.pitchName} - {timeText}";
            }
            else
            {
                subtitle.text = match.pitchName;
            }
        }
        else
        {
            subtitle.text = "";
        }
    }
}
