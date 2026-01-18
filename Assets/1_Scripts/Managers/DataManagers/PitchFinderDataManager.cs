using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
public class PitchFinderDataManager : IDataManager
{
    private readonly List<StadiumModel> _allPitches;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public ReactiveProperty<PitchSize?> SelectedSizeFilter { get; } = new ReactiveProperty<PitchSize?>(null);
    public ReactiveProperty<SortPicthesType?> SelectedSortType { get; } = new ReactiveProperty<SortPicthesType?>(null);
    public ReactiveProperty<string> SearchQuery { get; } = new ReactiveProperty<string>(string.Empty);

    public ReactiveCollection<ToggleButtonModel> SizeFilters { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveCollection<ToggleButtonModel> SortFilters { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveCollection<StadiumModel> FilteredPitches { get; } = new ReactiveCollection<StadiumModel>();

    private readonly ReactiveCollection<object> _sizeFiltersAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _sortFiltersAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _filteredPitchesAsObject = new ReactiveCollection<object>();

    public ReactiveCollection<object> SizeFiltersAsObject => _sizeFiltersAsObject;
    public ReactiveCollection<object> SortFiltersAsObject => _sortFiltersAsObject;
    public ReactiveCollection<object> FilteredPitchesAsObject => _filteredPitchesAsObject;

    public StadiumModel GetStadiumById(int stadiumId)
    {
        return _allPitches.FirstOrDefault(s => s.id == stadiumId);
    }

    public PitchFinderDataManager(AppConfig config, AppModel appModel)
    {
        if (appModel.stadiums == null || appModel.stadiums.Count == 0)
        {
            appModel.stadiums = new List<StadiumModel>();
            if (config != null && config.pitches != null)
            {
                foreach (var pitchConfig in config.pitches)
                {
                    appModel.stadiums.Add(new StadiumModel(pitchConfig));
                }
            }
        }

        _allPitches = appModel.stadiums;

        BindMirror(SizeFilters, _sizeFiltersAsObject);
        BindMirror(SortFilters, _sortFiltersAsObject);
        BindMirror(FilteredPitches, _filteredPitchesAsObject);

        InitializeFilters();
        SubscribeToFilterChanges();
        UpdateFilteredPitches();
    }

    private void BindMirror<T>(ReactiveCollection<T> source, ReactiveCollection<object> mirror)
    {
        source.ObserveAdd().Subscribe(e => mirror.Insert(e.Index, e.Value)).AddTo(_disposables);
        source.ObserveRemove().Subscribe(e => mirror.RemoveAt(e.Index)).AddTo(_disposables);
        source.ObserveReplace().Subscribe(e => mirror[e.Index] = e.NewValue).AddTo(_disposables);
        source.ObserveMove().Subscribe(e => mirror.Move(e.OldIndex, e.NewIndex)).AddTo(_disposables);
        source.ObserveReset().Subscribe(_ =>
        {
            mirror.Clear();
            foreach (var item in source) mirror.Add(item);
        }).AddTo(_disposables);
    }

    private void InitializeFilters()
    {
        foreach (PitchSize size in Enum.GetValues(typeof(PitchSize)))
        {
            SizeFilters.Add(new ToggleButtonModel { name = size.ToString(), selected = false });
        }

        foreach (SortPicthesType sortType in Enum.GetValues(typeof(SortPicthesType)))
        {
            SortFilters.Add(new ToggleButtonModel { name = sortType.ToString(), selected = false });
        }
    }

    private void SubscribeToFilterChanges()
    {
        SelectedSizeFilter.Subscribe(_ => UpdateFilteredPitches()).AddTo(_disposables);
        SelectedSortType.Subscribe(_ => UpdateFilteredPitches()).AddTo(_disposables);
        SearchQuery.Subscribe(_ => UpdateFilteredPitches()).AddTo(_disposables);
    }

    public void SelectSizeFilter(PitchSize? size)
    {
        SelectedSizeFilter.Value = size;
        UpdateSizeFiltersSelection();
    }

    public void SelectSortType(SortPicthesType? sortType)
    {
        SelectedSortType.Value = sortType;
        UpdateSortFiltersSelection();
    }

    private void UpdateSizeFiltersSelection()
    {
        var sizes = Enum.GetValues(typeof(PitchSize));
        for (int i = 0; i < SizeFilters.Count && i < sizes.Length; i++)
        {
            var size = (PitchSize)sizes.GetValue(i);
            var model = SizeFilters[i];
            var newSelected = SelectedSizeFilter.Value == size;

            if (model.selected != newSelected)
            {
                SizeFilters[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    private void UpdateSortFiltersSelection()
    {
        var sortTypes = Enum.GetValues(typeof(SortPicthesType));
        for (int i = 0; i < SortFilters.Count && i < sortTypes.Length; i++)
        {
            var sortType = (SortPicthesType)sortTypes.GetValue(i);
            var model = SortFilters[i];
            var newSelected = SelectedSortType.Value == sortType;

            if (model.selected != newSelected)
            {
                SortFilters[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    private void UpdateFilteredPitches()
    {
        var filtered = _allPitches.AsEnumerable();

        if (SelectedSizeFilter.Value.HasValue)
        {
            filtered = filtered.Where(p => p.supportedSizes != null && p.supportedSizes.Contains(SelectedSizeFilter.Value.Value));
        }

        if (!string.IsNullOrEmpty(SearchQuery.Value))
        {
            var query = SearchQuery.Value.ToLower();
            filtered = filtered.Where(p =>
                (p.name != null && p.name.ToLower().Contains(query)) ||
                (p.address != null && p.address.ToLower().Contains(query)));
        }

        if (SelectedSortType.Value.HasValue)
        {
            switch (SelectedSortType.Value.Value)
            {
                case SortPicthesType.Closest:
                    filtered = filtered.OrderBy(p => Vector2.Distance(Vector2.zero, p.location));
                    break;
                case SortPicthesType.TopRated:
                    filtered = filtered.OrderByDescending(p => p.rating);
                    break;
                case SortPicthesType.LowestPrice:
                    filtered = filtered.OrderBy(p => p.basePricePerHour);
                    break;
            }
        }

        FilteredPitches.Clear();
        foreach (var pitch in filtered)
        {
            FilteredPitches.Add(pitch);
        }
    }
}