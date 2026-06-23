using DG.Tweening;
using UnityEngine;
using JoonyleGameDevKit;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class FloatingEnemyBehaviour : EnemyBehaviour
{
    public override EnemyType EnemyType => EnemyType.Floating;

    private StateMachine<FloatingEnemyBehaviour> _fsm;

    [SerializeField] private float _floatHeight = 1f;
    [SerializeField] private float _floatCycleDuration = 2f;
    [Range(0f, 1f)] [SerializeField] private float _cycleVariance = 0.2f;
    [SerializeField] private bool _moveHorizontal = false;
    [SerializeField] private Ease _floatEase = Ease.InOutExpo;
    [SerializeField] private LineRenderer _patrolLine;

    private float _actualCycleDuration;

    public float FloatHeight => _floatHeight;
    public float FloatCycleDuration => _actualCycleDuration;
    public bool MoveHorizontal => _moveHorizontal;
    public Ease FloatEase => _floatEase;

    protected override void OnInitialize()
    {
        _actualCycleDuration = _floatCycleDuration * (1f + Random.Range(-_cycleVariance, _cycleVariance));

        _fsm = new StateMachine<FloatingEnemyBehaviour>(this);
        _fsm.AddState(new FloatingEnemyPatrolState());
        _fsm.ChangeState<FloatingEnemyPatrolState>();

        var center = transform.position;
        _patrolLine.positionCount = 2;
        var direction = _moveHorizontal ? Vector3.right : Vector3.up;
        _patrolLine.SetPosition(0, center - direction * (_floatHeight * 0.5f));
        _patrolLine.SetPosition(1, center + direction * (_floatHeight * 0.5f));
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

        base.Tick(deltaTime);

        _fsm?.Update(deltaTime);
    }

    public void ChangeState<TState>() where TState : StateBase<FloatingEnemyBehaviour> => _fsm.ChangeState<TState>();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var center = transform.position;
        var direction = _moveHorizontal ? Vector3.right : Vector3.up;
        var pointA = center - direction * (_floatHeight * 0.5f);
        var pointB = center + direction * (_floatHeight * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.15f);
        Gizmos.DrawWireSphere(pointB, 0.15f);
    }
#endif
}
