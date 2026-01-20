using System;
using System.Collections.Generic;

[Serializable]
public class BookingModel
{
    public int id;
    public int stadiumId;
    public string dateTimeIso;
    public PitchSize pitchSize;
    public MatchDuration duration;
    public List<BookingExtra> extras = new List<BookingExtra>(); 
    public float totalCost;
    public BookingStatus status = BookingStatus.Draft;
    public string qrPayload;
    public int? matchId;

    public BookingModel() { }

    public BookingModel(int stadiumId, string dateTimeIso, AppModel appModel)
    {
        id = IdGenerator.GetNextId(appModel, "Booking");
        this.stadiumId = stadiumId;
        this.dateTimeIso = dateTimeIso;
    }
}