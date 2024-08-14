using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField]
    float _collisionRadius;

    [SerializeField]
    LayerMask _layerMask;

    [SerializeField]
    float _lifeTime;

    [Networked]
    TickTimer life { get; set; }

    RunnerSimulatePhysics3D _physicsSimulator;
    
    public override void Spawned()
    {
        life = TickTimer.CreateFromSeconds(Runner, _lifeTime);

        _physicsSimulator = Runner.GetComponent<RunnerSimulatePhysics3D>();
        _physicsSimulator.OnBeforeSimulate += OnBeforeSimulate;
    }

    public void AddVelocity(Vector3 velocity)
    {
        GetComponent<Rigidbody>().velocity = velocity;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _physicsSimulator.OnBeforeSimulate -= OnBeforeSimulate;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner)) Runner.Despawn(Object);
    }

    void OnBeforeSimulate()
    {
        var rigidbody = GetComponent<Rigidbody>();

        if (rigidbody.velocity.sqrMagnitude > 0)
        {
            Debug.DrawRay(
                rigidbody.position + rigidbody.velocity.normalized * _collisionRadius,
                rigidbody.velocity * _physicsSimulator.PhysicsSimulationDeltaTime,
                Color.yellow,
                _physicsSimulator.PhysicsSimulationDeltaTime
            );

            if (Runner.LagCompensation.Raycast(
                rigidbody.position + rigidbody.velocity.normalized * _collisionRadius,
                rigidbody.velocity,
                rigidbody.velocity.magnitude * _physicsSimulator.PhysicsSimulationDeltaTime,
                Object.InputAuthority,
                out var hit,
                _layerMask,
                HitOptions.None
            ))
            {
                var hitObject = hit.GameObject.GetComponentInParent<NetworkObject>();

                Debug.Log($"{System.DateTime.Now}: {Object.InputAuthority} -> {hitObject.InputAuthority}");

                if (hitObject.InputAuthority != Object.InputAuthority)
                {
                    var prev = Leaderboard.current.GetScore(Object.InputAuthority);
                    Leaderboard.current.SetScore(Object.InputAuthority, prev + 1);

                    if (Runner.IsServer) Runner.Despawn(Object);
                }
            }
        }
    }

    // Destroy when contact to collider
    void OnCollisionEnter()
    {
        if (Runner.IsServer) Runner.Despawn(Object);
    }
}
