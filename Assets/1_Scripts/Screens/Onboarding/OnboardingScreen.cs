using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnboardingScreen : UIScreen
{
    [SerializeField] private ListContainer defaultPitchSize;
    [SerializeField] private ListContainer defaultDuration;
    [SerializeField] private Button startButton;

    private ProfileDataManager Profile => DataManager.Profile;
    
    private ReactiveCollection<ToggleButtonModel> _pitchSizes = new ReactiveCollection<ToggleButtonModel>();
    private ReactiveCollection<ToggleButtonModel> _durations = new ReactiveCollection<ToggleButtonModel>();
    private readonly ReactiveCollection<object> _pitchSizesAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _durationsAsObject = new ReactiveCollection<object>();

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        InitializePitchSizes();
        InitializeDurations();

        if (defaultPitchSize != null)
        {
            defaultPitchSize.Init(_pitchSizesAsObject);
            
            AddToDispose(UIManager.SubscribeToView(defaultPitchSize, (ToggleButtonModel data) =>
            {
                if (System.Enum.TryParse<PitchSize>(data.name, out var size))
                {
                    Profile.DefaultPitchSize.Value = size;
                    UpdatePitchSizesSelection();
                }
            }));
        }

        if (defaultDuration != null)
        {
            defaultDuration.Init(_durationsAsObject);
            
            AddToDispose(UIManager.SubscribeToView(defaultDuration, (ToggleButtonModel data) =>
            {
                if (System.Enum.TryParse<MatchDuration>(data.name, out var duration))
                {
                    Profile.DefaultDuration.Value = duration;
                    UpdateDurationsSelection();
                }
            }));
        }

        if (startButton != null)
        {
            AddToDispose(startButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    DataManager.Instance.SaveAppModel();
                    SceneManager.LoadScene("SampleScene");
                }));
        }

        AddToDispose(Profile.DefaultPitchSize.Subscribe(_ => UpdatePitchSizesSelection()));
        AddToDispose(Profile.DefaultDuration.Subscribe(_ => UpdateDurationsSelection()));
    }

    private void InitializePitchSizes()
    {
        _pitchSizes.Clear();
        var pitchSizes = System.Enum.GetValues(typeof(PitchSize)).Cast<PitchSize>();
        foreach (var size in pitchSizes)
        {
            var isSelected = Profile.DefaultPitchSize.Value == size;
            _pitchSizes.Add(new ToggleButtonModel
            {
                name = size.ToString(),
                selected = isSelected
            });
        }
        
        BindPitchSizesCollection();
    }

    private void InitializeDurations()
    {
        _durations.Clear();
        var durations = System.Enum.GetValues(typeof(MatchDuration)).Cast<MatchDuration>();
        foreach (var duration in durations)
        {
            var isSelected = Profile.DefaultDuration.Value == duration;
            _durations.Add(new ToggleButtonModel
            {
                name = duration.ToString(),
                selected = isSelected
            });
        }
        
        BindDurationsCollection();
    }

    private void BindPitchSizesCollection()
    {
        _pitchSizesAsObject.Clear();
        foreach (var item in _pitchSizes)
        {
            _pitchSizesAsObject.Add(item);
        }

        _pitchSizes.ObserveAdd().Subscribe(e => _pitchSizesAsObject.Insert(e.Index, e.Value)).AddTo(this);
        _pitchSizes.ObserveRemove().Subscribe(e => _pitchSizesAsObject.RemoveAt(e.Index)).AddTo(this);
        _pitchSizes.ObserveReplace().Subscribe(e => _pitchSizesAsObject[e.Index] = e.NewValue).AddTo(this);
        _pitchSizes.ObserveReset().Subscribe(_ =>
        {
            _pitchSizesAsObject.Clear();
            foreach (var item in _pitchSizes) _pitchSizesAsObject.Add(item);
        }).AddTo(this);
    }

    private void BindDurationsCollection()
    {
        _durationsAsObject.Clear();
        foreach (var item in _durations)
        {
            _durationsAsObject.Add(item);
        }

        _durations.ObserveAdd().Subscribe(e => _durationsAsObject.Insert(e.Index, e.Value)).AddTo(this);
        _durations.ObserveRemove().Subscribe(e => _durationsAsObject.RemoveAt(e.Index)).AddTo(this);
        _durations.ObserveReplace().Subscribe(e => _durationsAsObject[e.Index] = e.NewValue).AddTo(this);
        _durations.ObserveReset().Subscribe(_ =>
        {
            _durationsAsObject.Clear();
            foreach (var item in _durations) _durationsAsObject.Add(item);
        }).AddTo(this);
    }

    private void UpdatePitchSizesSelection()
    {
        var sizes = System.Enum.GetValues(typeof(PitchSize)).Cast<PitchSize>().ToList();
        for (int i = 0; i < _pitchSizes.Count && i < sizes.Count; i++)
        {
            var size = sizes[i];
            var model = _pitchSizes[i];
            var newSelected = Profile.DefaultPitchSize.Value == size;

            if (model.selected != newSelected)
            {
                _pitchSizes[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    private void UpdateDurationsSelection()
    {
        var durations = System.Enum.GetValues(typeof(MatchDuration)).Cast<MatchDuration>().ToList();
        for (int i = 0; i < _durations.Count && i < durations.Count; i++)
        {
            var duration = durations[i];
            var model = _durations[i];
            var newSelected = Profile.DefaultDuration.Value == duration;

            if (model.selected != newSelected)
            {
                _durations[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }
}
