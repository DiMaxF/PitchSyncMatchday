using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BookingConfirmScreen : UIScreen
{
    [SerializeField] private Image qrImage;
    [SerializeField] ShortBookingCard shortBookingCard;
    [SerializeField] Text totalPrice;
    [SerializeField] Text durationText;
    [SerializeField] Text pitchSizeText;
    [SerializeField] ListContainer extrasSummary;
    [SerializeField] Button saveButton;
    [SerializeField] Button matchCenterButton;

    private BookingConfirmDataManager BookingConfirm => DataManager.BookingConfirm;

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (qrImage != null)
        {
            AddToDispose(BookingConfirm.QRCodeTexture.Subscribe(texture =>
            {
                if (texture != null)
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    qrImage.sprite = sprite;
                }
                else
                {
                    qrImage.sprite = null;
                }
            }));
        }

        if (shortBookingCard != null)
        {
            AddToDispose(BookingConfirm.ConfirmedBooking.Subscribe(booking =>
            {
                if (booking != null)
                {
                    shortBookingCard.Init(booking);
                }
            }));
        }

        if (totalPrice != null)
        {
            AddToDispose(BookingConfirm.TotalPrice.Subscribe(price =>
            {
                totalPrice.text = $"${price:F2}";
            }));
        }

        if (durationText != null)
        {
            AddToDispose(BookingConfirm.DurationText.Subscribe(text =>
            {
                durationText.text = text;
            }));
        }

        if (pitchSizeText != null)
        {
            AddToDispose(BookingConfirm.PitchSizeText.Subscribe(text =>
            {
                pitchSizeText.text = text;
            }));
        }

        if (extrasSummary != null)
        {
            extrasSummary.Init(BookingConfirm.ExtrasSummaryAsObject);
        }

        if (saveButton != null)
        {
            AddToDispose(saveButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnSaveClicked();
                    ScreenManager.Show(Screens.MyBookingScreen);
                }));
        }

        if (matchCenterButton != null)
        {
            AddToDispose(matchCenterButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ScreenManager?.Show(Screens.HomeScreen);
                }));
        }
    }

    private void OnSaveClicked()
    {
        if (BookingConfirm.QRCodeTexture.Value != null)
        {
            GalleryController.SaveImageToGallery(BookingConfirm.QRCodeTexture.Value);
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        
        if (totalPrice != null && BookingConfirm.TotalPrice.Value > 0)
        {
            totalPrice.text = $"${BookingConfirm.TotalPrice.Value:F2}";
        }

        if (durationText != null)
        {
            durationText.text = BookingConfirm.DurationText.Value;
        }

        if (pitchSizeText != null)
        {
            pitchSizeText.text = BookingConfirm.PitchSizeText.Value;
        }

        if (shortBookingCard != null && BookingConfirm.ConfirmedBooking.Value != null)
        {
            shortBookingCard.Init(BookingConfirm.ConfirmedBooking.Value);
        }

        if (qrImage != null && BookingConfirm.QRCodeTexture.Value != null)
        {
            var texture = BookingConfirm.QRCodeTexture.Value;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            qrImage.sprite = sprite;
        }
    }
}
