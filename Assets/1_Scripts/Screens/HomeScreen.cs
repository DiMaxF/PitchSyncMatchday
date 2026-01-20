using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class HomeScreen : UIScreen
{
    [SerializeField] private Button pitchFinderButton;
    [SerializeField] private Button myBookingButton;
    [SerializeField] private Button walletButton;
    [SerializeField] private Button lineupButton;
    [SerializeField] private Button notificationsButton;
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
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        
        if (upcomingEvents != null)
        {
            upcomingEvents.Init(Booking.UpcomingBookingsAsObject);
        }
    }
}
