using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    Projectile _projectilePrefab;

    [SerializeField]
    Transform _cameraRef;

    Vector3 _initPosition;
    Quaternion _initRotation;

    public override void Spawned()
    {
        _initPosition = transform.position;
        _initRotation = transform.rotation;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            transform.position = _initRotation * data.position + _initPosition;
            transform.rotation = _initRotation * data.rotation;

            if (HasStateAuthority)
            {
                if (data.buttons.IsSet(NetworkInputData.RELEASE_BUTTON))
                {
                    Runner.Spawn(
                        _projectilePrefab,
                        data.releasePosition,
                        Quaternion.identity,
                        Object.InputAuthority,
                        (runner, o) => o.GetComponent<Projectile>().AddVelocity(data.releaseVelocity)
                    );
                }
            }

            if (HasInputAuthority)
            {
                Camera.main.transform.position = _cameraRef.position;
                Camera.main.transform.rotation = _cameraRef.rotation;
            }
        }
    }
}
