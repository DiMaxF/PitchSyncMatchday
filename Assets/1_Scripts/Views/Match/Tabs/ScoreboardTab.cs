using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardTab : UIView
{
    [SerializeField] private Text score;
    [SerializeField] private Button goalRed;
    [SerializeField] private Button goalGreen;

    [SerializeField] private Text timer;
    [SerializeField] private ListContainer timerStates;
    [SerializeField] private Button finishTimer;

    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (goalGreen != null)
        {
            goalGreen.OnClickAsObservable()
                .Subscribe(_ => MatchCenter.AddGoal(TeamSide.Green))
                .AddTo(this);
        }

        if (goalRed != null)
        {
            goalRed.OnClickAsObservable()
                .Subscribe(_ => MatchCenter.AddGoal(TeamSide.Red))
                .AddTo(this);
        }

        if (finishTimer != null)
        {
            finishTimer.OnClickAsObservable()
                .Subscribe(_ => MatchCenter.FinishMatch())
                .AddTo(this);
        }

        if (timerStates != null)
        {
            timerStates.Init(MatchCenter.TimerStatesAsObject);

            AddToDispose(UIManager.SubscribeToView(timerStates, (ToggleButtonModel data) =>
            {
                if (Enum.TryParse<TimerState>(data.name, out var state))
                {
                    MatchCenter.SelectTimerState(state);
                }
            }));
        }

        AddToDispose(MatchCenter.ScoreBlue.Subscribe(_ => UpdateScore()));
        AddToDispose(MatchCenter.ScoreRed.Subscribe(_ => UpdateScore()));
        AddToDispose(MatchCenter.ElapsedSeconds.Subscribe(_ => UpdateTimer()));
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        UpdateScore();
        UpdateTimer();
    }

    private void UpdateScore()
    {
        if (score != null)
        {
            score.text = $"{MatchCenter.ScoreBlue.Value} — {MatchCenter.ScoreRed.Value}";
        }
    }

    private void UpdateTimer()
    {
        if (timer != null)
        {
            int totalSeconds = MatchCenter.ElapsedSeconds.Value;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timer.text = $"{minutes:D2}:{seconds:D2}";
        }
    }
}
