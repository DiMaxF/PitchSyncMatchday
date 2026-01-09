using System.Collections.Generic;
using UnityEngine;

public class Navbar : UIView<List<NavbarButtonModel>>
{
   // [SerializeField] private ListContainer<NavbarButtonModel> listView;

    public override void UpdateUI()
    {
        //listView.ForceRefresh(DataProperty.Value ?? new List<NavbarButtonModel>());
    }

    protected override void Subscribe()
    {

    }
}
