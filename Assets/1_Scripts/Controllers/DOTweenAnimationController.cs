using DG.Tweening;

public class DOTweenAnimationController : IAnimationController
{
    private Sequence _animation;

    public void StartAnimation()
    {
        StopAnimation();
        _animation = DOTween.Sequence();
    }

    public void StopAnimation()
    {
        _animation?.Kill();
        _animation = null;
    }

    public bool HasActiveAnimation => _animation != null && _animation.IsActive();

    public Sequence GetSequence() => _animation;
}