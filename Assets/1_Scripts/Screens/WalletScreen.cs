using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class WalletScreen : UIScreen
{
    [SerializeField] private Text totalCost;
    [SerializeField] private Text paidText;
    [SerializeField] private Text remainigText;

    [SerializeField] private ListContainer participants;
    [SerializeField] private Button participamtsAdd;
    [SerializeField] private ListContainer expenses;
    [SerializeField] private Button expensesAdd;
    [SerializeField] private SimpleToggle splitToggle;
    [SerializeField] private SimpleToggle manualToggle;
    [SerializeField] private AddParticipantsPanel addParticipantsPanel;
    [SerializeField] private AddExpensePanel addExpensePanel;
    [SerializeField] private ConfirmPanel confirmPanel;

    private WalletDataManager Wallet => DataManager.Wallet;
    private int? _pendingParticipantIdToRemove;
    private int? _pendingExpenseIdToRemove;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (addParticipantsPanel != null) addParticipantsPanel.gameObject.SetActive(false);
        if (addExpensePanel != null) addExpensePanel.gameObject.SetActive(false);
        if (confirmPanel != null) confirmPanel.gameObject.SetActive(false);
    }

    protected override void SubscribeToData()
    {
        base.SubscribeToData();

        if (Wallet.CurrentWallet.Value == null)
        {
            Wallet.InitializeWallet();
        }

        if (participants != null)
        {
            participants.Init(Wallet.ParticipantsAsObject);

            AddToDispose(UIManager.SubscribeToView(participants, (int participantId) =>
            {
                ShowConfirmRemoveParticipant(participantId);
            }));
        }

        if (participamtsAdd != null)
        {
            AddToDispose(participamtsAdd.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (addParticipantsPanel != null)
                    {
                        addParticipantsPanel.InitForAdd();
                    }
                }));
        }

        if (expenses != null)
        {
            expenses.Init(Wallet.ExpensesAsObject);

            AddToDispose(UIManager.SubscribeToView(expenses, (int expenseId) =>
            {
                ShowConfirmRemoveExpense(expenseId);
            }));
        }

        if (expensesAdd != null)
        {
            AddToDispose(expensesAdd.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (addExpensePanel != null)
                    {
                        addExpensePanel.InitForAdd();
                    }
                }));
        }

        if (splitToggle != null)
        {
            AddToDispose(UIManager.SubscribeToView(splitToggle, (bool isOn) =>
            {
                Wallet.SetSplitMode(isOn ? SplitMode.Equal : SplitMode.Manual);
            }));

            AddToDispose(Wallet.CurrentSplitMode.Subscribe(mode =>
            {
                splitToggle.Init(mode == SplitMode.Equal);
            }));
        }

        if (manualToggle != null)
        {
            AddToDispose(UIManager.SubscribeToView(manualToggle, (bool isOn) =>
            {
                Wallet.SetSplitMode(isOn ? SplitMode.Manual : SplitMode.Equal);
            }));

            AddToDispose(Wallet.CurrentSplitMode.Subscribe(mode =>
            {
                manualToggle.Init(mode == SplitMode.Manual);
            }));
        }

        AddToDispose(Wallet.TotalCost.Subscribe(_ => UpdateTotals()));
        AddToDispose(Wallet.TotalPaid.Subscribe(_ => UpdateTotals()));

        if (confirmPanel != null)
        {
            AddToDispose(UIManager.SubscribeToView(confirmPanel, (bool confirmed) =>
            {
                if (confirmed)
                {
                    if (_pendingParticipantIdToRemove.HasValue)
                    {
                        Wallet.RemoveParticipant(_pendingParticipantIdToRemove.Value);
                        _pendingParticipantIdToRemove = null;
                    }
                    else if (_pendingExpenseIdToRemove.HasValue)
                    {
                        Wallet.RemoveExpense(_pendingExpenseIdToRemove.Value);
                        _pendingExpenseIdToRemove = null;
                    }
                }
                else
                {
                    _pendingParticipantIdToRemove = null;
                    _pendingExpenseIdToRemove = null;
                }
            }));
        }
    }

    private void ShowConfirmRemoveParticipant(int participantId)
    {
        var participant = Wallet.Participants.FirstOrDefault(p => p.id == participantId);
        if (participant == null || confirmPanel == null) return;

        _pendingParticipantIdToRemove = participantId;

        var model = new ConfirmPanelModel
        {
            title = "Remove Participant",
            subtitle = $"Are you sure you want to remove {participant.name}?",
            acceptText = "Remove",
            declineText = "Cancel"
        };

        confirmPanel.Init(model);
        confirmPanel.Show();
    }

    private void ShowConfirmRemoveExpense(int expenseId)
    {
        var expense = Wallet.Expenses.FirstOrDefault(e => e.id == expenseId);
        if (expense == null || confirmPanel == null) return;

        _pendingExpenseIdToRemove = expenseId;

        var model = new ConfirmPanelModel
        {
            title = "Remove Expense",
            subtitle = $"Are you sure you want to remove {expense.name} (${expense.amount:F2})?",
            acceptText = "Remove",
            declineText = "Cancel"
        };

        confirmPanel.Init(model);
        confirmPanel.Show();
    }

    private void UpdateTotals()
    {
        float totalCostValue = Wallet.TotalCost.Value;
        float totalPaidValue = Wallet.TotalPaid.Value;
        float remaining = totalCostValue - totalPaidValue;

        if (totalCost != null)
        {
            totalCost.text = $"${totalCostValue:F2}";
        }

        if (paidText != null)
        {
            paidText.text = $"${totalPaidValue:F2}";
        }

        if (remainigText != null)
        {
            remainigText.text = $"${remaining:F2}";
        }
    }

    protected override void RefreshViews()
    {
        base.RefreshViews();
        UpdateTotals();
    }
}
