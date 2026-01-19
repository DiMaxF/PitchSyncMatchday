using UnityEngine;
using UnityEngine.UI;

public class PlayerIcon : UIView<SquadPlayerModel>
{
    [SerializeField] private Text countText;
    [SerializeField] private Image squadColor;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (countText != null)
        {
            countText.text = data.squadNumber.ToString();
        }

        if (squadColor != null)
        {
            squadColor.sprite = data.teamIcon;
        }
    }

}
