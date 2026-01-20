using UnityEngine;
using UnityEngine.UI;

public class NoteItemView : UIView<string>
{
    [SerializeField] private Text value;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (value != null)
        {
            value.text = data;
        }
    }
}

