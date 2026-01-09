using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class ListContainer<TItem> : UIView<List<TItem>>
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private UIView<TItem> itemPrefab;           
    [SerializeField] private UIView noItemPrefab;              
    [SerializeField] private float spawnDelayPerItem = 0.05f;    

    private readonly List<UIView<TItem>> _spawnedItems = new List<UIView<TItem>>();

    public override void UpdateUI()
    {
        var itemsData = DataProperty.Value ?? new List<TItem>();

        ClearItems();

        foreach (var itemData in itemsData)
        {
            SpawnItem(itemData);
        }
            
        if (itemsData.Count == 0 && noItemPrefab != null)
        {
            Instantiate(noItemPrefab, contentParent);
        }

        AnimateItemsSpawn();
    }

    private void SpawnItem(TItem itemData)
    {
        var itemInstance = Instantiate(itemPrefab, contentParent);
        UIManager.RegisterView(itemInstance); 

        itemInstance.Init(itemData);
        _spawnedItems.Add(itemInstance);
    }

    private void ClearItems()
    {
        foreach (var item in _spawnedItems)
        {
            if (item != null)
            {
                UIManager.UnregisterView(item);
                Destroy(item.gameObject);
            }
        }

        _spawnedItems.Clear();

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private async void AnimateItemsSpawn()
    {
        foreach (var item in _spawnedItems)
        {
            if (item != null) item.gameObject.SetActive(false);
        }

        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            var item = _spawnedItems[i];
            if (item == null) continue;

            await UniTask.Delay(System.TimeSpan.FromSeconds(spawnDelayPerItem),
                cancellationToken: this.GetCancellationTokenOnDestroy());

            item.ShowAsync().Forget();
        }
    }

    public void ForceRefresh(List<TItem> newData)
    {
        DataProperty.Value = newData;
    }
}
