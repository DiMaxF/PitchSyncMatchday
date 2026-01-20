using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AddExpensePanel : UIView
{
    [SerializeField] private Button saveButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField amountInput;
    [SerializeField] private ListContainer expenseTypes;

    private ExtraType _selectedType = ExtraType.Ball;
    private WalletDataManager Wallet => DataManager.Wallet;

    protected override void Subscribe()
    {
        base.Subscribe();

        if (expenseTypes != null)
        {
            InitializeExpenseTypes();
            expenseTypes.Init(Wallet.ExpenseTypesAsObject);

            AddToDispose(UIManager.SubscribeToView(expenseTypes, (ToggleButtonModel data) =>
            {
                if (Enum.TryParse<ExtraType>(data.name, out var type))
                {
                    _selectedType = type;
                }
            }));
        }

        if (saveButton != null)
        {
            saveButton.OnClickAsObservable()
                .Subscribe(_ => OnSaveClicked())
                .AddTo(this);
        }

        if (closeButton != null)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ => Hide())
                .AddTo(this);
        }
    }

    private void InitializeExpenseTypes()
    {
        for (int i = 0; i < Wallet.ExpenseTypes.Count; i++)
        {
            var typeModel = Wallet.ExpenseTypes[i];
            if (Enum.TryParse<ExtraType>(typeModel.name, out var type))
            {
                Wallet.ExpenseTypes[i] = new ToggleButtonModel
                {
                    name = typeModel.name,
                    selected = type == _selectedType
                };
            }
        }
    }

    private void OnSaveClicked()
    {
        if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.text)) return;
        if (amountInput == null || string.IsNullOrWhiteSpace(amountInput.text)) return;

        if (decimal.TryParse(amountInput.text, out decimal amount))
        {
            Wallet.AddExpense(nameInput.text, amount, _selectedType);
            Hide();
        }
    }

    public void InitForAdd()
    {
        if (nameInput != null) nameInput.text = "";
        if (amountInput != null) amountInput.text = "";
        _selectedType = ExtraType.Ball;
        InitializeExpenseTypes();
        Show();
    }
}

