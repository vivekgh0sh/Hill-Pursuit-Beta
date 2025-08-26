// --- CREATE NEW FILE: ReplaceCanvasInLevels.cs (must be inside an "Editor" folder) ---

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplaceCanvasInLevels
{
    // Path to the master Canvas prefab (drag your template prefab into this field in code).
    private const string CANVAS_PREFAB_PATH = "Assets/Prefabs/UI/Canvas.prefab";

    [MenuItem("Tools/Levels/Replace Canvas in All Levels")]
    public static void ReplaceCanvases()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("Operation cancelled by user.");
            return;
        }

        GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CANVAS_PREFAB_PATH);
        if (canvasPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", $"Could not find Canvas prefab at:\n{CANVAS_PREFAB_PATH}", "OK");
            return;
        }

        string originalScenePath = SceneManager.GetActiveScene().path;

        int fixedScenes = 0;
        int skippedScenes = 0;

        try
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled || !scene.path.Contains("Level_"))
                    continue;

                Debug.Log($"--- Processing Scene: {scene.path} ---");
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

                bool hasCorrectCanvas = false;

                // Find all canvases in scene
                Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (Canvas c in canvases)
                {
                    GameObject go = c.gameObject;
                    GameObject prefabSource = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(go);

                    // 🔥 CASE 1: Broken prefab reference
                    if (PrefabUtility.IsPartOfPrefabInstance(go) && prefabSource == null)
                    {
                        Debug.LogWarning($"[FIX] Removing broken Canvas prefab instance: {go.name}");
                        Object.DestroyImmediate(go);
                        continue;
                    }

                    // 🔥 CASE 2: Prefab asset missing from disk
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                    if (!string.IsNullOrEmpty(prefabPath) && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                    {
                        Debug.LogWarning($"[FIX] Removing Canvas with missing prefab asset at: {prefabPath}");
                        Object.DestroyImmediate(go);
                        continue;
                    }

                    // ✅ CASE 3: Correct prefab
                    if (prefabSource == canvasPrefab)
                    {
                        hasCorrectCanvas = true;
                        Debug.Log("Correct Canvas prefab already present, keeping it.");
                    }
                    // ❌ CASE 4: Wrong prefab or hand-made Canvas
                    else
                    {
                        Debug.Log($"Deleting old/mismatched Canvas: {go.name}");
                        Object.DestroyImmediate(go);
                    }
                }

                // If no correct Canvas remains, add one
                if (!hasCorrectCanvas)
                {
                    GameObject newCanvasInstance = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);
                    newCanvasInstance.name = "Canvas";
                    Debug.Log("New Canvas instantiated from prefab.");

                    // Try to reconnect PlayerSpawner -> GameplayUIController
                    PlayerSpawner spawner = Object.FindFirstObjectByType<PlayerSpawner>();
                    GameplayUIController uiController = newCanvasInstance.GetComponent<GameplayUIController>();

                    if (spawner != null && uiController != null)
                    {
                        SerializedObject spawnerSO = new SerializedObject(spawner);
                        SerializedProperty prop = spawnerSO.FindProperty("gameplayUIController");
                        if (prop != null)
                        {
                            prop.objectReferenceValue = uiController;
                            spawnerSO.ApplyModifiedProperties();
                            Debug.Log("Successfully reconnected PlayerSpawner -> GameplayUIController reference.");
                        }
                        else
                        {
                            Debug.LogWarning("Field 'gameplayUIController' not found in PlayerSpawner.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Could not find PlayerSpawner or GameplayUIController to reconnect references.");
                    }
                }

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
                fixedScenes++;
            }

            EditorUtility.DisplayDialog("Success!",
                $"Canvas replacement completed.\n\n✅ Fixed Scenes: {fixedScenes}\n⚠️ Skipped Scenes: {skippedScenes}",
                "OK");
        }
        finally
        {
            if (!string.IsNullOrEmpty(originalScenePath))
                EditorSceneManager.OpenScene(originalScenePath);

            Debug.Log("Cleanup complete.");
        }
    }
}
