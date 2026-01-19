using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AvailabilityPlannerScreen : UIScreen
{
    [SerializeField] private Button backButton;
    [SerializeField] private CalendarView calendar;
    [SerializeField] private ListContainer morningList;
    [SerializeField] private ListContainer afternoonList;
    [SerializeField] private ListContainer eveningList;
    [SerializeField] private Button continueButton;

    private BookingDataManager Booking => DataManager.Booking;

    protected override void OnEnable()
    {
        base.OnEnable();
        InitializeCalendar();
        InitializeTimeLists();
    }

    private void InitializeCalendar()
    {
        if (calendar != null)
        {
            var tomorrow = DateTime.Now.AddDays(1);
            calendar.Init(tomorrow);
        }
    }

    private void InitializeTimeLists()
    {
        new Log($"{Booking.MorningTimesAsObject.Count}", "InitializeTimeLists");
        if (morningList != null)
        {
            morningList.Init(Booking.MorningTimesAsObject);
        }

        if (afternoonList != null)
        {
            afternoonList.Init(Booking.AfternoonTimesAsObject);
        }

        if (eveningList != null)
        {
            eveningList.Init(Booking.EveningTimesAsObject);
        }
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (backButton != null)
        {
            AddToDispose(backButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.PitchFinderScreen);
                }));
        }

        if (continueButton != null)
        {
            var canContinue = Observable.CombineLatest(
                Booking.SelectedDate,
                Booking.SelectedTime,
                (date, time) => date.HasValue && !string.IsNullOrEmpty(time)
            );

            AddToDispose(canContinue.SubscribeToInteractable(continueButton));

            AddToDispose(continueButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.BookingBuilderScreen);
                }));
        }

        if (calendar != null)
        {
            AddToDispose(UIManager.SubscribeToView(calendar, (DateTime date) =>
            {
                Booking.SetSelectedDate(date);
            }));
        }

        if (morningList != null)
        {
            AddToDispose(UIManager.SubscribeToView(morningList, (ToggleButtonModel data) =>
            {
                Booking.SelectTimeSlot(data, Booking.MorningTimes);
            }));
        }

        if (afternoonList != null)
        {
            AddToDispose(UIManager.SubscribeToView(afternoonList, (ToggleButtonModel data) =>
            {
                Booking.SelectTimeSlot(data, Booking.AfternoonTimes);
            }));
        }

        if (eveningList != null)
        {
            AddToDispose(UIManager.SubscribeToView(eveningList, (ToggleButtonModel data) =>
            {
                Booking.SelectTimeSlot(data, Booking.EveningTimes);
            }));
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
    }
}
