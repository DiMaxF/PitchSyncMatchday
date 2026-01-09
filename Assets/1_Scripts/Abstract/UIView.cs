using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    public bool IsActive => gameObject.activeSelf;

    private CompositeDisposable _disposables = new CompositeDisposable();

    protected virtual void OnEnable()
    {
        UIManager.RegisterView(this);
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        _disposables.Clear();
    }

    protected virtual void OnDestroy()
    {
        UIManager.UnregisterView(this);
        _disposables.Dispose();
    }

    public virtual void Init()
    {
        UpdateUI();
    }

    public virtual void UpdateUI() { }

    protected virtual void Subscribe() { }

    protected IDisposable AddToDispose(IDisposable disposable)
    {
        _disposables.Add(disposable);
        return disposable;
    }

    public virtual async UniTask ShowAsync()
    {
        gameObject.SetActive(true);
        await AnimationPlayer.PlayAnimationsAsync(gameObject, true);
    }

    public virtual async UniTask HideAsync()
    {
        await AnimationPlayer.PlayAnimationsAsync(gameObject, false);
        gameObject.SetActive(false);
    }

    public virtual void Show() => ShowAsync().Forget();
    public virtual void Hide() => HideAsync().Forget();
}