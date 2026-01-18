using UnityEngine;
using UnityEngine.UI;

public class ShortBookingCard : UIView<BookingModel>
{
    [SerializeField] private Image imagePitch;
    [SerializeField] private Text timeText;
    [SerializeField] private Text dateText;
    [SerializeField] private Text nameText;
}
