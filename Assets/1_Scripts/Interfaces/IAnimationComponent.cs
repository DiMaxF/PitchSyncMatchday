using DG.Tweening;
public interface IAnimationComponent 
{
    Tween AnimateShow();

    Tween AnimateHide();
    int Order { get; }
    bool IsParallel { get; }
}
