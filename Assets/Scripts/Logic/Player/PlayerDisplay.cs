using TMPro;
using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;

public class PlayerDisplay : MonoBehaviour
{
    [SerializeField] private ChargeAbsorbVfx _chargeAbsorbVfxPrefab;
    [SerializeField] private SpriteRenderer _noChargeIcon;
    [SerializeField] private TextMeshPro _combo;

    [SerializeField] private TextMeshPro _toast;
    [SerializeField] private float _toastRiseDistance = 0.6f;
    [SerializeField] private float _toastDuration = 0.8f;

    [SerializeField] private float _chargeSpacing = 0.3f;
    [SerializeField] private float _chargeYOffset = 0f;
    [SerializeField] private float _noChargeIconFadeDuration = 0.25f;

    private SlingBehaviour _slingBehaviour;
    private ComboSystem _comboSystem;
    private Func<bool> _isComboRushActive;
    private Tween _noChargeIconTween;
    private Tween _toastTween;
    private Vector3 _toastStartLocalPos;
    private bool _isAimPreview;
    private bool _isAimPreviewFree;
    private List<SpriteRenderer> _charges = new();
    private ObjectPool<ChargeAbsorbVfx> _chargeAbsorbVfxPool;
    private int _pendingCharges;
    private int _prevCharges;

    private void OnDestroy()
    {
        if (_slingBehaviour != null) _slingBehaviour.OnChargesChanged -= OnChargesChanged;
        if (_comboSystem != null) _comboSystem.OnComboChanged -= OnComboChanged;
    }

    public void Initialize(SlingBehaviour slingBehaviour, ComboSystem comboSystem, Func<bool> isComboRushActive)
    {
        _slingBehaviour = slingBehaviour;
        _comboSystem = comboSystem;
        _isComboRushActive = isComboRushActive;

        _slingBehaviour.OnChargesChanged += OnChargesChanged;
        _comboSystem.OnComboChanged += OnComboChanged;

        _combo.gameObject.SetActive(false);
        var color = _noChargeIcon.color;
        color.a = 0f;
        _noChargeIcon.color = color;

        if (_toast != null)
        {
            _toastStartLocalPos = _toast.transform.localPosition;
            _toast.alpha = 0f;
        }

        _prevCharges = slingBehaviour.CurrCharges;
        SpawnChargeIcons(slingBehaviour.TotalCharges);
        RefreshDisplay();

        if (_chargeAbsorbVfxPrefab != null)
        {
            _chargeAbsorbVfxPool = new ObjectPool<ChargeAbsorbVfx>(
                createFunc: () =>
                {
                    var vfx = Instantiate(_chargeAbsorbVfxPrefab);
                    vfx.SetEnabledTrail(true);
                    vfx.SetReleaseAction(() => _chargeAbsorbVfxPool.Release(vfx));
                    vfx.gameObject.SetActive(false);
                    return vfx;
                },
                actionOnGet: v => v.gameObject.SetActive(true),
                actionOnRelease: v => v.gameObject.SetActive(false),
                actionOnDestroy: v => { if (v != null) Destroy(v.gameObject); },
                collectionCheck: false,
                defaultCapacity: 4,
                maxSize: 20
            );
        }
    }

    private void SpawnChargeIcons(int total)
    {
        foreach (var icon in _charges)
            Destroy(icon.gameObject);
        _charges.Clear();

        for (int i = 0; i < total; i++)
        {
            var vfx = Instantiate(_chargeAbsorbVfxPrefab, transform);
            var sr = vfx.GetComponent<SpriteRenderer>();
            _charges.Add(sr);
        }
    }

    // ====================================

    private void OnChargesChanged(int curr, int max)
    {
        if (max != _charges.Count)
            SpawnChargeIcons(max);

        if (curr < _prevCharges)
            _pendingCharges = Mathf.Max(0, _pendingCharges - (_prevCharges - curr));
        _prevCharges = curr;

        RefreshDisplay();
    }

