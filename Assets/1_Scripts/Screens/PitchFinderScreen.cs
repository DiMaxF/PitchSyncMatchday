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
            AddToDispose(searchBar.OnValueChangedAsObservable()
                .Subscribe(query => PitchFinder.SearchQuery.Value = query));
        }
    }

    private void SubscribeToPitchCards() 
    {
        if (pitchCards != null) 
        {
            AddToDispose(UIManager.SubscribeToView(pitchCards, (StadiumModel data) =>
            {
                DataManager.Booking.StartNewBooking(data);
                ScreenManager.Show(Screens.AvailabilityPlannerScreen);
            }));
        }
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();
        SubscribeToSearchBar();
        SubscribeToPitchCards();
        if (sizeFilteres != null)
        {
            AddToDispose(UIManager.SubscribeToView(sizeFilteres, (ToggleButtonModel data) =>
            {
                if (Enum.TryParse<PitchSize>(data.name, out var size))
                {
                    var isSelected = PitchFinder.SelectedSizeFilter.Value == size;
                    PitchFinder.SelectSizeFilter(isSelected ? (PitchSize?)null : size);
                }
            }));
        }

        if (sortFilteres != null)
        {
            AddToDispose(UIManager.SubscribeToView(sortFilteres, (ToggleButtonModel data) =>
            {
                if (Enum.TryParse<SortPicthesType>(data.name, out var sortType))
                {
                    var isSelected = PitchFinder.SelectedSortType.Value == sortType;
                    PitchFinder.SelectSortType(isSelected ? (SortPicthesType?)null : sortType);
                }
            }));
        }
    }
}

