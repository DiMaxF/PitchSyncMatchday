using UnityEngine;
using UnityEngine.UI;

public class PlayerIcon : UIView<SquadPlayerModel>
{
    [SerializeField] private Text countText;
    [SerializeField] private Image squadColor;

    private RectTransform _rect;

    protected override void Awake()
    {
        base.Awake();
        _rect = GetComponent<RectTransform>();
    }

    public void ApplyLayout(Vector2 anchoredPos, float size)
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();
        if (_rect == null) return;

        _rect.anchoredPosition = anchoredPos;
        _rect.sizeDelta = new Vector2(size, size);
    }

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
