using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyBookingScreen : UIScreen
{
    [SerializeField] private ListContainer categoryList;
    [SerializeField] private ListContainer cardList;
    [SerializeField] private Button backButton;

    private BookingDataManager Booking => DataManager.Booking;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (categoryList != null)
        {
            categoryList.Init(Booking.CategoryFiltersAsObject);
        }

        if (cardList != null)
        {
            cardList.Init(Booking.FilteredBookingsAsObject);
        }

        if (categoryList != null)
        {
            AddToDispose(UIManager.SubscribeToView(categoryList, (ToggleButtonModel data) =>
            {
                if (System.Enum.TryParse<BookingCategoryType>(data.name, out var categoryType))
                {
                    Booking.SelectCategory(categoryType);
                }
            }));
        }

        if (cardList != null)
        {
            AddToDispose(UIManager.SubscribeToView(cardList, (BookingModel booking) =>
            {
                if (booking != null)
                {
                }
            }));

            AddToDispose(UIManager.SubscribeToView(cardList, (int bookingId) =>
            {
                var booking = Booking.AllBookings.FirstOrDefault(b => b.id == bookingId);
                if (booking != null)
                {
                    DataManager.BookingConfirm.InitializeForBooking(booking);
                    ScreenManager?.Show(Screens.BookingConfirmScreen);
                }
            }));
        }

        if (backButton != null)
        {
            AddToDispose(backButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.HomeScreen);
                }));
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        
        if (categoryList != null)
        {
            categoryList.Init(Booking.CategoryFiltersAsObject);
        }

        if (cardList != null)
        {
            cardList.Init(Booking.FilteredBookingsAsObject);
        }
    }
}
