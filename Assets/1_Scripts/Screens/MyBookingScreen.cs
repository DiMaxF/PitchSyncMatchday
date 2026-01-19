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
    [SerializeField] private QrPanel qrPanel;

    private BookingDataManager Booking => DataManager.Booking;
    private BookingConfirmDataManager BookingConfirm => DataManager.BookingConfirm;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (qrPanel != null)
        {
            qrPanel.gameObject.SetActive(false);
        }
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();
        //new Log($"{Booking.AllBookings.Count}", "MyBookingScreen");
        
        if (Booking.SelectedCategory.Value == null)
        {
            Booking.SelectCategory(BookingCategoryType.Upcoming);
        }
        
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
                    BookingConfirm.InitializeForBooking(booking);
                    
                    AddToDispose(BookingConfirm.QRCodeTexture.Subscribe(texture =>
                    {
                        if (texture != null && qrPanel != null)
                        {
                            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            qrPanel.Init(sprite);
                            qrPanel.gameObject.SetActive(true);
                            qrPanel.Show();
                        }
                    }));
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
