using UnityEngine;
using UnityEngine.UI;

public class MatchEventView : UIView<MatchEvent>
{
    [SerializeField] private Text value;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        string teamLabel = data.team == TeamSide.Green ? "Blue" : "Orange";
        string text;
        
        if (!string.IsNullOrEmpty(data.description) && data.description == "equalized")
        {
            text = $"{data.minute}' {teamLabel} {data.description}";
        }
        else
        {
            text = $"{data.minute}' {data.type} — {teamLabel}";
        }

        if (value != null)
        {
            value.text = text;
        }
    }
}

