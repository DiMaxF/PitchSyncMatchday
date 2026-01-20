using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class BookingCard : UIView<BookingModel>
{
    [SerializeField] private Button showQr;
    [SerializeField] private Button action;
    [SerializeField] private Image background;
    [SerializeField] private StatusBooking statusText;
    [SerializeField] private Text pitchNameText;
    [SerializeField] private Text dateTimeText;
    [SerializeField] private Text pitchInfoText;

    protected override void Subscribe()
    {
        base.Subscribe();
        
        if (showQr != null)
        {
            showQr.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (DataProperty.Value != null)
                    {
                        UIManager.TriggerAction(this, DataProperty.Value.id);
                    }
                })
                .AddTo(this);
        }

        if (action != null)
        {
            action.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Trigger(DataProperty.Value);
                })
                .AddTo(this);
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        if (statusText != null)
        {
            statusText.Init(data);
        }

        var stadium = GetStadiumById(data.stadiumId);

        if (background != null)
        {
            background.sprite = FileUtils.LoadImageAsSprite(stadium.photoPath);
            }
        if (pitchNameText != null)
        {
            pitchNameText.text = stadium != null ? stadium.name : string.Empty;
        }

        if (DateTime.TryParse(data.dateTimeIso, out var dateTime))
        {
            if (dateTimeText != null)
            {
                dateTimeText.text = $"{dateTime.ToString("dd MMMM yyyy")} � {dateTime.ToString("HH:mm")}";
            }
        }


        if (pitchInfoText != null)
        {
            pitchInfoText.text = $"{data.pitchSize} � {((int)data.duration) / 60} min";
        }
    }

    private StadiumModel GetStadiumById(int stadiumId)
    {
        return DataManager.PitchFinder.GetStadiumById(stadiumId);
    }
}
