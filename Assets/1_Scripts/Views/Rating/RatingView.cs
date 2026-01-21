using UnityEngine;
using UnityEngine.UI;

public class RatingView : UIView<float>
{
    [SerializeField] private StarView[] stars;
    [SerializeField] private Text valueText;

    public override void UpdateUI()
    {
        var data = DataProperty.Value;

        if (stars != null)
        {
            new Log($"value {data}", "RatingView");
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    bool isActive = data >= (i);
                    new Log($"{i} {isActive}", $"Star{i}");
                    if(isActive)stars[i].SetShow();
                    else stars[i].SetHide();
                }
            }
        }

        if (valueText != null) valueText.text = data.ToString("0.0");
    }
}
