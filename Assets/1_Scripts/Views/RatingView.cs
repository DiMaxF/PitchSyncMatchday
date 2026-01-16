using UnityEngine;
using UnityEngine.UI;

public class RatingView : UIView<float>
{
    [SerializeField] private ListContainer stars;
    [SerializeField] private Text valueText;

    public override void UpdateUI()
    {
        var data = DataProperty.Value;
        if (stars != null) 
        {
            var count = (int) data;
            stars.Init(count);
        }
        if(valueText != null) valueText.text = data.ToString("0.0");
    }
}
