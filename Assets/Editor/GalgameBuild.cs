using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

public static class GalgameBuild
{
    public static void VerifyWindowsBuild()
    {
        WorkbookSceneBuilder.Prepare();
        OriginalSceneValidation.Validate();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = "Builds/Windows/ZZVan.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        bool success = report.summary.result == BuildResult.Succeeded;
        Debug.Log(success ? "GALGAME_PLAYER_BUILD_PASSED" : "GALGAME_PLAYER_BUILD_FAILED");
        if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
    }
}
