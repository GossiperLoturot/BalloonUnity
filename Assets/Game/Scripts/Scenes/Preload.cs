using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour
{
    [SerializeField]
    ARPose _arPose;

    Vector3 _initialPosition;
    Quaternion _initialRotation;

    void Awake()
    {
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
}
