using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour
{
    [SerializeField]
    CompatView _compatView;

    [SerializeField]
    ARPose _arPose;

    Vector3 _initialPosition;
    Quaternion _initialRotation;

    #if UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "getMobileType")]
    static extern int GetMobileType();
    #endif

    void Awake()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (GetMobileType() == 0) _compatView.ShowFrame();
        #endif

        _initialRotation = Quaternion.identity;
        _initialPosition = Vector3.zero;

        _arPose.onChanged += (position, rotation) => {
            _initialPosition = Vector3.zero;
            _initialRotation = rotation;
        };
    }

    void OnDestroy()
    {
        var latest = new PreloadSnapshot()
        {
            initialPosition = _initialPosition,
            initialRotation = _initialRotation,
        };
        PreloadSnapshot.latest = latest;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 40), "Start"))
        {
            SceneManager.LoadScene("Game/Scenes/Main", LoadSceneMode.Single);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game/Scenes/Main", LoadSceneMode.Single);
    }
}
