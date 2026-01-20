using UnityEngine;
using UnityEngine.UI;

public class ParticipantCard : UIView<ParticipantModel>
{
    [SerializeField] private Text valueText;
    [SerializeField] private Text nameText;
    [SerializeField] private Image icon;

    private LineupDataManager Lineup => DataManager.Lineup;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (valueText != null)
        {
            valueText.text = $"Paid ${data.paidAmount:F2}";
        }
        
        if (nameText != null)
        {
            nameText.text = data.name;
        }

        if (icon != null)
        {
            Sprite avatarSprite = null;
            
            if (data.playerId.HasValue)
            {
                var player = Lineup.GetPlayerById(data.playerId.Value);
                if (player != null)
                {
                    avatarSprite = player.avatar;
                }
            }
            
            if (avatarSprite == null)
            {
                avatarSprite = Resources.Load<Sprite>("ic_profile_0");
            }
            
            icon.sprite = avatarSprite;
        }
    }
}
