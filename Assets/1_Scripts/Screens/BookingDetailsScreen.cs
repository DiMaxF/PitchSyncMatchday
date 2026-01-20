using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BookingDetailsScreen : UIScreen
{
    [SerializeField] private Text pitchSize;
    [SerializeField] private Text location;
    [SerializeField] private Text startTime;
    [SerializeField] private Text endTime;
    [SerializeField] private StatusBooking statusBooking;
    [SerializeField] private ShortBookingCard shortBooking;
    [SerializeField] private ListContainer extras;
    [SerializeField] private Text basePrice;
    [SerializeField] private Text extrasPrice;
    [SerializeField] private Text totalPrice;
    [SerializeField] private Button checkInQrButton;
    [SerializeField] private QrPanel qrPanel;
    [SerializeField] private Button showCheckInQrButton;
    [SerializeField] private Button showMatchCenter;
    [SerializeField] private Button closeButton;

    private BookingConfirmDataManager BookingConfirm => DataManager.BookingConfirm;
    private MatchCenterDataManager MatchCenter => DataManager.MatchCenter;
    private BookingModel _currentBooking;
    private IDisposable _qrCodeSubscription;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (extras != null)
        {
            extras.Init(BookingConfirm.ExtrasSummaryAsObject);
        }

        AddToDispose(BookingConfirm.ConfirmedBooking.Subscribe(booking =>
        {
            _currentBooking = booking;
            UpdateBookingInfo(booking);
        }));

        if (checkInQrButton != null && qrPanel != null)
        {
            AddToDispose(checkInQrButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (_currentBooking != null)
                    {
                        _qrCodeSubscription?.Dispose();
                        BookingConfirm.InitializeForBooking(_currentBooking);
                        
                        _qrCodeSubscription = BookingConfirm.QRCodeTexture.Subscribe(texture =>
                        {
                            if (texture != null)
                            {
                                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                                qrPanel.Init(sprite);
                                qrPanel.gameObject.SetActive(true);
                                qrPanel.Show();
                            }
                        });
                        AddToDispose(_qrCodeSubscription);
                    }
                }));
        }

        if (showCheckInQrButton != null)
        {
            AddToDispose(showCheckInQrButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (_currentBooking != null)
                    {
                        BookingConfirm.InitializeForBooking(_currentBooking);
                        ScreenManager?.Show(Screens.BookingConfirmScreen);
                    }
                }));
        }

        if (showMatchCenter != null)
        {
            AddToDispose(showMatchCenter.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (_currentBooking != null)
                    {
                        MatchCenter.InitializeFromBooking(_currentBooking.id);
                        ScreenManager?.Show(Screens.MatchCenterScreen);
                    }
                }));
        }


        if (closeButton != null)
        {
            AddToDispose(closeButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Back();
                }));
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        UpdateBookingInfo(_currentBooking);
    }

    private void UpdateBookingInfo(BookingModel booking)
    {
        if (booking == null) return;

        if (shortBooking != null)
        {
            shortBooking.Init(booking);
        }

        var stadium = DataManager.PitchFinder.GetStadiumById(booking.stadiumId);

        if (pitchSize != null)
        {
            pitchSize.text = booking.pitchSize.ToString();
        }

        if (location != null)
        {
            location.text = stadium != null ? stadium.address : string.Empty;
        }

        if (statusBooking != null)
        {
            statusBooking.Init(booking.status);
        }

        if (DateTime.TryParse(booking.dateTimeIso, out var dateTime))
        {
            if (startTime != null)
            {
                startTime.text = dateTime.ToString("HH:mm");
            }
            if (endTime != null)
            {
                var durationMinutes = (int)booking.duration;
                endTime.text = dateTime.AddMinutes(durationMinutes).ToString("HH:mm");
            }
        }

        float hours = (int)booking.duration / 60f;
        float baseCost = stadium != null ? stadium.basePricePerHour * hours : 0f;
        float extrasCost = 0f;
        if (booking.extras != null)
        {
            extrasCost = booking.extras.Sum(e => e.GetTotalPrice());
        }
        float total = baseCost + extrasCost;

        if (basePrice != null)
        {
            basePrice.text = $"${baseCost:F2}";
        }
        if (extrasPrice != null)
        {
            extrasPrice.text = $"${extrasCost:F2}";
        }
        if (totalPrice != null)
        {
            totalPrice.text = $"${total:F2}";
        }
    }
}
