using UnityEngine;
using UnityEngine.UI;

public class ParticipantCard : UIView<ParticipantModel>
{
    [SerializeField] private Text valueText;
    [SerializeField] private Text nameText;
    [SerializeField] private Image icon;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (valueText != null)
        {
            valueText.text = $"Paid ${data.paidAmount}";
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
