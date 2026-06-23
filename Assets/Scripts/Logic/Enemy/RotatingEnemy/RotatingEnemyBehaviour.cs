using UnityEngine;
using JoonyleGameDevKit;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class RotatingEnemyBehaviour : EnemyBehaviour
{
    public override EnemyType EnemyType => EnemyType.Rotating;

    private StateMachine<RotatingEnemyBehaviour> _fsm;

    [SerializeField] private float _rotateSpeed = 180f; // 초당 회전 각도(도)
    [SerializeField] private bool _clockwise = true;

    // 시계 방향이면 각도가 감소해야 하므로 부호를 뒤집는다
    public float AngularVelocity => _clockwise ? -_rotateSpeed : _rotateSpeed;

    protected override void OnInitialize()
    {
        _fsm = new StateMachine<RotatingEnemyBehaviour>(this);
        _fsm.AddState(new RotatingEnemyIdleState());
        _fsm.ChangeState<RotatingEnemyIdleState>();
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

    public void ChangeState<TState>() where TState : StateBase<RotatingEnemyBehaviour> => _fsm.ChangeState<TState>();
}
