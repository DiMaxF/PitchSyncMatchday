using System.Collections.Generic;
using System;

[Serializable]
public class AppModel
{
    public List<StadiumModel> stadiums = new List<StadiumModel>();
    public List<BookingModel> bookings = new List<BookingModel>();
}
