using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class BookingDataManager : IDataManager
{
    private readonly List<BookingModel> _allBookings;

    // Основные реактивные коллекции
    public ReactiveCollection<BookingModel> UpcomingBookings { get; } = new ReactiveCollection<BookingModel>();
    public ReactiveCollection<BookingModel> PastBookings { get; } = new ReactiveCollection<BookingModel>();
    public ReactiveCollection<BookingModel> AllBookings { get; } = new ReactiveCollection<BookingModel>();

    // Текущий черновик
    public ReactiveProperty<BookingModel> CurrentDraft { get; } = new ReactiveProperty<BookingModel>(null);

    // Выборы для текущего букинга
    public ReactiveProperty<StadiumModel> SelectedStadium { get; } = new ReactiveProperty<StadiumModel>(null);
    public ReactiveProperty<string> SelectedDateTimeIso { get; } = new ReactiveProperty<string>(null);
    public ReactiveProperty<PitchSize> SelectedPitchSize { get; } = new ReactiveProperty<PitchSize>(PitchSize.Size7x7);
    public ReactiveProperty<MatchDuration> SelectedDuration { get; } = new ReactiveProperty<MatchDuration>(MatchDuration.Min90);

    public ReactiveProperty<List<BookingExtra>> SelectedExtras { get; } = new ReactiveProperty<List<BookingExtra>>(new List<BookingExtra>());

    public ReactiveProperty<DateTime?> SelectedDate { get; } = new ReactiveProperty<DateTime?>(null);
    public ReactiveProperty<string> SelectedTime { get; } = new ReactiveProperty<string>(null);

    public ReactiveCollection<ToggleButtonModel> MorningTimes { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveCollection<ToggleButtonModel> AfternoonTimes { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveCollection<ToggleButtonModel> EveningTimes { get; } = new ReactiveCollection<ToggleButtonModel>();

    private readonly ReactiveCollection<object> _morningTimesAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _afternoonTimesAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _eveningTimesAsObject = new ReactiveCollection<object>();

    public ReactiveCollection<object> MorningTimesAsObject => _morningTimesAsObject;
    public ReactiveCollection<object> AfternoonTimesAsObject => _afternoonTimesAsObject;
    public ReactiveCollection<object> EveningTimesAsObject => _eveningTimesAsObject;

    private readonly Dictionary<ExtraType, int> _maxQuantity = new Dictionary<ExtraType, int>
    {
        { ExtraType.Ball, 3 },
        { ExtraType.WaterPack, 30 },
        { ExtraType.Bibs, 2 },        
        { ExtraType.Referee, 1 }
    };

    private readonly Dictionary<ExtraType, float> _pricePerUnit = new Dictionary<ExtraType, float>
    {
        { ExtraType.Ball, 15f },
        { ExtraType.WaterPack, 5f },  
        { ExtraType.Bibs, 20f },
        { ExtraType.Referee, 50f }
    };

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public BookingDataManager(AppModel appModel)
    {
        if (appModel.bookings == null || appModel.bookings.Count == 0)
        {
            appModel.bookings = new List<BookingModel>();
        }

        _allBookings = appModel.bookings;

        SyncAllBookingsToReactive();
        UpdateFilteredBookings();

        SelectedDuration.Subscribe(_ => RecalculateTotalCost()).AddTo(_disposables);
        SelectedExtras.Subscribe(_ => RecalculateTotalCost()).AddTo(_disposables);
        SelectedPitchSize.Subscribe(_ => RecalculateTotalCost()).AddTo(_disposables);

        BindTimeCollections();
        InitializeTimeSlots();
        new Log($"{MorningTimes.Count}", "BookingDataManager");
    }
    private void SyncAllBookingsToReactive()
    {
        AllBookings.Clear();
        foreach (var booking in _allBookings)
        {
            AllBookings.Add(booking);
        }
    }


    public void StartNewBooking(StadiumModel stadium)
    {
        SelectedStadium.Value = stadium;
        SelectedPitchSize.Value = PitchSize.Size7x7;
        SelectedDuration.Value = MatchDuration.Min90;
        SelectedDateTimeIso.Value = null;
        SelectedDate.Value = null;
        SelectedTime.Value = null;
        SelectedExtras.Value = new List<BookingExtra>();
        DeselectAllTimes();

        CurrentDraft.Value = new BookingModel
        {
            stadiumId = stadium.id,
            status = BookingStatus.Draft,
            totalCost = 0f,
            extras = new List<BookingExtra>()
        };
    }

    public void SetDateTime(string dateTimeIso)
    {
        SelectedDateTimeIso.Value = dateTimeIso;
        if (CurrentDraft.Value != null)
        {
            CurrentDraft.Value.dateTimeIso = dateTimeIso;
        }
    }

    public void SetSelectedDate(DateTime? date)
    {
        SelectedDate.Value = date;
        if (date.HasValue && !string.IsNullOrEmpty(SelectedTime.Value))
        {
            CombineDateAndTime();
        }
    }

    public void SetSelectedTime(string time)
    {
        SelectedTime.Value = time;
        if (SelectedDate.Value.HasValue && !string.IsNullOrEmpty(time))
        {
            CombineDateAndTime();
        }
    }

    public void SelectTimeSlot(ToggleButtonModel timeModel, ReactiveCollection<ToggleButtonModel> sourceCollection)
    {
        var isCurrentlySelected = timeModel.selected;
        DeselectAllTimes();

        if (!isCurrentlySelected)
        {
            var index = sourceCollection.IndexOf(timeModel);
            if (index >= 0)
            {
                sourceCollection[index] = new ToggleButtonModel
                {
                    name = timeModel.name,
                    selected = true
                };
                SetSelectedTime(timeModel.name);
            }
        }
        else
        {
            SetSelectedTime(null);
        }
    }

    private void CombineDateAndTime()
    {
        if (!SelectedDate.Value.HasValue || string.IsNullOrEmpty(SelectedTime.Value))
            return;

        var date = SelectedDate.Value.Value;
        var timeParts = SelectedTime.Value.Split(':');
        if (timeParts.Length == 2 && int.TryParse(timeParts[0], out int hours) && int.TryParse(timeParts[1], out int minutes))
        {
            var dateTime = new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);
            var dateTimeIso = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            SetDateTime(dateTimeIso);
        }
    }

    private void InitializeTimeSlots()
    {
        var morningTimes = new[] { "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00" };
        var afternoonTimes = new[] { "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00" };
        var eveningTimes = new[] { "18:00", "19:00", "20:00", "21:00", "22:00", "23:00" };

        foreach (var time in morningTimes)
        {
            MorningTimes.Add(new ToggleButtonModel { name = time, selected = false });
        }

        foreach (var time in afternoonTimes)
        {
            AfternoonTimes.Add(new ToggleButtonModel { name = time, selected = false });
        }

        foreach (var time in eveningTimes)
        {
            EveningTimes.Add(new ToggleButtonModel { name = time, selected = false });
        }
    }

    private void BindTimeCollections()
    {
        BindMirror(MorningTimes, _morningTimesAsObject);
        BindMirror(AfternoonTimes, _afternoonTimesAsObject);
        BindMirror(EveningTimes, _eveningTimesAsObject);
    }

    private void BindMirror<T>(ReactiveCollection<T> source, ReactiveCollection<object> mirror)
    {
        source.ObserveAdd().Subscribe(e => mirror.Insert(e.Index, e.Value)).AddTo(_disposables);
        source.ObserveRemove().Subscribe(e => mirror.RemoveAt(e.Index)).AddTo(_disposables);
        source.ObserveReplace().Subscribe(e => mirror[e.Index] = e.NewValue).AddTo(_disposables);
        source.ObserveMove().Subscribe(e => mirror.Move(e.OldIndex, e.NewIndex)).AddTo(_disposables);
        source.ObserveReset().Subscribe(_ =>
        {
            mirror.Clear();
            foreach (var item in source) mirror.Add(item);
        }).AddTo(_disposables);
    }

    private void DeselectAllTimes()
    {
        for (int i = 0; i < MorningTimes.Count; i++)
        {
            if (MorningTimes[i].selected)
            {
                MorningTimes[i] = new ToggleButtonModel
                {
                    name = MorningTimes[i].name,
                    selected = false
                };
            }
        }

        for (int i = 0; i < AfternoonTimes.Count; i++)
        {
            if (AfternoonTimes[i].selected)
            {
                AfternoonTimes[i] = new ToggleButtonModel
                {
                    name = AfternoonTimes[i].name,
                    selected = false
                };
            }
        }

        for (int i = 0; i < EveningTimes.Count; i++)
        {
            if (EveningTimes[i].selected)
            {
                EveningTimes[i] = new ToggleButtonModel
                {
                    name = EveningTimes[i].name,
                    selected = false
                };
            }
        }
    }

    public void SetPitchSize(PitchSize size)
    {
        SelectedPitchSize.Value = size;
        if (CurrentDraft.Value != null)
        {
            CurrentDraft.Value.pitchSize = size;
        }
    }

    public void SetDuration(MatchDuration duration)
    {
        SelectedDuration.Value = duration;
        if (CurrentDraft.Value != null)
        {
            CurrentDraft.Value.duration = duration;
        }
    }

    /// <summary>
    /// Установка количества для конкретной экстра (0 = убрать)
    /// </summary>
    public void SetExtraQuantity(ExtraType type, int quantity)
    {
        if (quantity < 0) return;

        int max = _maxQuantity.TryGetValue(type, out var m) ? m : 10;
        quantity = Mathf.Clamp(quantity, 0, max);

        var list = new List<BookingExtra>(SelectedExtras.Value);
        var existing = list.Find(e => e.type == type);

        if (quantity == 0)
        {
            if (existing != null) list.Remove(existing);
        }
        else
        {
            float price = _pricePerUnit.TryGetValue(type, out var p) ? p : 10f;

            if (existing != null)
            {
                existing.quantity = quantity;
                existing.pricePerUnit = price;
            }
            else
            {
                list.Add(new BookingExtra(type, quantity, price));
            }
        }

        SelectedExtras.Value = list;

        if (CurrentDraft.Value != null)
        {
            CurrentDraft.Value.extras = list;
        }
    }

    /// <summary>
    /// Удобные методы инкремент/декремент (для UI кнопок + / -)
    /// </summary>
    public void IncrementExtra(ExtraType type) => AdjustExtra(type, 1);
    public void DecrementExtra(ExtraType type) => AdjustExtra(type, -1);

    private void AdjustExtra(ExtraType type, int delta)
    {
        var current = SelectedExtras.Value.Find(e => e.type == type);
        int newQuantity = (current?.quantity ?? 0) + delta;
        SetExtraQuantity(type, newQuantity);
    }

    /// <summary>
    /// Пересчёт общей стоимости
    /// </summary>
    private void RecalculateTotalCost()
    {
        if (CurrentDraft.Value == null || SelectedStadium.Value == null) return;

        var stadium = SelectedStadium.Value;
        float hours = (int)SelectedDuration.Value / 60f;
        float baseCost = stadium.basePricePerHour * hours;

        // Можно добавить зависимость цены от pitchSize (например, больший размер — дороже)
        // float sizeMultiplier = SelectedPitchSize.Value == PitchSize.Size11x11 ? 1.5f : 1f;
        // baseCost *= sizeMultiplier;

        float extrasCost = 0f;
        foreach (var extra in SelectedExtras.Value)
        {
            extrasCost += extra.GetTotalPrice();
        }

        float total = baseCost + extrasCost;
        CurrentDraft.Value.totalCost = total;
    }

    public void ConfirmBooking()
    {
        if (CurrentDraft.Value == null) return;

        CurrentDraft.Value.status = BookingStatus.Confirmed;
        CurrentDraft.Value.qrPayload = JsonUtility.ToJson(CurrentDraft.Value);

        _allBookings.Add(CurrentDraft.Value);
        AllBookings.Add(CurrentDraft.Value);
        UpdateFilteredBookings();

        CurrentDraft.Value = null;
        SelectedStadium.Value = null;
        SelectedDateTimeIso.Value = null;
        SelectedDate.Value = null;
        SelectedTime.Value = null;
        SelectedExtras.Value = new List<BookingExtra>();
        DeselectAllTimes();
    }

    public void CancelDraft()
    {
        CurrentDraft.Value = null;
        SelectedStadium.Value = null;
        SelectedDateTimeIso.Value = null;
        SelectedDate.Value = null;
        SelectedTime.Value = null;
        SelectedExtras.Value = new List<BookingExtra>();
        DeselectAllTimes();
    }

    public void CancelBooking(int bookingId)
    {
        var booking = _allBookings.FirstOrDefault(b => b.id == bookingId);
        if (booking != null)
        {
            booking.status = BookingStatus.Canceled;
            UpdateFilteredBookings();
        }
    }

    public void UpdateFilteredBookings()
    {
        var now = DateTime.UtcNow;

        UpcomingBookings.Clear();
        PastBookings.Clear();

        var upcoming = _allBookings
            .Where(b => b.status != BookingStatus.Canceled && b.status != BookingStatus.Finished)
            .Where(b => DateTime.TryParse(b.dateTimeIso, out var date) && date > now)
            .OrderBy(b => DateTime.Parse(b.dateTimeIso))
            .ToList();

        var past = _allBookings
            .Where(b => b.status == BookingStatus.Canceled || b.status == BookingStatus.Finished ||
                        (DateTime.TryParse(b.dateTimeIso, out var date) && date <= now))
            .OrderByDescending(b => DateTime.Parse(b.dateTimeIso))
            .ToList();

        foreach (var b in upcoming) UpcomingBookings.Add(b);
        foreach (var b in past) PastBookings.Add(b);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}