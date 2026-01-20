using System;
using System.Collections.Generic;
using System.Linq;

public static class IdGenerator
{
    public static int GetNextId(AppModel appModel, string modelType)
    {
        if (appModel.lastIds == null)
        {
            appModel.lastIds = new Dictionary<string, int>();
        }

        if (!appModel.lastIds.ContainsKey(modelType))
        {
            appModel.lastIds[modelType] = 0;
        }

        appModel.lastIds[modelType]++;
        return appModel.lastIds[modelType];
    }

    public static void InitializeIds(AppModel appModel)
    {
        if (appModel.lastIds == null)
        {
            appModel.lastIds = new Dictionary<string, int>();
        }

        if (appModel.players != null && appModel.players.Count > 0)
        {
            appModel.lastIds["Player"] = appModel.players.Max(p => p.id);
        }
        else
        {
            appModel.lastIds["Player"] = 0;
        }

        if (appModel.bookings != null && appModel.bookings.Count > 0)
        {
            appModel.lastIds["Booking"] = appModel.bookings.Max(b => b.id);
        }
        else
        {
            appModel.lastIds["Booking"] = 0;
        }

        if (appModel.matches != null && appModel.matches.Count > 0)
        {
            appModel.lastIds["Match"] = appModel.matches.Max(m => m.id);
        }
        else
        {
            appModel.lastIds["Match"] = 0;
        }

        if (appModel.lineups != null && appModel.lineups.Count > 0)
        {
            appModel.lastIds["Lineup"] = appModel.lineups.Max(l => l.id);
        }
        else
        {
            appModel.lastIds["Lineup"] = 0;
        }

        if (appModel.wallet != null && appModel.wallet.id > 0)
        {
            appModel.lastIds["Wallet"] = appModel.wallet.id;
        }
        else
        {
            appModel.lastIds["Wallet"] = 0;
        }

        if (appModel.stadiums != null && appModel.stadiums.Count > 0)
        {
            appModel.lastIds["Stadium"] = appModel.stadiums.Max(s => s.id);
        }
        else
        {
            appModel.lastIds["Stadium"] = 0;
        }

        int maxParticipantId = 0;
        int maxExpenseId = 0;
        if (appModel.wallet != null)
        {
            if (appModel.wallet.participants != null && appModel.wallet.participants.Count > 0)
            {
                maxParticipantId = appModel.wallet.participants.Max(p => p.id);
            }
            if (appModel.wallet.expenses != null && appModel.wallet.expenses.Count > 0)
            {
                maxExpenseId = appModel.wallet.expenses.Max(e => e.id);
            }
        }
        appModel.lastIds["Participant"] = maxParticipantId;
        appModel.lastIds["Expense"] = maxExpenseId;
    }
}

