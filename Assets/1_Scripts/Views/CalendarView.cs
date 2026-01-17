using System;
using UnityEngine;
using UnityEngine.UI;

public class CalendarView : UIView<DateTime>
{
    [SerializeField] private CalendarManager manager;
    [SerializeField] private Text selectedText;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (manager != null)
        {
            manager.OnSelect += OnDateSelected;
        }
    }

    public override void Init(DateTime initialData = default)
    {
        if (initialData == default)
        {
            initialData = DateTime.Now.AddDays(1);
        }

        base.Init(initialData);

        if (manager != null)
        {
            manager.UpdateCalenderWithSelectedDate(initialData);
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();

        if (manager != null && DataProperty.Value != default(DateTime))
        {
            var date = DataProperty.Value;
            if (date.Year >= 1 && date.Year <= 9999 && date.Month >= 1 && date.Month <= 12)
            {
                manager.UpdateCalenderWithSelectedDate(date);
            }
        }
    }

    private void OnDateSelected(DateTime selectedDate)
    {
        DataProperty.Value = selectedDate;

        Trigger(selectedDate);
    }

    protected override void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnSelect -= OnDateSelected;
        }

        base.OnDestroy();
    }
}