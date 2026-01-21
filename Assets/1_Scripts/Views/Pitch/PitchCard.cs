using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PitchCard : UIView<StadiumModel>
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text locationNameText;
    [SerializeField] private Image photo;
    [SerializeField] private Button pickButton;
    [SerializeField] private RatingView ratingView;

    private CompositeDisposable _disposables = new CompositeDisposable();

    protected override void Subscribe()
    {
        base.Subscribe();
        pickButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Trigger(DataProperty.Value);
            })
            .AddTo(this);

        // Подписываемся на изменения геолокации пользователя
        if (DataManager.Instance != null && DataManager.Profile != null)
        {
            DataManager.Profile.UserLatitude
                .Subscribe(_ => UpdateDistance())
                .AddTo(_disposables);

            DataManager.Profile.UserLongitude
                .Subscribe(_ => UpdateDistance())
                .AddTo(_disposables);

            DataManager.Profile.LocationPermissionGranted
                .Subscribe(_ => UpdateDistance())
                .AddTo(_disposables);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _disposables?.Dispose();
    }

    public override void UpdateUI()
    {
        var data = DataProperty.Value;

        if (nameText != null) nameText.text = data.name;
        if (ratingView != null) ratingView.Init(data.rating);
        if (priceText != null) priceText.text = $"From ${data.basePricePerHour}";
        if (locationNameText != null) locationNameText.text = $"{data.address}";
        if (photo != null) photo.sprite = data.photo;

        UpdateDistance();
    }

    private void UpdateDistance()
    {
        if (distanceText == null) return;

        var data = DataProperty.Value;
        if (data == null) return;

        // Проверяем, есть ли разрешение на геолокацию
        if (DataManager.Instance == null || DataManager.Profile == null || 
            !DataManager.Profile.LocationPermissionGranted.Value)
        {
            distanceText.text = "";
            return;
        }

        // Получаем текущую позицию пользователя
        Vector2 userLocation = new Vector2(
            DataManager.Profile.UserLatitude.Value,
            DataManager.Profile.UserLongitude.Value
        );

        // Проверяем, что координаты валидны (не нулевые)
        if (userLocation.x == 0f && userLocation.y == 0f)
        {
            distanceText.text = "";
            return;
        }

        // Вычисляем расстояние
        float distance = LocationService.CalculateDistance(userLocation, data.location);

        // Форматируем расстояние
        if (distance < 1f)
        {
            distanceText.text = $"{(distance * 1000f):0} m";
        }
        else
        {
            distanceText.text = $"{distance:F1} km";
        }
    }
}
