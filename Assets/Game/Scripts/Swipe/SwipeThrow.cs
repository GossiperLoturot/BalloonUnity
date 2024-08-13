using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class SwipeThrow : MonoBehaviour
{
    public float holdAreaMargin = 0.5f;

    public float holdAreaAngle = 60.0f;

    public float velocityScale = 2.0f;

    public float minVelocity = 6.0f;

    public GameObject holdObjectPrefab;

    public event Action<Vector3, Vector3> onRelease;

    SwipeThrowInputAction _inputAction;
    SwipeState _state;
    IList<TimeVector3> _timePositions;
    GameObject _holdObject;

    void Awake()
    {
        _inputAction = new SwipeThrowInputAction();
        _inputAction.Default.Enable();
        _inputAction.Default.Touch.performed += OnTouchPerformed;
        _inputAction.Default.Point.performed += OnPointPerformed;
        _inputAction.Default.Touch.canceled += OnTouchComplete;

        _state = SwipeState.Ready;

        _timePositions = new List<TimeVector3>();
    }

    void Update()
    {
        if (_timePositions.Count != 0)
        {
            var localPosition = _timePositions[^1].vector;
            var position = Camera.main.transform.TransformPoint(localPosition);

            if (_holdObject == null) _holdObject = Instantiate(holdObjectPrefab);
            _holdObject.transform.position = position;
        }
        else
        {
            Destroy(_holdObject);
            _holdObject = null;
        }
    }

    // スワイプ軌道の表示
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_timePositions == null || _timePositions.Count == 0) return;

        foreach (var timePosition in _timePositions)
        {
            var localPosition = timePosition.vector;
            var position = Camera.main.transform.TransformPoint(localPosition);

            Gizmos.DrawSphere(position, 0.01f);
            UnityEditor.Handles.Label(position, string.Format("{0}, {1}", timePosition.time, timePosition.vector));
        }

        for (var i = 1; i < _timePositions.Count; i++)
        {
            var localPosition1 = _timePositions[i - 1].vector;
            var position1 = Camera.main.transform.TransformPoint(localPosition1);

            var localPosition2 = _timePositions[i].vector;
            var position2 = Camera.main.transform.TransformPoint(localPosition2);

            Gizmos.DrawLine(position1, position2);
        }
    }
    #endif

    /// <summary>
    /// スワイプ軌道から発射体加速度を計算し、シーン上に配置する関数
    /// </summary>
    void ReleaseProjectile()
    {
        if (_timePositions.Count < 2) return;

        // 速度の数値計算 (初期値は0と仮定)
        var velocities = new List<Vector3>();
        for (var i = 1; i < _timePositions.Count - 1; i++)
        {
            var deltaTime = _timePositions[i].time - _timePositions[i - 1].time;
            var deltaPosition = _timePositions[i].vector - _timePositions[i - 1].vector;
            var localVelocity = deltaPosition / deltaTime;

            if (deltaTime == 0) continue;

            velocities.Add(localVelocity);
        }

        if (velocities.Count == 0) return;

        var position = Camera.main.transform.TransformPoint(_timePositions[^1].vector);
        var velocity = Camera.main.transform.TransformVector(velocities[^1]) * velocityScale;
        
        // 最低速度の保証
        if (velocity.magnitude < minVelocity) velocity *= minVelocity / velocity.magnitude;
        
        onRelease?.Invoke(position, velocity);
    }

    // スクリーン押下開始
    void OnTouchPerformed(InputAction.CallbackContext cx)
    {
        if (_state == SwipeState.Released)
        {
            _state = SwipeState.Ready;
            return;
        }
    }

    // スクリーン押下位置変更時
    void OnPointPerformed(InputAction.CallbackContext cx)
    {
        // スクリーン押下開始
        if (_state == SwipeState.Ready)
        {
            var point = cx.ReadValue<Vector2>();

            var ray = Camera.main.ScreenPointToRay(point);
            var position = ray.origin + ray.direction * holdAreaMargin;
            var localPosition = Camera.main.transform.InverseTransformPoint(position);

            _timePositions.Add(new TimeVector3(Time.time, localPosition));

            _state = SwipeState.Hold;
            return;
        }

        // スクリーン押下中
        if (_state == SwipeState.Hold)
        {
            var point = cx.ReadValue<Vector2>();

            var ray = Camera.main.ScreenPointToRay(point);
            var position = ray.origin + ray.direction * holdAreaMargin;
            var localPosition = Camera.main.transform.InverseTransformPoint(position);

            // 視線方向に傾ける
            var rotation = Quaternion.Euler(holdAreaAngle, 0, 0);
            var localPosition0 = _timePositions[0].vector;
            var finalLocalPosition = rotation * (localPosition - localPosition0) + localPosition0;

            _timePositions.Add(new TimeVector3(Time.time, finalLocalPosition));

            _state = SwipeState.Hold;
            return;
        }
    }

    // スクリーン押下終了
    void OnTouchComplete(InputAction.CallbackContext cx)
    {
        if (_state == SwipeState.Hold)
        {
            ReleaseProjectile();

            _timePositions.Clear();

            _state = SwipeState.Released;
            return;
        }
    }

    enum SwipeState
    {
        Ready,
        Hold,
        Released,
    }

    public struct TimeVector3
    {
        public float time;
        public Vector3 vector;

        public TimeVector3(float time, Vector3 vector)
        {
            this.time = time;
            this.vector = vector;
        }
    }
}