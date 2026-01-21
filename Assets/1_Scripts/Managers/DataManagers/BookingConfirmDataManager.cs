using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Common;

public class BookingConfirmDataManager : IDataManager
{
    private readonly AppModel _appModel;
    private Dictionary<ExtraType, BookingExtraConfig.ExtraConfigData> _extraConfigs;
    
    public ReactiveProperty<BookingModel> ConfirmedBooking { get; } = new ReactiveProperty<BookingModel>(null);
    public ReactiveProperty<Texture2D> QRCodeTexture { get; } = new ReactiveProperty<Texture2D>(null);
    public ReactiveProperty<string> StadiumName { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<string> StadiumAddress { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<string> DateTimeText { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<string> PitchSizeText { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<string> DurationText { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<float> TotalPrice { get; } = new ReactiveProperty<float>(0f);
    public ReactiveCollection<BookingExtraModel> ExtrasSummary { get; } = new ReactiveCollection<BookingExtraModel>();
    private readonly ReactiveCollection<object> _extrasSummaryAsObject = new ReactiveCollection<object>();
    public ReactiveCollection<object> ExtrasSummaryAsObject => _extrasSummaryAsObject;
    
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    
    public BookingConfirmDataManager(AppModel appModel, AppConfig config)
    {
        _appModel = appModel;
        InitializeExtraConfigs(config);
        BindMirror(ExtrasSummary, _extrasSummaryAsObject);
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
    
    public void InitializeForBooking(BookingModel booking)
    {
        ConfirmedBooking.Value = booking;
        
        if (booking == null)
        {
            ClearAll();
            return;
        }
        
        var stadium = GetStadiumById(booking.stadiumId);
        if (stadium != null)
        {
            StadiumName.Value = stadium.name;
            StadiumAddress.Value = stadium.address;
        }
        
        if (DateTime.TryParse(booking.dateTimeIso, out var dateTime))
        {
            DateTimeText.Value = dateTime.ToString("dd MMMM yyyy, HH:mm");
        }
        else
        {
            DateTimeText.Value = booking.dateTimeIso;
        }
        
        PitchSizeText.Value = booking.pitchSize.ToString();
        DurationText.Value = $"{(int)booking.duration} min";
        TotalPrice.Value = booking.totalCost;
        
        ExtrasSummary.Clear();
        if (booking.extras != null)
        {
            foreach (var extra in booking.extras.Where(e => e.quantity > 0))
            {
                var config = _extraConfigs.GetValueOrDefault(extra.type);
                if (config != null)
                {
                    var model = new BookingExtraModel(config, extra.quantity);
                    ExtrasSummary.Add(model);
                }
            }
        }
        
        string payload = booking.qrPayload;
        if (string.IsNullOrEmpty(payload))
        {
            payload = GenerateCompactQRPayload(booking);
        }
        GenerateQRCode(payload);
    }
    
    private string GenerateCompactQRPayload(BookingModel booking)
    {
        if (booking == null) return string.Empty;
        
        int durationMinutes = (int)booking.duration;
        return $"{booking.id}|{booking.dateTimeIso}|{durationMinutes}";
    }

    private StadiumModel GetStadiumById(int stadiumId)
    {
        if (_appModel?.stadiums == null) return null;
        return _appModel.stadiums.FirstOrDefault(s => s.id == stadiumId);
    }
    
    private void GenerateQRCode(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            QRCodeTexture.Value = null;
            return;
        }
        
        var writer = new MultiFormatWriter();
        var hints = new Dictionary<EncodeHintType, object>();
        hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
        hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.M);
        
        int qrSize = 256;
        var bitMatrix = writer.encode(payload, BarcodeFormat.QR_CODE, qrSize, qrSize, hints);
        
        var texture = new Texture2D(bitMatrix.Width, bitMatrix.Height, TextureFormat.RGBA32, false);
        
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                if (bitMatrix[i, j])
                {
                    texture.SetPixel(i, j, Color.white);
                }
                else
                {
                    texture.SetPixel(i, j, new Color(0, 0, 0, 0));
                }
            }
        }
        
        texture.Apply();
        
        if (QRCodeTexture.Value != null)
        {
            UnityEngine.Object.Destroy(QRCodeTexture.Value);
        }
        
        QRCodeTexture.Value = texture;
    }
    
    private void ClearAll()
    {
        StadiumName.Value = string.Empty;
        StadiumAddress.Value = string.Empty;
        DateTimeText.Value = string.Empty;
        PitchSizeText.Value = string.Empty;
        DurationText.Value = string.Empty;
        TotalPrice.Value = 0f;
        ExtrasSummary.Clear();
        
        if (QRCodeTexture.Value != null)
        {
            UnityEngine.Object.Destroy(QRCodeTexture.Value);
            QRCodeTexture.Value = null;
        }
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
        ClearAll();
    }
}

