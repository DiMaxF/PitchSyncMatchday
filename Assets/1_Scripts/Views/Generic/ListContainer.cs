using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class ListContainer : UIView<ReactiveCollection<object>>
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private UIView itemPrefab;
    [SerializeField] private UIView noItemPrefab;
    [SerializeField] private float spawnDelayPerItem = 0.05f;
    [SerializeField] private bool animateRemoval = true;

    private readonly Dictionary<int, UIView> _spawnedItems = new Dictionary<int, UIView>();
    private bool _isInitialized = false;

    public new void Init(object data)
    {
        if (data is ReactiveCollection<object> collection)
        {
            if (_isInitialized && DataProperty.Value == collection) return;

            DataProperty.Value = collection;
            ClearAllItems();
            InitializeFromCollection(collection);
            _isInitialized = true;
        }
    }

    private void InitializeFromCollection(ReactiveCollection<object> collection)
    {
        collection.ObserveAdd().Subscribe(e => OnItemAdded(e.Index, e.Value)).AddTo(this);
        collection.ObserveRemove().Subscribe(e => OnItemRemoved(e.Index, e.Value)).AddTo(this);
        collection.ObserveReset().Subscribe(_ => OnCollectionReset()).AddTo(this);
        collection.ObserveMove().Subscribe(e => OnItemMoved(e.OldIndex, e.NewIndex)).AddTo(this);
        collection.ObserveReplace().Subscribe(e => OnItemReplaced(e.Index, e.NewValue)).AddTo(this);

        for (int i = 0; i < collection.Count; i++)
        {
            OnItemAdded(i, collection[i]);
        }

        UpdateNoItemView();
    }

    private void OnItemAdded(int index, object itemData)
    {
        if (itemPrefab == null) return;

        var instance = Instantiate(itemPrefab, contentParent);
        UIManager.RegisterView(instance);

        var initMethod = instance.GetType().GetMethod("Init", new[] { typeof(object) });
        if (initMethod != null)
        {
            initMethod.Invoke(instance, new[] { itemData });
        }
        else
        {
            var genericInit = instance.GetType().GetMethod("Init");
            genericInit?.Invoke(instance, itemData != null ? new[] { itemData } : null);
        }

        _spawnedItems[index] = instance;

        UIManager.ForwardEventsFrom(instance, this).AddTo(this);

        instance.gameObject.SetActive(false);
        AnimateItemAppear(instance, index).Forget();
    }

    private async UniTask AnimateItemAppear(UIView item, int index)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(index * spawnDelayPerItem),
            cancellationToken: this.GetCancellationTokenOnDestroy());
        if (item != null) 
        {
            await item.ShowAsync();
        }
    }

    private async void OnItemRemoved(int index, object itemData)
    {
        if (!_spawnedItems.TryGetValue(index, out var item) || item == null) return;

        _spawnedItems.Remove(index);

        if (animateRemoval)
        {
            await item.HideAsync();
        }

        UIManager.UnregisterView(item);
        Destroy(item.gameObject);

        ReindexAfterRemoval(index);
        UpdateNoItemView();
    }

    private void OnItemReplaced(int index, object newData)
    {
        if (_spawnedItems.TryGetValue(index, out var item) && item != null)
        {
            var initMethod = item.GetType().GetMethod("Init", new[] { typeof(object) });
            initMethod?.Invoke(item, new[] { newData });
        }
    }

    private void OnItemMoved(int oldIndex, int newIndex)
    {
        if (_spawnedItems.TryGetValue(oldIndex, out var item) && item != null)
        {
            item.transform.SetSiblingIndex(newIndex);
            _spawnedItems.Remove(oldIndex);
            _spawnedItems[newIndex] = item;
        }
    }

    private void ReindexAfterRemoval(int removedIndex)
    {
        var keys = _spawnedItems.Keys.Where(k => k > removedIndex).OrderBy(k => k).ToList();
        foreach (var oldKey in keys)
        {
            var item = _spawnedItems[oldKey];
            _spawnedItems.Remove(oldKey);
            _spawnedItems[oldKey - 1] = item;
        }
    }

    private void OnCollectionReset()
    {
        ClearAllItems();
        UpdateNoItemView();
    }

    private void ClearAllItems()
    {
        foreach (var item in _spawnedItems.Values)
        {
            if (item != null)
            {
                UIManager.UnregisterView(item);
                Destroy(item.gameObject);
            }
        }
        _spawnedItems.Clear();
        ClearNoItemView();
    }

    private void UpdateNoItemView()
    {
        ClearNoItemView();

        if (DataProperty.Value != null && DataProperty.Value.Count == 0 && noItemPrefab != null)
        {
            var noItem = Instantiate(noItemPrefab, contentParent);
            noItem.name = "NoItemPlaceholder";
        }
    }

    private void ClearNoItemView()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            var child = contentParent.GetChild(i);
            if (child.name.StartsWith("NoItem"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}