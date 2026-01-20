using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FinishBookingScreen : UIScreen
{
    [SerializeField] private Text score;
    [SerializeField] private Text time;
    [SerializeField] private ListContainer keyMoments;
    [SerializeField] private MatchSchemeView schemeView;

    [SerializeField] private Button goHomeButton;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (MatchCenter != null && MatchCenter.CurrentMatch.Value != null)
        {
            MatchCenter.FinishMatch();
        }

        if (keyMoments != null && MatchCenter != null)
        {
            keyMoments.Init(MatchCenter.EventsAsObject);
        }

        if (schemeView != null && MatchCenter != null)
        {
            if (MatchCenter.CurrentLineup.Value != null)
            {
                schemeView.Init(MatchCenter.CurrentLineup.Value);
            }
            
            AddToDispose(MatchCenter.CurrentLineup.Subscribe(lineup =>
            {
                if (lineup != null)
                {
                    schemeView.Init(lineup);
                }
            }));
        }

        if (MatchCenter != null)
        {
            AddToDispose(MatchCenter.ScoreBlue.Subscribe(_ => UpdateScore()));
            AddToDispose(MatchCenter.ScoreRed.Subscribe(_ => UpdateScore()));
            AddToDispose(MatchCenter.ElapsedSeconds.Subscribe(_ => UpdateTime()));
        }

        if (goHomeButton != null)
        {
            AddToDispose(goHomeButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.HomeScreen);
                }));
        }

        UpdateScore();
        UpdateTime();
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        UpdateScore();
        UpdateTime();
        
        if (MatchCenter != null && MatchCenter.CurrentLineup.Value != null)
        {
            schemeView?.Init(MatchCenter.CurrentLineup.Value);
        }
    }

    private void UpdateScore()
    {
        if (score != null && MatchCenter != null)
        {
            score.text = $"{MatchCenter.ScoreBlue.Value} — {MatchCenter.ScoreRed.Value}";
        }
    }

    private void UpdateTime()
    {
        if (time != null && MatchCenter != null)
        {
            int totalSeconds = MatchCenter.ElapsedSeconds.Value;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            time.text = $"{minutes:D2}:{seconds:D2}";
        }
    }
}
