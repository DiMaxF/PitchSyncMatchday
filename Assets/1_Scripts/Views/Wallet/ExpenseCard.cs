using UnityEngine;
using UnityEngine.UI;

public class ExpenseCard : UIView<ExpenseModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text amountText;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (amountText != null)
        {
            amountText.text = $"${data.amount}";
        }
        if (nameText != null)
        {
            nameText.text = $"{data.name}";
        }

        /*if (icon != null && data.icon != null)
        {
            //icon.sprite = data.;
        }*/
    }
}
