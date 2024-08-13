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

    Vector3 _position;
    Quaternion _rotation;
    bool _releaseButton;
    Vector3 _releasePosition;
    Vector3 _releaseVelocity;

    void Awake()
    {
        _spawnOriginQueue = new(_spawnOriginPool);
    }

    void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }

            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    async void StartGame(GameMode mode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        gameObject.AddComponent<RunnerSimulatePhysics3D>();

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
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

        var arPose = _clientSingleton.GetComponent<ARPose>();
        arPose.onChanged += (position, rotation) => {
            _position = position;
            _rotation = rotation;
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
        Destroy(_clientSingleton);

        var o = new GameObject("Leaderboard Snapshot");

        var snapshot = o.AddComponent<LeaderboardSnapshot>();
        snapshot.state = Leaderboard.current.GetState();
        snapshot.maxRunningTime = Leaderboard.current.maxRunningTime;
        snapshot.runningTime = Leaderboard.current.GetRunningTime();
        snapshot.selfScore = Leaderboard.current.GetScore(runner.LocalPlayer);
        snapshot.otherScore = Leaderboard.current.GetOtherScore(runner.LocalPlayer);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
