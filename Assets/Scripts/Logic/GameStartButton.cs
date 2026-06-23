using System;
using DG.Tweening;
using UnityEngine;

public class GameStartButton : MonoBehaviour
{
    [SerializeField] private Transform _visual; // 눌림 연출 대상 (비우면 자기 자신)
    [SerializeField] private float _pressDepth = 0.2f; // 눌리는 깊이 (로컬 Y)
    [SerializeField] private float _pressDuration = 0.08f;
    [SerializeField] private float _releaseDuration = 0.2f;

    private bool _pressed;
    private Action _onPressed;

    public void Initialize(Action onPressed)
    {
        _onPressed = onPressed;
    }

    public void Press()
    {
        if (_pressed) return;
        
        _pressed = true;

        var target = _visual != null ? _visual : transform;
        var baseY = target.localPosition.y;

        DOTween.Sequence()
            .Append(target.DOLocalMoveY(baseY - _pressDepth, _pressDuration).SetEase(Ease.OutQuad))
            .Append(target.DOLocalMoveY(baseY, _releaseDuration).SetEase(Ease.OutBack))
            .SetLink(gameObject)
            .OnComplete(() => _onPressed?.Invoke());
    }
}
