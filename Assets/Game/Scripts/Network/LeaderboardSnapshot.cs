using UnityEngine;

public class LeaderboardSnapshot : MonoBehaviour
{
    public static LeaderboardSnapshot current;

    public NetworkGameState state;
    public float maxRunningTime;
    public float runningTime;
    public int selfScore;
    public int otherScore;

    void Awake()
    {
        if (current != null) throw new UnityException("Leaderboard must be one component in the scene");

        current = this;
    }

    void OnDestroy()
    {
        // シングルトンの生存期間はシーン上にゲームオブジェクトが生きている間だけ
        current = null;
    }

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

        var runningTimeStr = (maxRunningTime - runningTime).ToString();

        GUI.TextArea(new Rect(200, 0, 400, 200), $"state: {stateStr}\nnrunning: {runningTimeStr}\nself score: {selfScore}\nother score: {otherScore}");
    }
}
