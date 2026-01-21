using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using UniRx.Triggers;

public class HomeScreen : UIScreen
{
    [SerializeField] private Button pitchFinderButton;
    [SerializeField] private Button myBookingButton;
    [SerializeField] private Button myBookingButton2;
    [SerializeField] private Button walletButton;
    [SerializeField] private Button lineupButton;
    [SerializeField] private Button notificationsButton;
    [SerializeField] private Button bookPitchButton;
    [SerializeField] private QrPanel qrPanel;
    [SerializeField] private ListContainer upcomingEvents;

    private BookingDataManager Booking => DataManager.Booking;
    private BookingConfirmDataManager BookingConfirm => DataManager.BookingConfirm;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();
        
        pitchFinderButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.PitchFinderScreen);
            })
            .AddTo(this);

        myBookingButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.MyBookingScreen);
            })
            .AddTo(this);
        myBookingButton2.OnClickAsObservable()
.Subscribe(_ =>
{
    ScreenManager.Show(Screens.MyBookingScreen);
})
.AddTo(this);
        walletButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.WalletScreen);
            })
            .AddTo(this);
            
        lineupButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.LineupScreen);
            })
            .AddTo(this);


        notificationsButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.NotificationsScreen);
            })
            .AddTo(this);

        if (upcomingEvents != null)
        {
            upcomingEvents.Init(Booking.UpcomingBookingsAsObject);
            
            AddToDispose(UIManager.SubscribeToView(upcomingEvents, (BookingModel booking) =>
            {
                if (booking != null)
                {
                    BookingConfirm.InitializeForBooking(booking);
                    ScreenManager?.Show(Screens.BookingDetailsScreen);
                }
            }));

            AddToDispose(UIManager.SubscribeToView(upcomingEvents, (int bookingId) =>
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

        if (bookPitchButton != null && Booking.UpcomingBookingsAsObject != null)
        {
            UpdateBookPitchButtonVisibility();
            
            AddToDispose(Booking.UpcomingBookingsAsObject.ObserveCountChanged()
                .Subscribe(_ => UpdateBookPitchButtonVisibility()));

            bookPitchButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ScreenManager.Show(Screens.PitchFinderScreen);
            })
            .AddTo(this);
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        
        if (upcomingEvents != null)
        {
            upcomingEvents.Init(Booking.UpcomingBookingsAsObject);
        }
        
        UpdateBookPitchButtonVisibility();
    }

    private void UpdateBookPitchButtonVisibility()
    {
        if (bookPitchButton == null) return;
        if (Booking.UpcomingBookingsAsObject == null) return;

        int count = Booking.UpcomingBookingsAsObject.Count;
        if (count == 0)
        {
            bookPitchButton.gameObject.SetActive(true);
        }
        else
        {
            bookPitchButton.gameObject.SetActive(false);
        }
    }
}
