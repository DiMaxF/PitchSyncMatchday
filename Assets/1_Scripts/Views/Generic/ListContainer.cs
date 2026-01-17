using System;
using System.Collections.Generic;
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

    private readonly List<UIView> _activeItems = new List<UIView>();
    private readonly Stack<UIView> _pooledItems = new Stack<UIView>();
    private CompositeDisposable _collectionDisposables;
    private bool _isInitialized = false;

    public new void Init(object data)
    {
        if (data is ReactiveCollection<object> collection)
        {
            if (_isInitialized && DataProperty.Value == collection) return;

            DataProperty.Value = collection;
            InitializeFromCollection(collection);
            _isInitialized = true;
        }
    }

    private void InitializeFromCollection(ReactiveCollection<object> collection)
    {
        _collectionDisposables?.Clear();
        _collectionDisposables = new CompositeDisposable();

        if (!_isInitialized)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }

        collection.ObserveAdd().Subscribe(e =>
        {
            OnItemAdded(e.Index, e.Value);
            UpdateNoItemView();
        }).AddTo(_collectionDisposables);
        collection.ObserveRemove().Subscribe(e => OnItemRemoved(e.Index, e.Value)).AddTo(_collectionDisposables);
        collection.ObserveReset().Subscribe(_ => OnCollectionReset()).AddTo(_collectionDisposables);
        collection.ObserveMove().Subscribe(e => OnItemMoved(e.OldIndex, e.NewIndex)).AddTo(_collectionDisposables);
        collection.ObserveReplace().Subscribe(e => OnItemReplaced(e.Index, e.NewValue)).AddTo(_collectionDisposables);

        ClearAllItems();

        for (int i = 0; i < collection.Count; i++)
        {
            OnItemAdded(i, collection[i]);
        }

        UpdateNoItemView();
    }

    private void OnItemAdded(int index, object itemData)
    {
        if (itemPrefab == null) return;

        var instance = GetOrCreateItem();
        instance.transform.SetParent(contentParent, false);
        instance.transform.SetSiblingIndex(index);
        instance.gameObject.SetActive(false);

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

        _activeItems.Insert(index, instance);

        AnimateItemAppear(instance, index).Forget();
    }

    private async UniTask AnimateItemAppear(UIView item, int index)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(index * spawnDelayPerItem),
            cancellationToken: this.GetCancellationTokenOnDestroy());
        if (item != null)
        {
            await item.ShowAsync();
        }
    }

    private async void OnItemRemoved(int index, object itemData)
    {
        if (index < 0 || index >= _activeItems.Count) return;

        var item = _activeItems[index];
        _activeItems.RemoveAt(index);

        if (animateRemoval)
        {
            await item.HideAsync();
        }

        item.gameObject.SetActive(false);
        _pooledItems.Push(item);

        ReindexAfterRemoval(index);
        UpdateNoItemView();
    }

    private void OnItemReplaced(int index, object newData)
    {
        if (index < 0 || index >= _activeItems.Count) return;
        var item = _activeItems[index];
        if (item != null)
        {
            var dataPropertyField = item.GetType().BaseType?.GetField("DataProperty",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dataPropertyField != null)
            {
                var dataProperty = dataPropertyField.GetValue(item);
                var valueProperty = dataProperty?.GetType().GetProperty("Value");
                if (valueProperty != null && newData != null)
                {
                    valueProperty.SetValue(dataProperty, newData);
                }
            }
        }
    }

    private void OnItemMoved(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= _activeItems.Count) return;
        if (newIndex < 0 || newIndex >= _activeItems.Count) return;

        var item = _activeItems[oldIndex];
        _activeItems.RemoveAt(oldIndex);
        _activeItems.Insert(newIndex, item);
        UpdateSiblingIndices();
    }

    private void ReindexAfterRemoval(int removedIndex)
    {
        for (int i = removedIndex; i < _activeItems.Count; i++)
        {
            _activeItems[i].transform.SetSiblingIndex(i);
        }
    }

    private void UpdateSiblingIndices()
    {
        for (int i = 0; i < _activeItems.Count; i++)
        {
            _activeItems[i].transform.SetSiblingIndex(i);
        }
    }

    private void OnCollectionReset()
    {
        ClearAllItems();
        if (DataProperty.Value != null)
        {
            for (int i = 0; i < DataProperty.Value.Count; i++)
            {
                OnItemAdded(i, DataProperty.Value[i]);
            }
        }
        UpdateNoItemView();
    }

    private void ClearAllItems()
    {
        foreach (var item in _activeItems)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
                _pooledItems.Push(item);
            }
        }
        _activeItems.Clear();
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

    private UIView GetOrCreateItem()
    {
        if (_pooledItems.Count > 0)
        {
            return _pooledItems.Pop();
        }

        var instance = Instantiate(itemPrefab, contentParent);
        UIManager.RegisterView(instance);
        UIManager.ForwardEventsFrom(instance, this).AddTo(this);
        return instance;
    }
}
 
