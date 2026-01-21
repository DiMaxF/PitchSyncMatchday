using System.Collections.Generic;
using System;

[Serializable]
public class AppModel
{
    public List<StadiumModel> stadiums = new List<StadiumModel>();
    public List<BookingModel> bookings = new List<BookingModel>();
    public List<PlayerModel> players = new List<PlayerModel>();
    public List<MatchModel> matches = new List<MatchModel>();
    public List<LineupModel> lineups = new List<LineupModel>();
    public WalletModel wallet;
    public List<NotificationModel> notifications = new List<NotificationModel>();
    public Dictionary<string, int> lastIds = new Dictionary<string, int>();
    
    public int matchesPlayed = 0;
    public int bookingsCount = 0;
    public int lineupCreatedCount = 0;
    public bool notificationsEnabled = true;
    public PitchSize defaultPitchSize = PitchSize.Size7x7;
    public MatchDuration defaultDuration = MatchDuration.Min60;
    public string profileName = "User";
    public string profileEmail = "user@example.com";
    public string profileUserpicPath = "";
    public float userLatitude = 0f;
    public float userLongitude = 0f;
    public bool locationPermissionGranted = false;
}
