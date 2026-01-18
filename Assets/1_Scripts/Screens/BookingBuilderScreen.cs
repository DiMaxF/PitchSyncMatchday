using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BookingBuilderScreen : UIScreen
{
    [SerializeField] ShortBookingCard shortBookingCard;
    [SerializeField] ListContainer pitchSizesContainer;
    [SerializeField] ListContainer durationContainer;
    [SerializeField] ListContainer extrasContainer;
    [SerializeField] Text total;
    [SerializeField] Button backButton;
    [SerializeField] Button generateButton;

    private BookingDataManager Booking => DataManager.Booking;

    protected override void OnEnable()
    {
        base.OnEnable();
        InitializeContainers();
        InitializeExtras();
    }

    private void InitializeContainers()
    {
        if (pitchSizesContainer != null)
        {
            pitchSizesContainer.Init(Booking.PitchSizesAsObject);
        }

        if (durationContainer != null)
        {
            durationContainer.Init(Booking.DurationsAsObject);
        }
    }

    private void InitializeExtras()
    {
        if (extrasContainer != null)
        {
            extrasContainer.Init(Booking.AvailableExtrasAsObject);
        }
    }


    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (shortBookingCard != null && Booking.CurrentDraft.Value != null)
        {
            shortBookingCard.Init(Booking.CurrentDraft.Value);
        }

        if (backButton != null)
        {
            AddToDispose(backButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.AvailabilityPlannerScreen);
                }));
        }

        if (generateButton != null)
        {
            AddToDispose(generateButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnGenerateClicked();
                }));
        }

        if (pitchSizesContainer != null)
        {
            AddToDispose(UIManager.SubscribeToView(pitchSizesContainer, (ToggleButtonModel data) =>
            {
                if (System.Enum.TryParse<PitchSize>(data.name, out var size))
                {
                    Booking.SetPitchSize(size);
                }
            }));
        }

        if (durationContainer != null)
        {
            AddToDispose(UIManager.SubscribeToView(durationContainer, (ToggleButtonModel data) =>
            {
                if (System.Enum.TryParse<MatchDuration>(data.name, out var duration))
                {
                    Booking.SetDuration(duration);
                }
            }));
        }

        if (extrasContainer != null)
        {
            AddToDispose(UIManager.SubscribeToView(extrasContainer, (BookingExtraModel data) =>
            {
                if (data != null)
                {
                    Booking.SetExtraQuantity(data.type, data.currentQuantity);
                }
            }));
        }

        AddToDispose(Booking.CurrentDraft.Subscribe(draft =>
        {
            if (draft != null)
            {
                if (shortBookingCard != null)
                {
                    shortBookingCard.Init(draft);
                }
                InitializeExtras();
            }
        }));

        AddToDispose(Booking.SelectedExtras.Subscribe(_ =>
        {
            InitializeExtras();
        }));

        AddToDispose(Booking.TotalCost.Subscribe(cost =>
        {
            if (total != null)
            {
                total.text = $"${cost:F2}";
            }
        }));
    }

    private void OnGenerateClicked()
    {
        if (Booking.CurrentDraft.Value == null)
        {
            Debug.LogWarning("Нет активного букинга");
            return;
        }

        Booking.ConfirmBooking();
        ScreenManager?.Show(Screens.MyBookingScreen);
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        if (total != null && Booking.TotalCost.Value > 0)
        {
            total.text = $"${Booking.TotalCost.Value:F2}";
        }
        InitializeExtras();
    }
}
