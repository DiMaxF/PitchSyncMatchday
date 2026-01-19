using System;
using UnityEngine;
using UnityEngine.UI;

public class ShortBookingCard : UIView<BookingModel>
{
    [SerializeField] private Image imagePitch;
    [SerializeField] private Text timeText;
    [SerializeField] private Text dateText;
    [SerializeField] private Text nameText;

    public override void UpdateUI()
    {
        base.UpdateUI();
        var data = DataProperty.Value;
        if (data == null) return;

        var stadium = GetStadiumById(data.stadiumId);
        if (nameText != null)
        {
            nameText.text = stadium != null ? stadium.name : string.Empty;
        }

        if (DateTime.TryParse(data.dateTimeIso, out var dateTime))
        {
            if (dateText != null)
            {
                dateText.text = dateTime.ToString("ddd, MMM dd");
            }

            if (timeText != null)
            {
                var startTime = dateTime.ToString("HH:mm");
                var durationMinutes = (int)data.duration;
                var endTime = dateTime.AddMinutes(durationMinutes).ToString("HH:mm");
                timeText.text = $"{startTime} – {endTime}";
            }
        }
    }

    private StadiumModel GetStadiumById(int stadiumId)
    {
        return DataManager.PitchFinder.GetStadiumById(stadiumId);
    }
}
