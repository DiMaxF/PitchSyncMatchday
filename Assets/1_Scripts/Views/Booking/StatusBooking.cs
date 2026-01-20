using UnityEngine;
using UnityEngine.UI;

public class StatusBooking : UIView<BookingStatus>
{
    [SerializeField] private Color green = Color.green;
    [SerializeField] private Color red = Color.red;
    [SerializeField] private Color yellow = Color.yellow;

    [SerializeField] private Text text;
    [SerializeField] private Image outline;


    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = Data.Value;
        switch (data) 
        {
            case BookingStatus.Draft:
                text.text = "Draft";
                text.color = yellow;
                outline.color = yellow;
                break;
            case BookingStatus.Finished:
                text.text = "Finished";
                text.color = red;
                outline.color = red;
                break;
            case BookingStatus.Confirmed:
                text.text = "Confirmed";
                text.color = green;
                outline.color = green;
                break;
        }
    }
}
