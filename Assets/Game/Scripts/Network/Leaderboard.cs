using System;
using System.Text;
using Fusion;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    public static Leaderboard current;

    public float maxReadyTime = 3;
    public float maxRunningTime = 60;

    [Networked]
    [OnChangedRender(nameof(OnChangeState))]
    NetworkGameState state { get; set; }

    [Networked]
    [OnChangedRender(nameof(OnChangeReadyTime))]
    float readyTime { get; set; }

    [Networked]
    [OnChangedRender(nameof(OnChangeRunningTime))]
    float runningTime { get; set; }

    [Networked]
    [Capacity(2)]
    [OnChangedRender(nameof(OnChangeScores))]
    NetworkDictionary<int, int> scores => default;

    public event Action onStateChanged;
    public event Action onReadyTimeChanged;
    public event Action onRunningTimeChanged;
    public event Action onScoreChanged;

    public override void Spawned()
    {
        if (current != null) throw new UnityException("Leaderboard must be one component in the scene");

        current = this;
    }

    // ゲームループ
    public override void FixedUpdateNetwork()
    {
        if (state == NetworkGameState.Ready)
        {
            if (readyTime >= maxReadyTime)
            {
                runningTime = 0;
                foreach (var (key, _) in scores)
                {
                    scores.Set(key, 0);
                }

                state = NetworkGameState.Running;
                return;
            }
            else
            {
                readyTime += Runner.DeltaTime;
                return;
            }
        }

        if (state == NetworkGameState.Running)
        {
            runningTime += Runner.DeltaTime;

            if (runningTime >= maxRunningTime)
            {
                Runner.Shutdown();

                state = NetworkGameState.Finished;
                return;
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        current = null;
    }

    // プレイヤー入退出

    public void JoinPlayer(PlayerRef player) 
    {
        scores.Add(player.PlayerId, default); 

        if (scores.Count == 2)
        {
            if (state == NetworkGameState.Waiting)
            {
                readyTime = 0;
                state = NetworkGameState.Ready;
                return;
            }
        }
    }

    public void LeftPlayer(PlayerRef player)
    {
        scores.Remove(player.PlayerId); 

        if (scores.Count != 2)
        {
            if (state == NetworkGameState.Ready || state == NetworkGameState.Running)
            {
                Runner.Shutdown();

                state = NetworkGameState.Interrupt;
                return;
            }
        }
    } 

    // ゲーム状態

    public NetworkGameState GetState() => state;

    void OnChangeState(NetworkBehaviourBuffer _) => onStateChanged?.Invoke(); 

    // ゲーム開始待機時間

    public float GetTime() => readyTime; 

    void OnChangeReadyTime(NetworkBehaviourBuffer _) => onReadyTimeChanged?.Invoke(); 

    // ゲーム中時間

    public float GetRunningTime() => runningTime; 

    void OnChangeRunningTime(NetworkBehaviourBuffer _) => onRunningTimeChanged?.Invoke(); 

    // スコア

    public void SetScore(PlayerRef player, int score) => scores.Set(player.PlayerId, score); 

    public int GetScore(PlayerRef player) => scores.Get(player.PlayerId);

    public int GetOtherScore(PlayerRef player) 
    {
        foreach (var (k, v) in scores)
        {
            if (k != player.PlayerId) return v;
        }
        throw new UnityException("not found other score");
    }

    void OnChangeScores(NetworkBehaviourBuffer _) => onScoreChanged?.Invoke(); 

    // デバッグ
    void OnGUI()
    {
        // Network Objectのスポーン待機
        if (current == null) return;

        var stateStr = state switch {
            NetworkGameState.Waiting => "WAITING",
            NetworkGameState.Ready => "READY",
            NetworkGameState.Running => "RUNNING",
            NetworkGameState.Interrupt => "INTERRUPT",
            NetworkGameState.Finished => "FINISHED",
            _ => "UNKNOWN"
        };

        var readyTimeStr = (maxReadyTime - readyTime).ToString();

        var runningTimeStr = (maxRunningTime - runningTime).ToString();

        var sb = new StringBuilder();
        foreach (var (key, score) in scores) sb.Append($"\n  player {key}: score {score}");
        var scoresStr = sb.ToString();

        GUI.TextArea(new Rect(200, 0, 400, 200), $"state: {stateStr}\nready: {readyTimeStr}\nrunning: {runningTimeStr}\nscore: {scoresStr}");
    }
}
