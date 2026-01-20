using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class WalletDataManager : IDataManager
{
    private readonly AppModel _appModel;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public ReactiveProperty<WalletModel> CurrentWallet { get; } = new ReactiveProperty<WalletModel>(null);
    public ReactiveCollection<ParticipantModel> Participants { get; } = new ReactiveCollection<ParticipantModel>();
    public ReactiveCollection<ExpenseModel> Expenses { get; } = new ReactiveCollection<ExpenseModel>();
    public ReactiveProperty<SplitMode> CurrentSplitMode { get; } = new ReactiveProperty<SplitMode>(SplitMode.Equal);
    public ReactiveCollection<ToggleButtonModel> ExpenseTypes { get; } = new ReactiveCollection<ToggleButtonModel>();
    public ReactiveProperty<float> TotalCost { get; } = new ReactiveProperty<float>(0);
    public ReactiveProperty<float> TotalPaid { get; } = new ReactiveProperty<float>(0);

    private readonly ReactiveCollection<object> _participantsAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _expensesAsObject = new ReactiveCollection<object>();
    private readonly ReactiveCollection<object> _expenseTypesAsObject = new ReactiveCollection<object>();

    public ReactiveCollection<object> ParticipantsAsObject => _participantsAsObject;
    public ReactiveCollection<object> ExpensesAsObject => _expensesAsObject;
    public ReactiveCollection<object> ExpenseTypesAsObject => _expenseTypesAsObject;

    public WalletDataManager(AppModel appModel)
    {
        _appModel = appModel;

        BindCollections();
        InitializeExpenseTypes();
        CurrentSplitMode.Subscribe(_ => RecalculateOwedAmounts()).AddTo(_disposables);
        
        LoadWallet();
    }

    private void InitializeExpenseTypes()
    {
        foreach (ExtraType type in Enum.GetValues(typeof(ExtraType)))
        {
            ExpenseTypes.Add(new ToggleButtonModel
            {
                name = type.ToString(),
                selected = false
            });
        }

        ExpenseTypes.ObserveAdd().Subscribe(e => _expenseTypesAsObject.Insert(e.Index, e.Value)).AddTo(_disposables);
        ExpenseTypes.ObserveRemove().Subscribe(e => _expenseTypesAsObject.RemoveAt(e.Index)).AddTo(_disposables);
        ExpenseTypes.ObserveReplace().Subscribe(e => _expenseTypesAsObject[e.Index] = e.NewValue).AddTo(_disposables);
        ExpenseTypes.ObserveReset().Subscribe(_ =>
        {
            _expenseTypesAsObject.Clear();
            foreach (var t in ExpenseTypes) _expenseTypesAsObject.Add(t);
        }).AddTo(_disposables);
    }

    private void BindCollections()
    {
        Participants.ObserveAdd().Subscribe(e => _participantsAsObject.Insert(e.Index, e.Value)).AddTo(_disposables);
        Participants.ObserveRemove().Subscribe(e => _participantsAsObject.RemoveAt(e.Index)).AddTo(_disposables);
        Participants.ObserveReplace().Subscribe(e => _participantsAsObject[e.Index] = e.NewValue).AddTo(_disposables);
        Participants.ObserveReset().Subscribe(_ =>
        {
            _participantsAsObject.Clear();
            foreach (var p in Participants) _participantsAsObject.Add(p);
        }).AddTo(_disposables);

        Expenses.ObserveAdd().Subscribe(e => { _expensesAsObject.Insert(e.Index, e.Value); RecalculateTotals(); }).AddTo(_disposables);
        Expenses.ObserveRemove().Subscribe(e => { _expensesAsObject.RemoveAt(e.Index); RecalculateTotals(); }).AddTo(_disposables);
        Expenses.ObserveReplace().Subscribe(e => { _expensesAsObject[e.Index] = e.NewValue; RecalculateTotals(); }).AddTo(_disposables);
        Expenses.ObserveReset().Subscribe(_ =>
        {
            _expensesAsObject.Clear();
            foreach (var e in Expenses) _expensesAsObject.Add(e);
            RecalculateTotals();
        }).AddTo(_disposables);

        Participants.ObserveAdd().Subscribe(_ => { RecalculateOwedAmounts(); RecalculateTotals(); }).AddTo(_disposables);
        Participants.ObserveRemove().Subscribe(_ => { RecalculateOwedAmounts(); RecalculateTotals(); }).AddTo(_disposables);
        Participants.ObserveReplace().Subscribe(_ => { RecalculateOwedAmounts(); RecalculateTotals(); }).AddTo(_disposables);
    }

    private void LoadWallet()
    {
        WalletModel wallet = null;
        
        if (FileUtils.FileExists("Wallet.json"))
        {
            wallet = FileUtils.LoadJson<WalletModel>("Wallet.json");
            
            if (wallet != null && wallet.id > 0)
            {
                if (wallet.participants == null)
                {
                    wallet.participants = new List<ParticipantModel>();
                }
                if (wallet.expenses == null)
                {
                    wallet.expenses = new List<ExpenseModel>();
                }
                
                CurrentWallet.Value = wallet;
                LoadWalletData(wallet);
                return;
            }
        }
        
        wallet = new WalletModel
        {
            id = IdGenerator.GetNextId(_appModel, "Wallet"),
            createdAt = DateTime.UtcNow,
            splitMode = SplitMode.Equal,
            participants = new List<ParticipantModel>(),
            expenses = new List<ExpenseModel>()
        };

        CurrentWallet.Value = wallet;
        LoadWalletData(wallet);
    }

    public void InitializeWallet(int? bookingId = null, int? matchId = null)
    {
        if (CurrentWallet.Value == null)
        {
            LoadWallet();
        }

        if (CurrentWallet.Value != null)
        {
            if (bookingId.HasValue)
            {
                CurrentWallet.Value.bookingId = bookingId;
            }
            if (matchId.HasValue)
            {
                CurrentWallet.Value.matchId = matchId;
            }
            SaveWallet();
        }
    }

    private bool _isLoading = false;

    private void LoadWalletData(WalletModel wallet)
    {
        _isLoading = true;

        Participants.Clear();
        Expenses.Clear();

        if (wallet.participants != null)
        {
            foreach (var p in wallet.participants)
            {
                Participants.Add(p);
            }
        }

        if (wallet.expenses != null)
        {
            foreach (var e in wallet.expenses)
            {
                Expenses.Add(e);
            }
        }

        CurrentSplitMode.Value = wallet.splitMode;
        
        float totalCost = Expenses.Sum(e => e.amount);
        float totalPaid = Participants.Sum(p => p.paidAmount);

        TotalCost.Value = totalCost;
        TotalPaid.Value = totalPaid;

        CurrentWallet.Value.totalCost = totalCost;
        CurrentWallet.Value.totalPaid = totalPaid;

        RecalculateOwedAmounts();
        
        _isLoading = false;
    }

    public void AddParticipant(int? playerId, string name,
         float amount)
    {
        if (CurrentWallet.Value == null) return;

        var participant = new ParticipantModel
        {
            id = IdGenerator.GetNextId(_appModel, "Participant"),
            playerId = playerId,
            name = name,
            paidAmount = amount,
            owedAmount = 0
        };

        Participants.Add(participant);
        SaveWallet();
    }

    public void UpdateParticipant(int participantId, float paidAmount, float owedAmount)
    {
        var participant = Participants.FirstOrDefault(p => p.id == participantId);
        if (participant == null) return;

        var index = Participants.IndexOf(participant);
        Participants[index] = new ParticipantModel
        {
            id = participant.id,
            playerId = participant.playerId,
            name = participant.name,
            paidAmount = paidAmount,
            owedAmount = owedAmount
        };

        SaveWallet();
    }

    public void RemoveParticipant(int participantId)
    {
        var participant = Participants.FirstOrDefault(p => p.id == participantId);
        if (participant != null)
        {
            Participants.Remove(participant);
            SaveWallet();
        }
    }

    public void AddExpense(string name, float amount, ExtraType type)
    {
        if (CurrentWallet.Value == null) return;

        var expense = new ExpenseModel
        {
            id = IdGenerator.GetNextId(_appModel, "Expense"),
            name = name,
            amount = amount,
            type = type,
            createdAt = DateTime.UtcNow
        };

        Expenses.Add(expense);
        SaveWallet();
    }

    public void RemoveExpense(int expenseId)
    {
        var expense = Expenses.FirstOrDefault(e => e.id == expenseId);
        if (expense != null)
        {
            Expenses.Remove(expense);
            SaveWallet();
        }
    }

    public void SetSplitMode(SplitMode mode)
    {
        CurrentSplitMode.Value = mode;
        if (CurrentWallet.Value != null)
        {
            CurrentWallet.Value.splitMode = mode;
            SaveWallet();
        }
    }

    private void RecalculateTotals()
    {
        if (CurrentWallet.Value == null) return;

        float totalCost = Expenses.Sum(e => e.amount);
        float totalPaid = Participants.Sum(p => p.paidAmount);

        TotalCost.Value = totalCost;
        TotalPaid.Value = totalPaid;

        CurrentWallet.Value.totalCost = totalCost;
        CurrentWallet.Value.totalPaid = totalPaid;

        RecalculateOwedAmounts();
        
        if (!_isLoading)
        {
            SaveWallet();
        }
    }

    private void RecalculateOwedAmounts()
    {
        if (CurrentWallet.Value == null || Participants.Count == 0) return;

        float totalCost = Expenses.Sum(e => e.amount);

        if (CurrentSplitMode.Value == SplitMode.Equal)
        {
            float perPerson = totalCost / Participants.Count;

            for (int i = 0; i < Participants.Count; i++)
            {
                var p = Participants[i];
                float newOwedAmount = perPerson - p.paidAmount;
                
                if (Mathf.Abs(p.owedAmount - newOwedAmount) > 0.01f)
                {
                    Participants[i] = new ParticipantModel
                    {
                        id = p.id,
                        playerId = p.playerId,
                        name = p.name,
                        paidAmount = p.paidAmount,
                        owedAmount = newOwedAmount
                    };
                }
            }
        }
    }

    private void SaveWallet()
    {
        if (CurrentWallet.Value == null || _isLoading) return;

        CurrentWallet.Value.participants = Participants.ToList();
        CurrentWallet.Value.expenses = Expenses.ToList();
        
        if (CurrentWallet.Value.createdAt == default(DateTime))
        {
            CurrentWallet.Value.createdAt = DateTime.UtcNow;
        }
        
        CurrentWallet.Value.lastModified = DateTime.UtcNow;

        FileUtils.SaveJson(CurrentWallet.Value, "Wallet.json");
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

