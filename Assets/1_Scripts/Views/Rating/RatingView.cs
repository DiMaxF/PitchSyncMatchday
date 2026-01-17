using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
public class RatingView : UIView<float>
{
    [SerializeField] private ListContainer stars;
    [SerializeField] private Text valueText;

    private ReactiveCollection<object> _starsCollection;

    public override void UpdateUI()
    {
        var data = DataProperty.Value;

        if (stars != null)
        {
            var starsList = GetStars(data);

            if (_starsCollection == null)
            {
                _starsCollection = starsList.Select(s => (object)s).ToReactiveCollection();
                stars.Init(_starsCollection);
            }
            else
            {
                _starsCollection.Clear();
                foreach (var star in starsList)
                {
                    _starsCollection.Add(star);
                }
            }
        }

        if (valueText != null) valueText.text = data.ToString("0.0");
    }

    private List<bool> GetStars(float rating, int total = 5)
    {
        var result = new List<bool>();
        for (int i = 1; i <= total; i++)
        {
            result.Add(rating >= i);
        }
        return result;
    }
}
