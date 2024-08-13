#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;

public static class UnityCI
{
    public static void Build()
    {
        var scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray();
        var options = new BuildPlayerOptions
        {
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            locationPathName = "Build",
            scenes = scenes,
        };

        if (!Directory.Exists("Build"))
        {
            Directory.CreateDirectory("Build");
        }

        BuildPipeline.BuildPlayer(options);
    }
}
#endif
