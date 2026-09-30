using System;
using UnityEditor;
using UnityEngine;

public sealed class DialogueWorkbookPostprocessor : AssetPostprocessor
{
    private static bool queued;
    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (queued || Array.IndexOf(imported, DialogueWorkbookImporter.Source) < 0) return;
        queued = true;
        EditorApplication.delayCall += () =>
        {
            queued = false;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            try { DialogueWorkbookImporter.Import(); }
            catch (Exception error) { Debug.LogError("剧情表导入失败，保留上次成功的数据：" + error.Message); }
        };
    }
}
