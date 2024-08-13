using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte RELEASE_BUTTON = 1;

    public Vector3 position;
    public Quaternion rotation;

    public NetworkButtons buttons;
    public Vector3 releasePosition;
    public Vector3 releaseVelocity;
}
