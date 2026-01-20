using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AddExpensePanel : UIView
{
    [SerializeField] private Text selectedTypeText;
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField amountInput;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button dropdownShow;
    [SerializeField] private Button dropdownHide;
    [SerializeField] private ListContainer dropdownCategory;

    private WalletDataManager Wallet => DataManager.Wallet;
    private readonly ReactiveCollection<object> _expenseTypesAsObject = new ReactiveCollection<object>();
    private ExpenseTypeModel _selectedType;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        InitializeExpenseTypes();

        if (dropdownCategory != null)
        {
            dropdownCategory.Init(_expenseTypesAsObject);
            dropdownCategory.gameObject.SetActive(false);

            AddToDispose(UIManager.SubscribeToView(dropdownCategory, (ExpenseTypeModel type) =>
            {
                if (type != null)
                {
                    _selectedType = type;
                    if (selectedTypeText != null)
                    {
                        selectedTypeText.text = type.name;
                    }
                    HideDropdown();
                }
            }));
        }

        _selectedType = null;
        if (selectedTypeText != null)
        {
            selectedTypeText.text = "None";
        }

        if (dropdownShow != null)
        {
            dropdownShow.OnClickAsObservable()
                .Subscribe(_ => ShowDropdown())
                .AddTo(this);
        }

        if (saveButton != null)
        {
            saveButton.OnClickAsObservable()
                .Subscribe(_ => Save())
                .AddTo(this);
        }

        if (dropdownHide != null)
        {
            dropdownHide.OnClickAsObservable()
                .Subscribe(_ => HideDropdown())
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
        _expenseTypesAsObject.Clear();

        foreach (ExtraType type in Enum.GetValues(typeof(ExtraType)))
        {
            _expenseTypesAsObject.Add(new ExpenseTypeModel(type.ToString(), false));
        }

        _expenseTypesAsObject.Add(new ExpenseTypeModel("Pitch", true));
    }

    private void Save()
    {
        if (nameInput == null || string.IsNullOrWhiteSpace(nameInput.text)) return;
        if (amountInput == null || string.IsNullOrWhiteSpace(amountInput.text)) return;
        if (_selectedType == null) return;

        if (float.TryParse(amountInput.text, out float amount))
        {
            ExtraType expenseType = ExtraType.Ball;
            if (!_selectedType.isPitch && Enum.TryParse<ExtraType>(_selectedType.name, out var type))
            {
                expenseType = type;
            }

            Wallet.AddExpense(nameInput.text, amount, expenseType);
            Hide();
        }
    }

    private void ShowDropdown()
    {
        if (dropdownCategory != null)
        {
            dropdownCategory.Show();
        }
        if (dropdownHide != null)
        {
            dropdownHide.gameObject.SetActive(true);
        }
    }

    private void HideDropdown()
    {
        if (dropdownCategory != null)
        {
            dropdownCategory.Hide();
        }
        if (dropdownHide != null)
        {
            dropdownHide.gameObject.SetActive(false);
        }
    }

    public void InitForAdd()
    {
        _selectedType = null;
        if (selectedTypeText != null)
        {
            selectedTypeText.text = "None";
        }
        if (nameInput != null)
        {
            nameInput.text = "";
        }
        if (amountInput != null)
        {
            amountInput.text = "";
        }
        HideDropdown();
        Show();
    }
}

