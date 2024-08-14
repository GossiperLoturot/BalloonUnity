using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class ARPose : MonoBehaviour
{
    public float accelSensitivity = 20;

    public float velocityResilience = 5;

    public float positionResilience = 2;

    public Vector3 positionScale = new Vector3(5, 1, 1);

    public Vector3 positionBounds = new Vector3(5, 1, 1);

    public event Action<Vector3, Quaternion> onChanged;

    #if UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "getMobileType")]
    static extern int GetMobileType();
    #endif

    ARPoseInputAction _inputAction;
    Vector3 _position;
    Quaternion _rotation;

    void Awake()
    {
        _inputAction = new ARPoseInputAction();
        _inputAction.Default.Enable();
        _inputAction.Default.Attitude.performed += OnAttitudePerformed;
        _inputAction.Default.Accel.performed += OnAccelPerformed;

        _position = Vector3.zero;
        _rotation = Quaternion.identity;
    }

    void Update()
    {
        if (AttitudeSensor.current != null && !AttitudeSensor.current.enabled)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }

        if (LinearAccelerationSensor.current != null && !LinearAccelerationSensor.current.enabled)
        {
            InputSystem.EnableDevice(LinearAccelerationSensor.current);
        }

        var finalPosition = Vector3.Scale(_position, positionScale);
        var finalRotation = _rotation;
        onChanged?.Invoke(finalPosition, finalRotation);
    }

    void OnAttitudePerformed(InputAction.CallbackContext cx)
    {
        var readValue = cx.ReadValue<Quaternion>();

        _rotation = AttitudeDeviceToCamera(readValue);
    }

    /// <summary>
    /// デバイスの姿勢からカメラの向きへのQuaternion変換 (Y軸回転補正なし)
    /// (参考: https://source.android.com/docs/core/interaction/sensors/sensor-types#rotation_vector)
    /// </summary>
    Quaternion AttitudeDeviceToCamera(Quaternion rotation)
    {
        #if UNITY_WEBGL
        return GetMobileType() switch {
            // Android
            1 => Quaternion.Euler(90, 0, 0) * new Quaternion(-rotation.x, -rotation.y, rotation.z, rotation.w),
            // iOS
            2 => Quaternion.Euler(90, 0, 0) * new Quaternion(-rotation.x, -rotation.y, rotation.z, rotation.w),
            // その他
            _ => Quaternion.identity,
        };
        #elif UNITY_ANDROID
        return Quaternion.Euler(90, 0, 0) * new Quaternion(-rotation.x, -rotation.y, rotation.z, rotation.w);
        #elif UNITY_IOS
        return Quaternion.Euler(90, 0, 0) * new Quaternion(-rotation.x, -rotation.y, rotation.z, rotation.w);
        #else
        return Quaternion.identity;
        #endif
    }

    // 前回OnAccelPerformedが実行された時刻
    float _prevTime;
    Vector3 _velocity;

    void OnAccelPerformed(InputAction.CallbackContext cx)
    {
        var deltaTime = (float)cx.time - _prevTime;

        var accel = AccelDeviceToCamera(cx.ReadValue<Vector3>());

        _velocity += accel * deltaTime * accelSensitivity;
        // exponential interpolation
        _velocity = Vector3.Lerp(_velocity, Vector3.zero, deltaTime * velocityResilience);

        _position += _velocity * deltaTime;
        // exponential interpolation
        _position = Vector3.Lerp(_position, Vector3.zero, deltaTime * positionResilience);

        _prevTime = (float)cx.time;
    }

    /// <summary>
    /// デバイスの直線加速度からカメラの加速度へのベクトル変換
    /// (参考: https://source.android.com/docs/core/interaction/sensors/sensor-types#linear_acceleration)
    /// </summary>
    Vector3 AccelDeviceToCamera(Vector3 accel)
    {
        return -accel;
    }
}
