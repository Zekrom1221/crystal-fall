using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Проверка сцен и сборка из командной строки:
// Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
public static class BuildScript
{
    private const string LogPrefix = "[BuildScript]";
    private const string WindowsBuildPath = "Builds/Windows/CrystalFall.exe";

    [MenuItem("Tools/Crystal Fall/Validate Scenes")]
    private static void ValidateMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Validate();
    }

    [MenuItem("Tools/Crystal Fall/Build Windows")]
    public static void BuildWindows()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (!Validate())
        {
            Debug.LogError($"{LogPrefix} Validation failed, build aborted");
            ExitIfBatchMode(1);
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = GetBuildScenes(),
            locationPathName = WindowsBuildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
        Debug.Log($"{LogPrefix} Build {summary.result}: {summary.totalSize / (1024 * 1024)} MB, " +
                  $"{summary.totalErrors} errors, {summary.totalWarnings} warnings, {summary.totalTime}");

        ExitIfBatchMode(summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    public static void ValidateFromCommandLine()
    {
        ExitIfBatchMode(Validate() ? 0 : 1);
    }

    // true, если в сценах из Build Settings нет компонентов с потерянными скриптами.
    // Битые ссылки на ассеты только логируются: в клоне без сторонних паков они ожидаемы.
    private static bool Validate()
    {
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError($"{LogPrefix} No enabled scenes in Build Settings");
            return false;
        }

        string previousScene = SceneManager.GetActiveScene().path;
        int missingScripts = 0;
        int brokenReferences = 0;

        foreach (string scenePath in scenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if (missing > 0)
                    {
                        missingScripts += missing;
                        Debug.LogError($"{LogPrefix} Missing script x{missing}: {scenePath} -> {GetHierarchyPath(t)}");
                    }
                    brokenReferences += CountBrokenReferences(t.gameObject, scenePath);
                }
            }
        }

        // В редакторе возвращаем сцену, которая была открыта до проверки
        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScene))
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);

        Debug.Log($"{LogPrefix} Validate: missing scripts = {missingScripts}, broken references = {brokenReferences}");
        return missingScripts == 0;
    }

    private static int CountBrokenReferences(GameObject go, string scenePath)
    {
        int count = 0;
        foreach (Component component in go.GetComponents<Component>())
        {
            if (component == null) continue;

            var property = new SerializedObject(component).GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (property.objectReferenceValue != null || property.objectReferenceInstanceIDValue == 0) continue;

                count++;
                Debug.LogWarning($"{LogPrefix} Broken reference: {scenePath} -> {GetHierarchyPath(go.transform)} " +
                                 $"({component.GetType().Name}.{property.propertyPath})");
            }
        }
        return count;
    }

    private static string[] GetBuildScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }

    private static string GetHierarchyPath(Transform t)
    {
        return t.parent == null ? t.name : GetHierarchyPath(t.parent) + "/" + t.name;
    }

    private static void ExitIfBatchMode(int code)
    {
        if (Application.isBatchMode) EditorApplication.Exit(code);
    }
}