    private void OnComboChanged(int currCombo, int maxCombo)
    {
        _combo.gameObject.SetActive(currCombo > 0);
        _combo.text = currCombo.ToString();
    }

    public void BeginAimPreview(bool isFree = false)
    {
        _isAimPreview = true;
        _isAimPreviewFree = isFree;
        RefreshDisplay();
    }

    public void EndAimPreview()
    {
        _isAimPreview = false;
        _isAimPreviewFree = false;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        var curr = _slingBehaviour.CurrCharges;
        var isPreview = _isAimPreview && !_isAimPreviewFree && !(_isComboRushActive?.Invoke() ?? false);
        var displayCurr = isPreview ? Mathf.Max(0, curr - 1) : curr;

        displayCurr = Mathf.Max(0, displayCurr - _pendingCharges);
        var offset = (displayCurr - 1) * _chargeSpacing * 0.5f;
        for (int i = 0; i < _charges.Count; i++)
        {
            bool visible = i < displayCurr;
            _charges[i].gameObject.SetActive(visible);
            if (visible)
                _charges[i].transform.localPosition = new Vector3(i * _chargeSpacing - offset, _chargeYOffset);
        }
    }

    public void PlayChargeAbsorb(Vector2 from, Vector2 velocity, Action onArrived)
    {
        _pendingCharges++;
        Func<Vector2> targetPosGetter = () => GetChargeSlotWorldPos(_slingBehaviour.CurrCharges);
        _chargeAbsorbVfxPool?.Get().Play(from, targetPosGetter, velocity, () =>
        {
            _pendingCharges = Mathf.Max(0, _pendingCharges - 1);
            RefreshDisplay();
            onArrived?.Invoke();
        });
        RefreshDisplay();
    }

    private Vector2 GetChargeSlotWorldPos(int slotIndex)
    {
        var totalCurr = _slingBehaviour.CurrCharges;
        var offset = (totalCurr - 1) * _chargeSpacing * 0.5f;
        var localX = slotIndex * _chargeSpacing - offset;
        return (Vector2)transform.TransformPoint(new Vector3(localX, _chargeYOffset));
    }

    public void ShowComboRushEffect()
    {
        if (_toast == null) return;

        _toastTween?.Kill(true);
        _toast.text = "COMBO RUSH!";
        _toast.alpha = 1f;
        _toast.transform.localPosition = _toastStartLocalPos;

        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.Append(_toast.transform
            .DOLocalMoveY(_toastStartLocalPos.y + _toastRiseDistance, _toastDuration)
            .SetEase(Ease.OutQuad));
        seq.Join(_toast
            .DOFade(0f, _toastDuration * 0.5f)
            .SetDelay(_toastDuration * 0.5f)
            .SetEase(Ease.InQuad));
        _toastTween = seq;
    }

    public void ShowNoChargeEffect()
    {
        StartCoroutine(NoChargePauseRoutine());

        _noChargeIconTween?.Kill();
        var color = _noChargeIcon.color;
        color.a = 1f;
        _noChargeIcon.color = color;
        _noChargeIconTween = _noChargeIcon
            .DOFade(0f, _noChargeIconFadeDuration)
            .SetDelay(_slingBehaviour.Config.noChargePauseDuration)
            .SetUpdate(true)
            .SetLink(gameObject);

        if (_toast != null)
        {
            _toastTween?.Kill(true);
            _toast.text = $"임시 텍스트";
            _toast.alpha = 1f;
            _toast.transform.localPosition = _toastStartLocalPos;

            var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            seq.Append(_toast.transform
                .DOLocalMoveY(_toastStartLocalPos.y + _toastRiseDistance, _toastDuration)
                .SetEase(Ease.OutQuad));
            seq.Join(_toast
                .DOFade(0f, _toastDuration * 0.5f)
                .SetDelay(_toastDuration * 0.5f)
                .SetEase(Ease.InQuad));
            _toastTween = seq;
        }
    }

    private IEnumerator NoChargePauseRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(_slingBehaviour.Config.noChargePauseDuration);
        Time.timeScale = 1f;
    }
}
