using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ExtraAddCard : UIView<BookingExtraModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Image icon;
    [SerializeField] private CounterView counterView;
    [SerializeField] private SimpleToggle toggleView;

    private BookingDataManager Booking => DataManager.Booking;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (counterView != null)
        {
            UIManager.SubscribeToView(counterView, (int quantity) =>
            {
                if (DataProperty.Value != null)
                {
                    var maxQuantity = DataProperty.Value.maxQuantity;
                    int clampedQuantity = Mathf.Clamp(quantity, 0, maxQuantity);

                    if (clampedQuantity != quantity)
                    {
                        counterView.Init(clampedQuantity);
                    }

                    Booking.SetExtraQuantity(DataProperty.Value.type, clampedQuantity);
                }
            }).AddTo(this);
        }

        if (toggleView != null)
        {
            UIManager.SubscribeToView(toggleView, (bool isOn) =>
            {
                if (DataProperty.Value != null)
                {
                    Booking.SetExtraQuantity(DataProperty.Value.type, isOn ? 1 : 0);
                }
            }).AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (nameText != null)
        {
            nameText.text = data.name;
        }

        if (priceText != null)
        {
            priceText.text = $"${data.pricePerUnit:F2}";
        }

        if (icon != null && data.icon != null)
        {
            icon.sprite = data.icon;
        }
        if (data.maxQuantity == 1)
        {
            if (toggleView != null)
            {
                toggleView.gameObject.SetActive(true);
                toggleView.Init(data.currentQuantity > 0);
            }
            if (counterView != null)
            {
                counterView.gameObject.SetActive(false);
            }
           
        }
        else
        {
            if (toggleView != null)
            {
                toggleView.gameObject.SetActive(false);
            }
            if (counterView != null)
            {
                counterView.gameObject.SetActive(true);
                counterView.Init(data.currentQuantity);
            }
            
        }



    }
}
