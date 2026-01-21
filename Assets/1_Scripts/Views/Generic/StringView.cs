using UnityEngine;
using UnityEngine.UI;

public class StringView : UIView<string>
{
    [SerializeField] private Text value;

    public override void UpdateUI()
    {
        base.UpdateUI();
        if (value != null) 
        {
            value.text = Data.Value;
        }
    }
}
