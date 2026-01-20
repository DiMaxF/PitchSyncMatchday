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

    private WalletDataManager Wallet => DataManager.Wallet;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (addParticipantsPanel != null) addParticipantsPanel.gameObject.SetActive(false);
        if (addExpensePanel != null) addExpensePanel.gameObject.SetActive(false);
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
                Wallet.RemoveParticipant(participantId);
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
                Wallet.RemoveExpense(expenseId);
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
