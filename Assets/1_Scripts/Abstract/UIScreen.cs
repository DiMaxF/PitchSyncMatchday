using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEditor;
using UnityEngine;

public abstract class UIScreen : MonoBehaviour
{
    protected DataManager Data;
    protected ScreenManager ScreenManager;

    private CompositeDisposable _disposables = new CompositeDisposable();

    public void Init(DataManager data, ScreenManager screenManager)
    {
        Data = data;
        ScreenManager = screenManager;
    }

    protected virtual void OnEnable()
    {
        SubscribeToData();
    }

    protected virtual void OnDisable()
    {
        _disposables.Clear();
    }

    protected virtual void OnDestroy()
    {
        _disposables.Dispose();
    }

    protected virtual void SubscribeToData() { }

    protected IDisposable AddToDispose(IDisposable disposable)
    {
        _disposables.Add(disposable);
        return disposable;
    }

    
    public virtual async UniTask ShowAsync()
    {
        gameObject.SetActive(true);
        await AnimationPlayer.PlayAnimationsAsync(gameObject, true);
        RefreshViews(); 
    }

    public virtual async UniTask HideAsync()
    {
        await AnimationPlayer.PlayAnimationsAsync(gameObject, false);
        gameObject.SetActive(false);
    }

   
    public virtual void Show() => ShowAsync().Forget();
    public virtual void Hide() => HideAsync().Forget();

    
    protected virtual void RefreshViews()
    {

    }
}

