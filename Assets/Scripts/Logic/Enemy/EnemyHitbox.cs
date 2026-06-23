using UnityEngine;

/// <summary>적의 타격 부위. 같은 적이라도 부위에 따라 처치/피격이 갈린다.</summary>
public enum EnemyHitPart
{
    Weakpoint, // 속살: 처치 가능한 약점
    Armor,     // 껍질: 단단한 방어 부위
}

/// <summary>
/// 적의 자식 콜라이더에 붙여 "어느 부위인지"를 표시하는 마커.
/// PlayerBehaviour.CheckHit이 이 컴포넌트로 부위를 구분해 처치/피격을 결정한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public sealed class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private EnemyHitPart _part;

    public EnemyHitPart Part => _part;
    public EnemyBehaviour Owner { get; private set; }

    private void Awake() => Owner = GetComponentInParent<EnemyBehaviour>();
}
