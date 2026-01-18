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


}
