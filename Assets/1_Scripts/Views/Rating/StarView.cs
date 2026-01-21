using Cysharp.Threading.Tasks;
using UnityEngine;

public class StarView : UIView<bool>
{
    [SerializeField] private GameObject hide;
    public void SetShow() 
    {
        hide.SetActive(false);
    }

    public void SetHide() 
    {
        hide.SetActive(true);
    }
}