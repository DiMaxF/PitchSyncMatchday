using UnityEngine;
using UnityEngine.UI;

public class ProfileCard : UIView<ProfileInfo>
{
    [SerializeField] private Image userpic;
    [SerializeField] private Text nameText;
    [SerializeField] private Text emailText;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (userpic != null)
        {
            userpic.sprite = data.userpic;
        }

        if (nameText != null)
        {
            nameText.text = data.name;
        }

        if (emailText != null)
        {
            emailText.text = data.email;
        }
    }
}
