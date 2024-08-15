using Fusion.Addons.Physics;
using Fusion.Sockets;
using Fusion;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    WaittingFrame _networkWaittingFrame;

    [SerializeField]
    WaittingFrame _matchWaittingFrame;

    [SerializeField]
    ReadyFrame _readyFrame;

    [SerializeField]
    HUDFrame _hudFrame;

    [SerializeField]
    HitFrame _cyanHitFrame;

    [SerializeField]
    HitFrame _orangeHitFrame;

    [SerializeField]
    List<Transform> _spawnOriginPool;

    [SerializeField]
    NetworkPrefabRef _playerPrefab;

    [SerializeField]
    NetworkPrefabRef _serverSingletonPrefab;

    [SerializeField]
    GameObject _clientSingletonPrefab;

    NetworkRunner _runner;
    Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    Dictionary<PlayerRef, Transform> _spawnedOrigins = new();
    Queue<Transform> _spawnOriginQueue;
    GameObject _clientSingleton;

    bool _isRegistered;
    int _prevSelfScore;
    int _prevOtherScore;

    Vector3 _position;
    Quaternion _rotation;
    bool _releaseButton;
    Vector3 _releasePosition;
    Vector3 _releaseVelocity;

    void Awake()
    {
        _networkWaittingFrame.ShowFrame();
        _matchWaittingFrame.HideFrame();
        _readyFrame.HideFrame();
        _hudFrame.HideFrame();

        _spawnOriginQueue = new(_spawnOriginPool);

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        gameObject.AddComponent<RunnerSimulatePhysics3D>();

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    void Update()
    {
        if (Leaderboard.current == null) return;

        // 初回のNetworkGameState適用
        if (Leaderboard.current.GetState() == NetworkGameState.Waiting)
        {
            _networkWaittingFrame.HideFrame();
            _matchWaittingFrame.ShowFrame();
            _readyFrame.HideFrame();
            _hudFrame.HideFrame();
        }
        if (Leaderboard.current.GetState() == NetworkGameState.Ready)
        {
            _networkWaittingFrame.HideFrame();
            _matchWaittingFrame.HideFrame();
            _readyFrame.ShowFrame();
            _hudFrame.HideFrame();
        }
        else if (Leaderboard.current.GetState() == NetworkGameState.Running)
        {
            _networkWaittingFrame.HideFrame();
            _matchWaittingFrame.HideFrame();
            _readyFrame.HideFrame();
            _hudFrame.ShowFrame();
        }

        // コールバックを登録
        if (_isRegistered) return;
        Leaderboard.current.onStateChanged += () => {
            if (Leaderboard.current.GetState() == NetworkGameState.Ready)
            {
                _networkWaittingFrame.HideFrame();
                _matchWaittingFrame.HideFrame();
                _readyFrame.ShowFrame();
                _hudFrame.HideFrame();
            }
            else if (Leaderboard.current.GetState() == NetworkGameState.Running)
            {
                _networkWaittingFrame.HideFrame();
                _matchWaittingFrame.HideFrame();
                _readyFrame.HideFrame();
                _hudFrame.ShowFrame();
            }
        };
        Leaderboard.current.onReadyTimeChanged += () => {
            _readyFrame.SetRemainTime(Leaderboard.current.maxReadyTime - Leaderboard.current.GetReadyTime());
        };
        Leaderboard.current.onRunningTimeChanged += () => {
            _hudFrame.SetRemainTime(Leaderboard.current.maxRunningTime - Leaderboard.current.GetRunningTime());
        };
        Leaderboard.current.onScoreChanged += () => {
            var selfScore = Leaderboard.current.GetScore(_runner.LocalPlayer);
            _hudFrame.SetSelfScore(selfScore);
            if (selfScore != _prevSelfScore)
            {
                _cyanHitFrame.ShowFrame();
                _prevSelfScore = selfScore;
            }

            var otherScore = Leaderboard.current.GetOtherScore(_runner.LocalPlayer);
            _hudFrame.SetOtherScore(otherScore);
            if (otherScore != _prevOtherScore)
            {
                _orangeHitFrame.ShowFrame();
                _prevOtherScore = otherScore;
            }
        };
        _isRegistered = true;
    }

    void OnGUI()
    {
        if (_runner == null) return;

        var isRunning = _runner.IsRunning;
        GUI.TextField(new Rect(0, Screen.height - 100, 200, 100), $"IsRunning: {isRunning}");
    }
 
    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnInput(NetworkRunner runner, Fusion.NetworkInput input)
    {
        var data = new NetworkInputData();

        data.position = _position;
        data.rotation = _rotation;

        data.buttons.Set(NetworkInputData.RELEASE_BUTTON, _releaseButton);
        _releaseButton = false;

        data.releasePosition = _releasePosition;
        data.releaseVelocity = _releaseVelocity;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, Fusion.NetworkInput input) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            var spawnOrigin = _spawnOriginQueue.Dequeue();
            var networkPlayerObject = runner.Spawn(_playerPrefab, spawnOrigin.position, spawnOrigin.rotation, player);

            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            _spawnedOrigins.Add(player, spawnOrigin);

            Leaderboard.current.JoinPlayer(player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out var networkObject))
        {
            Leaderboard.current.LeftPlayer(player);

            _spawnedCharacters.Remove(player);

            runner.Despawn(networkObject);
        }

        if (_spawnedOrigins.TryGetValue(player, out var origin))
        {
            _spawnedOrigins.Remove(player);

            _spawnOriginQueue.Enqueue(origin);
        }
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.IsServer)
        {
            var networkObject = runner.Spawn(_serverSingletonPrefab);

            Debug.Assert(networkObject.GetComponent<Leaderboard>() != null);
        }

        _clientSingleton = Instantiate(_clientSingletonPrefab);

        var preload = PreloadSnapshot.latest;
        var correctionPosition = -preload.initialPosition;
        var dir = preload.initialRotation * Vector3.forward;
        var correctionRotation = Quaternion.Inverse(Quaternion.LookRotation(Vector3.Scale(dir, new Vector3(1, 0, 1))));

        var arPose = _clientSingleton.GetComponent<ARPose>();
        arPose.onChanged += (position, rotation) => {
            _position = position + correctionPosition;
            _rotation = correctionRotation * rotation;
        };
        
        var swipeThrow = _clientSingleton.GetComponent<SwipeThrow>();
        swipeThrow.onRelease += (position, velocity) => {
            _releaseButton = true;
            _releasePosition = position;
            _releaseVelocity = velocity;
        };
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        var main = new MainSnapshot()
        {
            state = Leaderboard.current.GetState(),
            maxRunningTime = Leaderboard.current.maxRunningTime,
            runningTime = Leaderboard.current.GetRunningTime(),
            selfScore = Leaderboard.current.GetScore(runner.LocalPlayer),
            otherScore = Leaderboard.current.GetOtherScore(runner.LocalPlayer),
        };
        MainSnapshot.latest = main;

        Destroy(_clientSingleton);

        SceneManager.LoadScene("Game/Scenes/Post", LoadSceneMode.Single);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
