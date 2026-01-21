using UnityEngine;
using UnityEngine.UI;

public class StatusMatch : UIView<MatchStatus>
{
    [SerializeField] private Color green = Color.green;
    [SerializeField] private Color red = Color.red;
    [SerializeField] private Color yellow = Color.yellow;

    [SerializeField] private Text text;
    [SerializeField] private Image outline;


    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = Data.Value;
        switch (data) 
        {
            case MatchStatus.Upcoming:
                text.text = "Upcoming";
                text.color = yellow;
                outline.color = yellow;
                break;
            case MatchStatus.Live:
                text.text = "Live";
                text.color = green;
                outline.color = green;
                break;
            case MatchStatus.Finished:
                text.text = "Finished";
                text.color = red;
                outline.color = red;
                break;
        }
    }
}

