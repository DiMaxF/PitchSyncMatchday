using UnityEngine;
using UnityEngine.UI;

public class ShortExtraCard : UIView<BookingExtraModel>
{
    [SerializeField] private Text valueText;
    [SerializeField] private Image icon;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (valueText != null)
        {
            valueText.text = $"{data.currentQuantity}x {name}";
        }

        if (icon != null && data.icon != null)
        {
            icon.sprite = data.icon;
        }
    }
}
