using System;
using System.Collections.Generic;

[Serializable]
public class WalletModel
{
    public int id;
    public int? bookingId;                  
    public int? matchId;                     

    public decimal totalCost;               
    public decimal totalPaid;             

    public List<ParticipantModel> participants = new List<ParticipantModel>();
    public List<ExpenseModel> expenses = new List<ExpenseModel>();

    public SplitMode splitMode = SplitMode.Equal; 

    public string qrPayload;                 

    public DateTime createdAt;
    public DateTime? lastModified;
}