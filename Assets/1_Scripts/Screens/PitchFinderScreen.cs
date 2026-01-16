using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PitchFinderScreen : UIScreen
{
    [SerializeField] private InputField searchBar;
    [SerializeField] private ListContainer sortFilteres;
    [SerializeField] private ListContainer sizeFilteres;
    [SerializeField] private ListContainer pitchCards;

    private PitchFinderDataManager PitchFinder => DataManager.PitchFinder;

    protected override void OnEnable()
    {
        base.OnEnable();
        InitializeFilters();
        InitializePitchCards();
        SubscribeToSearchBar();
    }

    private void InitializeFilters()
    {
        if (sizeFilteres != null)
        {
            sizeFilteres.Init(PitchFinder.SizeFiltersAsObject);
        }

        if (sortFilteres != null)
        {
            sortFilteres.Init(PitchFinder.SortFiltersAsObject);
        }
    }

    private void InitializePitchCards()
    {
        if (pitchCards != null)
        {
            pitchCards.Init(PitchFinder.FilteredPitchesAsObject);
        }
    }

    private void SubscribeToSearchBar()
    {
        if (searchBar != null)
        {
            searchBar.OnValueChangedAsObservable()
                .Subscribe(query => PitchFinder.SearchQuery.Value = query)
                .AddTo(this);
        }
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (sizeFilteres != null)
        {
            UIManager.SubscribeToView(sizeFilteres, (ToggleButtonModel data) =>
            {
                if (data.selected)
                {
                    PitchFinder.SelectSizeFilter(null);
                }
                else
                {
                    if (Enum.TryParse<PitchSize>(data.name, out var size))
                    {
                        PitchFinder.SelectSizeFilter(size);
                    }
                }
            }, persistent: true).AddTo(this);
        }

        if (sortFilteres != null)
        {
            UIManager.SubscribeToView(sortFilteres, (ToggleButtonModel data) =>
            {
                if (data.selected)
                {
                    PitchFinder.SelectSortType(null);
                }
                else
                {
                    if (Enum.TryParse<SortPicthesType>(data.name, out var sortType))
                    {
                        PitchFinder.SelectSortType(sortType);
                    }
                }
            }, persistent: true).AddTo(this);
        }
    }
}

