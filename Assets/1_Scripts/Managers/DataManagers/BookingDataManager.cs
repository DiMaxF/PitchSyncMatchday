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
    
    public ReactiveProperty<BookingCategoryType?> SelectedCategory { get; } = new ReactiveProperty<BookingCategoryType?>(null);
    public ReactiveCollection<ToggleButtonModel> CategoryFilters { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveCollection<BookingModel> FilteredBookings { get; } = new ReactiveCollection<BookingModel>();
    
    private readonly ReactiveCollection<object> _categoryFiltersAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _filteredBookingsAsObject = new ReactiveCollection<object>();
    
    public ReactiveCollection<object> CategoryFiltersAsObject => _categoryFiltersAsObject;
    public ReactiveCollection<object> FilteredBookingsAsObject => _filteredBookingsAsObject;

    // Текущий черновик
    public ReactiveProperty<BookingModel> CurrentDraft { get; } = new ReactiveProperty<BookingModel>(null);

    // Выборы для текущего букинга
    public ReactiveProperty<StadiumModel> SelectedStadium { get; } = new ReactiveProperty<StadiumModel>(null);
    public ReactiveProperty<string> SelectedDateTimeIso { get; } = new ReactiveProperty<string>(null);
    public ReactiveProperty<PitchSize> SelectedPitchSize { get; } = new ReactiveProperty<PitchSize>(PitchSize.Size7x7);
    public ReactiveProperty<MatchDuration> SelectedDuration { get; } = new ReactiveProperty<MatchDuration>(MatchDuration.Min90);

    public ReactiveProperty<List<BookingExtra>> SelectedExtras { get; } = new ReactiveProperty<List<BookingExtra>>(new List<BookingExtra>());

    public ReactiveProperty<float> TotalCost { get; } = new ReactiveProperty<float>(0f);

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

    public ReactiveCollection<ToggleButtonModel> PitchSizes { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveCollection<ToggleButtonModel> Durations { get; } = new ReactiveCollection<ToggleButtonModel>();

    private readonly ReactiveCollection<object> _pitchSizesAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _durationsAsObject = new ReactiveCollection<object>();

    public ReactiveCollection<object> PitchSizesAsObject => _pitchSizesAsObject;
    public ReactiveCollection<object> DurationsAsObject => _durationsAsObject;

    public ReactiveCollection<BookingExtraModel> AvailableExtras { get; } = new ReactiveCollection<BookingExtraModel>();
    private readonly ReactiveCollection<object> _availableExtrasAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> AvailableExtrasAsObject => _availableExtrasAsObject;

    private Dictionary<ExtraType, BookingExtraConfig.ExtraConfigData> _extraConfigs;
    private AppConfig _config;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public BookingDataManager(AppModel appModel, AppConfig config)
    {
        _config = config;

        if (appModel.bookings == null || appModel.bookings.Count == 0)
        {
            appModel.bookings = new List<BookingModel>();
        }

        _allBookings = appModel.bookings;

        InitializeExtraConfigs(config);
        SyncAllBookingsToReactive();
        UpdateFilteredBookings();
        EnsureAllBookingsHaveAllExtras();

        SelectedDuration.Subscribe(_ => RecalculateTotalCost()).AddTo(_disposables);
        SelectedExtras.Subscribe(_ => RecalculateTotalCost()).AddTo(_disposables);
        SelectedPitchSize.Subscribe(_ => RecalculateTotalCost()).AddTo(_disposables);

        InitializeAvailableExtras();
        BindTimeCollections();
        InitializeTimeSlots();
        InitializePitchSizes();
        InitializeDurations();
        BindPitchSizeAndDurationCollections();
        InitializeCategoryFilters();
        BindCategoryFilters();
        UpdateFilteredBookingsByCategory();

        SelectedPitchSize.Subscribe(_ => UpdatePitchSizesSelection()).AddTo(_disposables);
        SelectedDuration.Subscribe(_ => UpdateDurationsSelection()).AddTo(_disposables);
        SelectedCategory.Subscribe(_ => UpdateFilteredBookingsByCategory()).AddTo(_disposables);
        AllBookings.ObserveAdd().Subscribe(_ => UpdateFilteredBookingsByCategory()).AddTo(_disposables);
        AllBookings.ObserveRemove().Subscribe(_ => UpdateFilteredBookingsByCategory()).AddTo(_disposables);
        AllBookings.ObserveReplace().Subscribe(_ => UpdateFilteredBookingsByCategory()).AddTo(_disposables);
        AllBookings.ObserveReset().Subscribe(_ => UpdateFilteredBookingsByCategory()).AddTo(_disposables);
    }
    private void InitializeExtraConfigs(AppConfig config)
    {
        _extraConfigs = new Dictionary<ExtraType, BookingExtraConfig.ExtraConfigData>();
        
        if (config?.extrasConfig != null)
        {
            foreach (var configData in config.extrasConfig.configs)
            {
                _extraConfigs[configData.type] = configData;
            }
        }
    }

    private void EnsureAllBookingsHaveAllExtras()
    {
        foreach (var booking in _allBookings)
        {
            EnsureBookingHasAllExtras(booking);
        }
    }

    private void EnsureBookingHasAllExtras(BookingModel booking)
    {
        if (booking.extras == null)
        {
            booking.extras = new List<BookingExtra>();
        }

        var allTypes = System.Enum.GetValues(typeof(ExtraType)).Cast<ExtraType>();
        
        foreach (var type in allTypes)
        {
            var existing = booking.extras.FirstOrDefault(e => e.type == type);
            if (existing == null)
            {
                var config = _extraConfigs.GetValueOrDefault(type);
                float price = config?.pricePerUnit ?? 0f;
                booking.extras.Add(new BookingExtra(type, 0, price));
            }
        }
    }

    private void InitializeAvailableExtras()
    {
        AvailableExtras.Clear();
        
        var allTypes = System.Enum.GetValues(typeof(ExtraType)).Cast<ExtraType>();
        
        foreach (var type in allTypes)
        {
            var config = _extraConfigs.GetValueOrDefault(type);
            if (config != null)
            {
                var currentExtra = SelectedExtras.Value.FirstOrDefault(e => e.type == type);
                int currentQuantity = currentExtra?.quantity ?? 0;
                
                var model = new BookingExtraModel(config, currentQuantity);
                AvailableExtras.Add(model);
            }
        }

        BindMirror(AvailableExtras, _availableExtrasAsObject);
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
        
        var initialExtras = InitializeExtrasForNewBooking();
        SelectedExtras.Value = initialExtras;
        DeselectAllTimes();

        CurrentDraft.Value = new BookingModel
        {
            stadiumId = stadium.id,
            status = BookingStatus.Draft,
            totalCost = 0f,
            extras = new List<BookingExtra>(initialExtras)
        };

        UpdateAllAvailableExtrasModels();
    }

    private List<BookingExtra> InitializeExtrasForNewBooking()
    {
        var extras = new List<BookingExtra>();
        var allTypes = System.Enum.GetValues(typeof(ExtraType)).Cast<ExtraType>();
        
        foreach (var type in allTypes)
        {
            var config = _extraConfigs.GetValueOrDefault(type);
            float price = config?.pricePerUnit ?? 0f;
            extras.Add(new BookingExtra(type, 0, price));
        }
        
        return extras;
    }

    private void UpdateAllAvailableExtrasModels()
    {
        var allTypes = System.Enum.GetValues(typeof(ExtraType)).Cast<ExtraType>();
        
        foreach (var type in allTypes)
        {
            var currentExtra = SelectedExtras.Value.FirstOrDefault(e => e.type == type);
            int currentQuantity = currentExtra?.quantity ?? 0;
            UpdateAvailableExtrasModel(type, currentQuantity);
        }
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

    private void InitializePitchSizes()
    {
        PitchSizes.Clear();
        var pitchSizes = System.Enum.GetValues(typeof(PitchSize)).Cast<PitchSize>();
        foreach (var size in pitchSizes)
        {
            var isSelected = SelectedPitchSize.Value == size;
            PitchSizes.Add(new ToggleButtonModel
            {
                name = size.ToString(),
                selected = isSelected
            });
        }
    }

    private void InitializeDurations()
    {
        Durations.Clear();
        var durations = System.Enum.GetValues(typeof(MatchDuration)).Cast<MatchDuration>();
        foreach (var duration in durations)
        {
            var isSelected = SelectedDuration.Value == duration;
            Durations.Add(new ToggleButtonModel
            {
                name = duration.ToString(),
                selected = isSelected
            });
        }
    }

    private void BindPitchSizeAndDurationCollections()
    {
        BindMirror(PitchSizes, _pitchSizesAsObject);
        BindMirror(Durations, _durationsAsObject);
    }

    public void UpdatePitchSizesSelection()
    {
        var sizes = System.Enum.GetValues(typeof(PitchSize)).Cast<PitchSize>().ToList();
        for (int i = 0; i < PitchSizes.Count && i < sizes.Count; i++)
        {
            var size = sizes[i];
            var model = PitchSizes[i];
            var newSelected = SelectedPitchSize.Value == size;

            if (model.selected != newSelected)
            {
                PitchSizes[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    public void UpdateDurationsSelection()
    {
        var durations = System.Enum.GetValues(typeof(MatchDuration)).Cast<MatchDuration>().ToList();
        for (int i = 0; i < Durations.Count && i < durations.Count; i++)
        {
            var duration = durations[i];
            var model = Durations[i];
            var newSelected = SelectedDuration.Value == duration;

            if (model.selected != newSelected)
            {
                Durations[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }

    private void BindMirror<T>(ReactiveCollection<T> source, ReactiveCollection<object> mirror)
    {
        mirror.Clear();
        foreach (var item in source)
        {
            mirror.Add(item);
        }

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
        UpdatePitchSizesSelection();
    }

    public void SetDuration(MatchDuration duration)
    {
        SelectedDuration.Value = duration;
        if (CurrentDraft.Value != null)
        {
            CurrentDraft.Value.duration = duration;
        }
        UpdateDurationsSelection();
    }

    public void SetExtraQuantity(ExtraType type, int quantity)
    {
        if (quantity < 0) return;

        var config = _extraConfigs.GetValueOrDefault(type);
        int max = config?.maxQuantity ?? 10;
        quantity = Mathf.Clamp(quantity, 0, max);

        var list = new List<BookingExtra>(SelectedExtras.Value);
        var existing = list.FirstOrDefault(e => e.type == type);

        if (quantity == 0)
        {
            if (existing != null)
            {
                existing.quantity = 0;
            }
        }
        else
        {
            float price = config?.pricePerUnit ?? 0f;

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
            EnsureBookingHasAllExtras(CurrentDraft.Value);
            var draftExtra = CurrentDraft.Value.extras.FirstOrDefault(e => e.type == type);
            if (draftExtra != null)
            {
                draftExtra.quantity = quantity;
                draftExtra.pricePerUnit = config?.pricePerUnit ?? 0f;
            }
        }

        UpdateAvailableExtrasModel(type, quantity);
    }

    private void UpdateAvailableExtrasModel(ExtraType type, int quantity)
    {
        var model = AvailableExtras.FirstOrDefault(m => m.type == type);
        if (model != null)
        {
            var index = AvailableExtras.IndexOf(model);
            if (index >= 0)
            {
                var config = _extraConfigs.GetValueOrDefault(type);
                if (config != null)
                {
                    AvailableExtras[index] = new BookingExtraModel(config, quantity);
                }
            }
        }
    }

    /// <summary>
    /// Удобные методы инкремент/декремент (для UI кнопок + / -)
    /// </summary>
    public void IncrementExtra(ExtraType type) => AdjustExtra(type, 1);
    public void DecrementExtra(ExtraType type) => AdjustExtra(type, -1);

    private void AdjustExtra(ExtraType type, int delta)
    {
        var current = SelectedExtras.Value.FirstOrDefault(e => e.type == type);
        int newQuantity = (current?.quantity ?? 0) + delta;
        SetExtraQuantity(type, newQuantity);
    }

    /// <summary>
    /// Пересчёт общей стоимости
    /// </summary>
    private void RecalculateTotalCost()
    {
        if (CurrentDraft.Value == null || SelectedStadium.Value == null)
        {
            TotalCost.Value = 0f;
            return;
        }

        var stadium = SelectedStadium.Value;
        float hours = (int)SelectedDuration.Value / 60f;
        float baseCost = stadium.basePricePerHour * hours;

        float extrasCost = 0f;
        foreach (var extra in SelectedExtras.Value)
        {
            extrasCost += extra.GetTotalPrice();
        }

        float total = baseCost + extrasCost;
        CurrentDraft.Value.totalCost = total;
        TotalCost.Value = total;
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
        var emptyExtras = InitializeExtrasForNewBooking();
        SelectedExtras.Value = emptyExtras;
        DeselectAllTimes();
        UpdateAllAvailableExtrasModels();
    }

    public void CancelDraft()
    {
        CurrentDraft.Value = null;
        SelectedStadium.Value = null;
        SelectedDateTimeIso.Value = null;
        SelectedDate.Value = null;
        SelectedTime.Value = null;
        var emptyExtras = InitializeExtrasForNewBooking();
        SelectedExtras.Value = emptyExtras;
        DeselectAllTimes();
        UpdateAllAvailableExtrasModels();
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

    private void InitializeCategoryFilters()
    {
        foreach (BookingCategoryType categoryType in System.Enum.GetValues(typeof(BookingCategoryType)))
        {
            CategoryFilters.Add(new ToggleButtonModel { name = categoryType.ToString(), selected = false });
        }
    }
    
    private void BindCategoryFilters()
    {
        BindMirror(CategoryFilters, _categoryFiltersAsObject);
        BindMirror(FilteredBookings, _filteredBookingsAsObject);
    }
    
    public void SelectCategory(BookingCategoryType? categoryType)
    {
        SelectedCategory.Value = categoryType;
        UpdateCategoryFiltersSelection();
    }
    
    private void UpdateCategoryFiltersSelection()
    {
        var categoryTypes = System.Enum.GetValues(typeof(BookingCategoryType));
        for (int i = 0; i < CategoryFilters.Count && i < categoryTypes.Length; i++)
        {
            var categoryType = (BookingCategoryType)categoryTypes.GetValue(i);
            var model = CategoryFilters[i];
            var newSelected = SelectedCategory.Value == categoryType;

            if (model.selected != newSelected)
            {
                CategoryFilters[i] = new ToggleButtonModel
                {
                    name = model.name,
                    selected = newSelected
                };
            }
        }
    }
    
    private void UpdateFilteredBookingsByCategory()
    {
        FilteredBookings.Clear();
        
        if (!SelectedCategory.Value.HasValue)
        {
            return;
        }
        
        var now = DateTime.UtcNow;
        var bookings = new List<BookingModel>();
        
        switch (SelectedCategory.Value.Value)
        {
            case BookingCategoryType.Upcoming:
                bookings = UpcomingBookings.ToList();
                break;
            case BookingCategoryType.Past:
                bookings = PastBookings.ToList();
                break;
        }
        
        foreach (var booking in bookings)
        {
            FilteredBookings.Add(booking);
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
        
        UpdateFilteredBookingsByCategory();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}