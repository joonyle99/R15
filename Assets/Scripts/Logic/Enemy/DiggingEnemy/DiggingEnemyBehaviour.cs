using DG.Tweening;
using UnityEngine;
using JoonyleGameDevKit;

[RequireComponent(typeof(Rigidbody2D))]
public class DiggingEnemyBehaviour : EnemyBehaviour
{
    public override EnemyType EnemyType => EnemyType.Digging;

    private StateMachine<DiggingEnemyBehaviour> _fsm;
    [SerializeField] private Collider2D _collider;
    public Collider2D Collider => _collider;

    [SerializeField] private float _emergeSpeed = 13f;
    [SerializeField] private float _returnSpeed = 14f;
    [SerializeField] private float _emergeHeight = 14f;
    [SerializeField] private float _idleInterval = 1f;
    [Range(0f, 1f)] [SerializeField] private float _idleVariance = 0.2f;
    [SerializeField] private float _warningDuration = 1f;
    [SerializeField] private Ease _emergeEase = Ease.OutQuart;
    [SerializeField] private Ease _returnEase = Ease.InQuart;
    [SerializeField] private LineRenderer _emergeLine;

    private float _actualIdleInterval;

    public float EmergeSpeed => _emergeSpeed;
    public float ReturnSpeed => _returnSpeed;
    public float EmergeHeight => _emergeHeight;
    public float IdleInterval => _actualIdleInterval;
    public float WarningDuration => _warningDuration;
    public Ease EmergeEase => _emergeEase;
    public Ease ReturnEase => _returnEase;

    protected override void OnInitialize()
    {
        _actualIdleInterval = _idleInterval * (1f + Random.Range(-_idleVariance, _idleVariance));

        _fsm = new StateMachine<DiggingEnemyBehaviour>(this);
        _fsm.AddState(new DiggingEnemyIdleState());
        _fsm.AddState(new DiggingEnemyWarningState());
        _fsm.AddState(new DiggingEnemyEmergeState());
        _fsm.ChangeState<DiggingEnemyIdleState>();
        
        var bottom = transform.position;
        _emergeLine.positionCount = 2;
        _emergeLine.SetPosition(0, bottom);
        _emergeLine.SetPosition(1, bottom + Vector3.up * _emergeHeight);
    }

    public override void FixedTick(float fixedDeltaTime)
    {
        if (IsDead) return;

        base.FixedTick(fixedDeltaTime);

        _fsm?.FixedUpdate(fixedDeltaTime);
    }

    public override void Tick(float deltaTime)
    {
        if (IsDead) return;

        _fsm?.Update(deltaTime);

        base.Tick(deltaTime);
    }

    public void ChangeState<TState>() where TState : StateBase<DiggingEnemyBehaviour> => _fsm.ChangeState<TState>();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var bottom = transform.position;
        var top = bottom + Vector3.up * _emergeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(bottom, top);
        Gizmos.DrawWireSphere(top, 0.15f);
    }
#endif
}
