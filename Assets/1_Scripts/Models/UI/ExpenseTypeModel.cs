using System;

[Serializable]
public class ExpenseTypeModel
{
    public string name;
    public bool isPitch;

    public ExpenseTypeModel(string name, bool isPitch = false)
    {
        this.name = name;
        this.isPitch = isPitch;
    }
}

