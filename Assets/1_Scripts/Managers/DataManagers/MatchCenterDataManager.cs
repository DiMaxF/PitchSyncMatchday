using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class MatchCenterDataManager : IDataManager
{
    private readonly AppModel _appModel;
    private readonly AppConfig _appConfig;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private IDisposable _timerSubscription;
    private bool _isSyncingNotes = false;

    public ReactiveProperty<MatchModel> CurrentMatch { get; } = new ReactiveProperty<MatchModel>(null);
    public ReactiveProperty<LineupModel> CurrentLineup { get; } = new ReactiveProperty<LineupModel>(null);
    public ReactiveProperty<int> ElapsedSeconds { get; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> ScoreBlue { get; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> ScoreRed { get; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<string> Notes { get; } = new ReactiveProperty<string>("");
    public ReactiveCollection<MatchEvent> Events { get; } = new ReactiveCollection<MatchEvent>();
    public ReactiveCollection<string> NotesItems { get; } = new ReactiveCollection<string>();
    private readonly ReactiveCollection<object> _eventsAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> EventsAsObject => _eventsAsObject;
    private readonly ReactiveCollection<object> _notesItemsAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> NotesItemsAsObject => _notesItemsAsObject;

    public ReactiveProperty<MatchCenterTabs> SelectedTab { get; } = new ReactiveProperty<MatchCenterTabs>(MatchCenterTabs.Lineup);
    public ReactiveCollection<ToggleButtonModel> Tabs { get; } = new ReactiveCollection<ToggleButtonModel>();
    private readonly ReactiveCollection<object> _tabsAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> TabsAsObject => _tabsAsObject;

    public ReactiveProperty<TimerState> CurrentTimerState { get; } = new ReactiveProperty<TimerState>(TimerState.Pause);
    public ReactiveCollection<ToggleButtonModel> TimerStates { get; } = new ReactiveCollection<ToggleButtonModel>();
    private readonly ReactiveCollection<object> _timerStatesAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> TimerStatesAsObject => _timerStatesAsObject;

    public MatchCenterDataManager(AppModel appModel, AppConfig appConfig)
    {
        _appModel = appModel;
        _appConfig = appConfig;

        if (_appModel.matches == null)
        {
            _appModel.matches = new List<MatchModel>();
        }
        if (_appModel.lineups == null)
        {
            _appModel.lineups = new List<LineupModel>();
        }

        BindEventsCollection();
        BindNotesCollection();
        InitializeTabs();
        InitializeTimerStates();
        SubscribeToChanges();
        SelectedTab.Subscribe(_ => UpdateTabsSelection()).AddTo(_disposables);
        CurrentTimerState.Subscribe(_ => UpdateTimerStatesSelection()).AddTo(_disposables);
    }

    private void BindEventsCollection()
    {
        _eventsAsObject.Clear();
        foreach (var evt in Events)
        {
            _eventsAsObject.Add(evt);
        }

        Events.ObserveAdd().Subscribe(e => _eventsAsObject.Insert(e.Index, e.Value)).AddTo(_disposables);
        Events.ObserveRemove().Subscribe(e => _eventsAsObject.RemoveAt(e.Index)).AddTo(_disposables);
        Events.ObserveReplace().Subscribe(e => _eventsAsObject[e.Index] = e.NewValue).AddTo(_disposables);
        Events.ObserveReset().Subscribe(_ =>
        {
            _eventsAsObject.Clear();
            foreach (var evt in Events) _eventsAsObject.Add(evt);
        }).AddTo(_disposables);
    }

    private void LoadNotesItemsFromString(string notes)
    {
        if (_isSyncingNotes) return;
        _isSyncingNotes = true;

        NotesItems.Clear();
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var parts = notes.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    NotesItems.Add(trimmed);
                }
            }
        }

        _isSyncingNotes = false;
    }

    private void UpdateNotesFromItems()
    {
        if (_isSyncingNotes) return;
        _isSyncingNotes = true;
        Notes.Value = string.Join("\n", NotesItems);
        _isSyncingNotes = false;
    }

    private void BindNotesCollection()
    {
        _notesItemsAsObject.Clear();
        foreach (var note in NotesItems)
        {
            _notesItemsAsObject.Add(note);
        }

        NotesItems.ObserveAdd().Subscribe(e => _notesItemsAsObject.Insert(e.Index, e.Value)).AddTo(_disposables);
        NotesItems.ObserveRemove().Subscribe(e => _notesItemsAsObject.RemoveAt(e.Index)).AddTo(_disposables);
        NotesItems.ObserveReplace().Subscribe(e => _notesItemsAsObject[e.Index] = e.NewValue).AddTo(_disposables);
        NotesItems.ObserveReset().Subscribe(_ =>
        {
            _notesItemsAsObject.Clear();
            foreach (var note in NotesItems) _notesItemsAsObject.Add(note);
        }).AddTo(_disposables);

        NotesItems.ObserveAdd().Subscribe(_ => UpdateNotesFromItems()).AddTo(_disposables);
        NotesItems.ObserveRemove().Subscribe(_ => UpdateNotesFromItems()).AddTo(_disposables);
        NotesItems.ObserveReplace().Subscribe(_ => UpdateNotesFromItems()).AddTo(_disposables);
        NotesItems.ObserveReset().Subscribe(_ => UpdateNotesFromItems()).AddTo(_disposables);
    }

    private void SubscribeToChanges()
    {
        ElapsedSeconds.Subscribe(seconds =>
        {
            if (CurrentMatch.Value != null)
            {
                CurrentMatch.Value.elapsedSeconds = seconds;
                AutoSave();
            }
        }).AddTo(_disposables);

        ScoreBlue.Subscribe(score =>
        {
            if (CurrentMatch.Value != null)
            {
                CurrentMatch.Value.scoreBlue = score;
                AutoSave();
            }
        }).AddTo(_disposables);

        ScoreRed.Subscribe(score =>
        {
            if (CurrentMatch.Value != null)
            {
                CurrentMatch.Value.scoreOrange = score;
                AutoSave();
            }
        }).AddTo(_disposables);

        Notes.Subscribe(notes =>
        {
            if (CurrentMatch.Value != null)
            {
                CurrentMatch.Value.notes = notes;
                AutoSave();
            }
        }).AddTo(_disposables);

        Events.ObserveAdd().Subscribe(_ => AutoSave()).AddTo(_disposables);
        Events.ObserveRemove().Subscribe(_ => AutoSave()).AddTo(_disposables);
        Events.ObserveReplace().Subscribe(_ => AutoSave()).AddTo(_disposables);
    }

    public void InitializeFromLineup(int? lineupId)
    {
        var lineup = _appModel.lineups.FirstOrDefault(l => l.id == lineupId);
        if (lineup == null) return;

        var match = new MatchModel(null, _appModel);
        match.lineupId = lineupId;
        match.pitchName = "Custom Match";
        match.pitchSize = _appModel.defaultPitchSize;
        match.totalDurationMinutes = (int)_appModel.defaultDuration;
        match.status = MatchStatus.Upcoming;

        InitializeMatch(match, lineup);
    }

    public void InitializeFromBooking(int? bookingId)
    {
        var booking = _appModel.bookings.FirstOrDefault(b => b.id == bookingId);
        if (booking == null) return;

        MatchModel match = null;
        if (booking.matchId.HasValue)
        {
            match = _appModel.matches.FirstOrDefault(m => m.id == booking.matchId.Value);
        }

        if (match == null)
        {
            match = new MatchModel(bookingId, _appModel);
            var stadium = _appModel.stadiums.FirstOrDefault(s => s.id == booking.stadiumId);
            match.pitchName = stadium != null ? stadium.name : "Unknown Pitch";
            match.pitchSize = booking.pitchSize;
            match.totalDurationMinutes = (int)booking.duration;
            match.status = MatchStatus.Upcoming;
        }

        LineupModel lineup = null;
        if (match.lineupId.HasValue)
        {
            lineup = _appModel.lineups.FirstOrDefault(l => l.id == match.lineupId.Value);
        }

        InitializeMatch(match, lineup);
    }

    private void InitializeMatch(MatchModel match, LineupModel lineup)
    {
        if (!_appModel.matches.Contains(match))
        {
            _appModel.matches.Add(match);
        }

        CurrentMatch.Value = match;
        CurrentLineup.Value = lineup;

        ElapsedSeconds.Value = match.elapsedSeconds;
        ScoreBlue.Value = match.scoreBlue;
        ScoreRed.Value = match.scoreOrange;
        Notes.Value = match.notes ?? "";
        LoadNotesItemsFromString(Notes.Value);

        if (match.status == MatchStatus.Live)
        {
            CurrentTimerState.Value = TimerState.Start;
            StartTimer();
        }
        else
        {
            CurrentTimerState.Value = TimerState.Pause;
        }

        Events.Clear();
        if (match.events != null)
        {
            foreach (var evt in match.events.OrderBy(e => e.elapsedSeconds))
            {
                Events.Add(evt);
            }
        }
    }

    public void StartMatch()
    {
        if (CurrentMatch.Value == null) return;

        if (CurrentMatch.Value.status == MatchStatus.Upcoming)
        {
            CurrentMatch.Value.startTimeIso = DateTime.UtcNow.ToString("o");
        }

        CurrentMatch.Value.status = MatchStatus.Live;
        CurrentTimerState.Value = TimerState.Start;
        StartTimer();
        AutoSave();
    }

    public void PauseMatch()
    {
        StopTimer();
        CurrentTimerState.Value = TimerState.Pause;
        AutoSave();
    }

    public void ResumeMatch()
    {
        if (CurrentMatch.Value != null && CurrentMatch.Value.status == MatchStatus.Live)
        {
            CurrentTimerState.Value = TimerState.Start;
            StartTimer();
        }
    }

    public void SelectTimerState(TimerState state)
    {
        if (CurrentMatch.Value == null) return;

        if (state == TimerState.Start)
        {
            if (CurrentMatch.Value.status == MatchStatus.Upcoming)
            {
                StartMatch();
            }
            else if (CurrentMatch.Value.status == MatchStatus.Live)
            {
                ResumeMatch();
            }
        }
        else if (state == TimerState.Pause)
        {
            PauseMatch();
        }
        else if (state == TimerState.Finish)
        {
            FinishMatch();
        }
    }

    private void StartTimer()
    {
        StopTimer();

        _timerSubscription = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                if (CurrentMatch.Value != null && CurrentMatch.Value.status == MatchStatus.Live)
                {
                    var newElapsed = ElapsedSeconds.Value + 1;
                    if (newElapsed <= CurrentMatch.Value.totalDurationMinutes * 60)
                    {
                        ElapsedSeconds.Value = newElapsed;
                    }
                    else
                    {
                        FinishMatch();
                    }
                }
            })
            .AddTo(_disposables);
    }

    private void StopTimer()
    {
        _timerSubscription?.Dispose();
        _timerSubscription = null;
    }

    public void AddGoal(TeamSide team, int? playerId = null)
    {
        if (CurrentMatch.Value == null) return;

        int oldScoreBlue = ScoreBlue.Value;
        int oldScoreRed = ScoreRed.Value;

        if (team == TeamSide.Green)
        {
            ScoreBlue.Value++;
        }
        else if (team == TeamSide.Red)
        {
            ScoreRed.Value++;
        }

        var currentMinute = ElapsedSeconds.Value / 60;
        string description = "Goal";
        
        if (ScoreBlue.Value == ScoreRed.Value && (oldScoreBlue != oldScoreRed))
        {
            description = "equalized";
        }

        var evt = new MatchEvent(ElapsedSeconds.Value, MatchEventType.Goal, team, playerId, description);
        AddEvent(evt);
    }

    public void AddCustomEvent(MatchEventType eventType, TeamSide team, int? playerId = null, string description = "")
    {
        var evt = new MatchEvent(ElapsedSeconds.Value, eventType, team, playerId, description);
        AddEvent(evt);
    }

    private void AddEvent(MatchEvent evt)
    {
        if (CurrentMatch.Value == null) return;

        if (CurrentMatch.Value.events == null)
        {
            CurrentMatch.Value.events = new List<MatchEvent>();
        }

        CurrentMatch.Value.events.Add(evt);
        Events.Add(evt);

        var sorted = Events.OrderBy(e => e.elapsedSeconds).ToList();
        Events.Clear();
        foreach (var e in sorted)
        {
            Events.Add(e);
        }
    }

    public void UpdateNotes(string notes)
    {
        Notes.Value = notes;
        LoadNotesItemsFromString(notes);
    }

    public void AddNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note)) return;
        NotesItems.Add(note.Trim());
    }

    public void FinishMatch()
    {
        if (CurrentMatch.Value == null) return;

        StopTimer();
        CurrentTimerState.Value = TimerState.Pause;
        CurrentMatch.Value.status = MatchStatus.Finished;
        CurrentMatch.Value.endTimeIso = DateTime.UtcNow.ToString("o");

        SaveMatch();

        if (CurrentMatch.Value.bookingId.HasValue)
        {
            var booking = _appModel.bookings.FirstOrDefault(b => b.id == CurrentMatch.Value.bookingId.Value);
            if (booking != null)
            {
                booking.status = BookingStatus.Finished;
                booking.matchId = CurrentMatch.Value.id;
            }
        }

        _appModel.matchesPlayed++;
        DataManager.Instance.SaveAppModel();
    }

    private void AutoSave()
    {
        if (CurrentMatch.Value != null)
        {
            SaveMatch();
        }
    }

    private void SaveMatch()
    {
        if (CurrentMatch.Value == null) return;

        if (CurrentLineup.Value != null)
        {
            CurrentLineup.Value.matchId = CurrentMatch.Value.id;
            if (!_appModel.lineups.Contains(CurrentLineup.Value))
            {
                _appModel.lineups.Add(CurrentLineup.Value);
            }
        }

        DataManager.Instance.SaveAppModel();
    }

    private void InitializeTabs()
    {
        foreach (MatchCenterTabs tab in Enum.GetValues(typeof(MatchCenterTabs)))
        {
            Tabs.Add(new ToggleButtonModel
            {
                name = tab.ToString(),
                selected = tab == SelectedTab.Value
            });
        }

        BindTabsCollection();
        UpdateTabsSelection();
    }

    private void BindTabsCollection()
    {
        _tabsAsObject.Clear();
        foreach (var tab in Tabs)
        {
            _tabsAsObject.Add(tab);
        }

        Tabs.ObserveAdd().Subscribe(e => _tabsAsObject.Insert(e.Index, e.Value)).AddTo(_disposables);
        Tabs.ObserveRemove().Subscribe(e => _tabsAsObject.RemoveAt(e.Index)).AddTo(_disposables);
        Tabs.ObserveReplace().Subscribe(e => _tabsAsObject[e.Index] = e.NewValue).AddTo(_disposables);
        Tabs.ObserveReset().Subscribe(_ =>
        {
            _tabsAsObject.Clear();
            foreach (var tab in Tabs) _tabsAsObject.Add(tab);
        }).AddTo(_disposables);
    }

    private void UpdateTabsSelection()
    {
        var tabs = Enum.GetValues(typeof(MatchCenterTabs)).Cast<MatchCenterTabs>().ToList();
        for (int i = 0; i < Tabs.Count && i < tabs.Count; i++)
        {
            var tab = tabs[i];
            var model = Tabs[i];
            var newSelected = SelectedTab.Value == tab;

            if (model.selected != newSelected)
            {
                Tabs[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    public void SelectTab(MatchCenterTabs tab)
    {
        SelectedTab.Value = tab;
    }

    private void InitializeTimerStates()
    {
        foreach (TimerState state in Enum.GetValues(typeof(TimerState)))
        {
            TimerStates.Add(new ToggleButtonModel
            {
                name = state.ToString(),
                selected = state == CurrentTimerState.Value
            });
        }

        BindTimerStatesCollection();
        UpdateTimerStatesSelection();
    }

    private void BindTimerStatesCollection()
    {
        _timerStatesAsObject.Clear();
        foreach (var state in TimerStates)
        {
            _timerStatesAsObject.Add(state);
        }

        TimerStates.ObserveAdd().Subscribe(e => _timerStatesAsObject.Insert(e.Index, e.Value)).AddTo(_disposables);
        TimerStates.ObserveRemove().Subscribe(e => _timerStatesAsObject.RemoveAt(e.Index)).AddTo(_disposables);
        TimerStates.ObserveReplace().Subscribe(e => _timerStatesAsObject[e.Index] = e.NewValue).AddTo(_disposables);
        TimerStates.ObserveReset().Subscribe(_ =>
        {
            _timerStatesAsObject.Clear();
            foreach (var state in TimerStates) _timerStatesAsObject.Add(state);
        }).AddTo(_disposables);
    }

    private void UpdateTimerStatesSelection()
    {
        var states = Enum.GetValues(typeof(TimerState)).Cast<TimerState>().ToList();
        for (int i = 0; i < TimerStates.Count && i < states.Count; i++)
        {
            var state = states[i];
            var model = TimerStates[i];
            var newSelected = CurrentTimerState.Value == state;

            if (model.selected != newSelected)
            {
                TimerStates[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    public void Dispose()
    {
        StopTimer();
        _disposables.Dispose();
    }
}

