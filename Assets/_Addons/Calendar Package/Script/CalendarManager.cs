using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarManager : MonoBehaviour
{
    [SerializeField] Text MonthAndYear;
    [SerializeField] private GameObject[] days;
    private int showYear;
    private int showMonth;
    [SerializeField] Button left;
    [SerializeField] Button right;
    public static int currentDateSelected;

    public event Action<DateTime> OnSelect;


    private void OnEnable()
    {
        left.onClick.AddListener(Left);
        right.onClick.AddListener(Right);
    }

    private void OnDisable()
    {
        left.onClick.RemoveListener(Left);
        right.onClick.RemoveListener(Right);
    }

    private void Start()
    {
        UpdateCalender(DateTime.Now.Year, DateTime.Now.Month);
    }

    DateTime _selectedDate;

    public void UpdateCalenderWithSelectedDate(DateTime selectedDate)
    {
        _selectedDate = selectedDate;
        UpdateCalender(selectedDate.Year, selectedDate.Month, selectedDate);
    }

    public void UpdateCalender(int year, int month, DateTime? selectedDate = null)
    {
        showYear = year;
        showMonth = month;
        if (selectedDate == null && _selectedDate != null) selectedDate = _selectedDate;
        DateTime temp = new DateTime(year, month, 1);
        MonthAndYear.text = temp.ToString("MMMM") + " " + temp.ToString("yyyy");
        int startDay = GetMonthStartDay(temp.Year, temp.Month);
        int endDay = GetTotalNumberOfDays(temp.Year, temp.Month);
        int previousEndDate;

        for (int i = 0; i < days.Length; i++)
        {
            days[i].GetComponent<Day>().DayModeSet(0);
            days[i].GetComponent<Day>().dateNum = 0;
        }

        for (int w = 0; w < 6; w++)
        {
            for (int i = 0; i < 7; i++)
            {
                int currentField = (w * 7) + i;

                if (currentField < startDay || currentField - startDay >= endDay)
                {
                    days[currentField].GetComponent<Day>().DayModeSet(0);
                }
                else
                {
                    days[currentField].GetComponent<Day>().DayModeSet(2);
                }

                if (currentField >= startDay && currentField - startDay < endDay)
                {
                    var dateDay = new DateTime(showYear, showMonth, (currentField - startDay) + 1);
                    days[currentField].GetComponent<Day>().Init(dateDay, this);
                }
                else if (currentField < startDay)
                {
                    int prevYear = showYear;
                    int prevMonth = showMonth - 1;

                    if (prevMonth < 1)
                    {
                        prevMonth = 12;
                        prevYear = showYear - 1;
                    }

                    previousEndDate = GetTotalNumberOfDays(prevYear, prevMonth);

                    int sub = startDay - currentField;
                    if (sub > 0 && sub < 7)
                    {
                        int day = previousEndDate - (sub - 1);
                        if (day >= 1 && day <= previousEndDate)
                        {
                            var dateDay = new DateTime(prevYear, prevMonth, day);
                            days[currentField].GetComponent<Day>().dateNum = day;
                            days[currentField].GetComponent<Day>().Init(dateDay, this);
                        }
                    }
                }
                else if (currentField - startDay >= endDay)
                {
                    int nextYear = showYear;
                    int nextMonth = showMonth + 1;

                    if (nextMonth > 12)
                    {
                        nextMonth = 1;
                        nextYear = showYear + 1;
                    }

                    int sub = (currentField - startDay) - endDay + 1;
                    if (sub > 0 && sub < 15)
                    {
                        int maxDays = GetTotalNumberOfDays(nextYear, nextMonth);
                        if (sub <= maxDays)
                        {
                            var dateDay = new DateTime(nextYear, nextMonth, sub);
                            days[currentField].GetComponent<Day>().Init(dateDay, this);
                            days[currentField].GetComponent<Day>().dateNum = sub;
                        }
                    }
                }
            }
        }

        if (selectedDate.HasValue && selectedDate.Value.Year == year && selectedDate.Value.Month == month)
        {
            int dayIndex = (selectedDate.Value.Day - 1) + startDay;
            if (dayIndex >= 0 && dayIndex < days.Length)
            {
                days[dayIndex].GetComponent<Day>().DayModeSet(1);
                currentDateSelected = selectedDate.Value.Day;
            }
        }
    }

    public void Select(DateTime date)
    {
        UpdateCalenderWithSelectedDate(date);
        OnSelect?.Invoke(date);
    }

    private int GetMonthStartDay(int year, int month)
    {
        DateTime temp = new DateTime(year, month, 1);
        return (int)temp.DayOfWeek;
    }

    private int GetTotalNumberOfDays(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }

    private void Left()
    {
        if (showMonth != 1)
        {
            UpdateCalender(showYear, showMonth - 1);
        }
        else
        {
            UpdateCalender(showYear - 1, 12);
        }
    }

    private void Right()
    {
        if (showMonth != 12)
        {
            UpdateCalender(showYear, showMonth + 1);
        }
        else
        {
            UpdateCalender(showYear + 1, 1);
        }
    }
}