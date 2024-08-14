using UnityEngine;
using UnityEngine.SceneManagement;

public class Post : MonoBehaviour
{
    // デバッグ
    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 200, 200, 40), "Start"))
        {
            SceneManager.LoadScene("Game/Scenes/Main", LoadSceneMode.Single);
        }

        var main = MainSnapshot.latest;

        // Network Objectのスポーン待機
        if (main == null) return;

        var stateStr = main.state switch {
            NetworkGameState.Waiting => "WAITING",
            NetworkGameState.Ready => "READY",
            NetworkGameState.Running => "RUNNING",
            NetworkGameState.Interrupt => "INTERRUPT",
            NetworkGameState.Finished => "FINISHED",
            _ => "UNKNOWN"
        };

        var runningTimeStr = (main.maxRunningTime - main.runningTime).ToString();

        GUI.TextArea(new Rect(0, 0, 400, 200), $"state: {stateStr}\nrunning: {runningTimeStr}\nself score: {main.selfScore}\nother score: {main.otherScore}");
    }
}
