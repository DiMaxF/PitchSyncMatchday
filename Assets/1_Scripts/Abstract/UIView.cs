using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    public bool IsActive => gameObject.activeSelf;

    private CompositeDisposable _disposables = new CompositeDisposable();
    private bool _isSubscribed = false;

    protected virtual void Awake()
    {
        if (!_isSubscribed)
        {
            _isSubscribed = true;
            Subscribe();
        }
    }

    protected virtual void OnEnable()
    {
        UIManager.RegisterView(this);
        
        if (!_isSubscribed)
        {
            _isSubscribed = true;
            Subscribe();
        }
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void OnDestroy()
    {
        UIManager.UnregisterView(this);
        _disposables.Clear();
        _disposables.Dispose();
        _isSubscribed = false;
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