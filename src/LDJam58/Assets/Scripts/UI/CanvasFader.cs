using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : OnMessage<UiFadeInRequested, UiFadeOutRequested>
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _startFadedOut = false;

    private void Start()
    {
        _canvasGroup.alpha = _startFadedOut ? 1 : 0;
    }

    protected override void Execute(UiFadeInRequested msg)
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 1;
        _canvasGroup.DOFade(0, msg.Duration).SetDelay(msg.Delay);
    }

    protected override void Execute(UiFadeOutRequested msg)
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, msg.Duration).SetDelay(msg.Delay);
    }
}
