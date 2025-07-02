using UnityEngine;
using UnityEditor;

public static class StageDesignerMenu
{
    [MenuItem("Tools/Stage Designer/Stage Designer Window", priority = 1)]
    public static void OpenStageDesigner()
    {
        StageDesignerWindow.ShowWindow();
    }
    
    [MenuItem("Tools/Stage Designer/Prefab Palette", priority = 2)]
    public static void OpenPrefabPalette()
    {
        PrefabPalette.ShowWindow();
    }
    
    [MenuItem("Tools/Stage Designer/Batch Operations", priority = 3)]
    public static void OpenBatchOperations()
    {
        StageBatchOperations.ShowWindow();
    }
    
    [MenuItem("Tools/Stage Designer/Stage Validator", priority = 4)]
    public static void OpenStageValidator()
    {
        StageValidator.ShowWindow();
    }
    
    [MenuItem("Tools/Stage Designer/Settings", priority = 20)]
    public static void OpenSettings()
    {
        Selection.activeObject = StageDesignerWindow.GetSettings();
    }
    
    [MenuItem("GameObject/Stage Designer/Create Stage Root", priority = 10)]
    public static void CreateStageRoot()
    {
        GameObject stageRoot = new GameObject("StageRoot");
        stageRoot.AddComponent<StageManager>();
        
        Undo.RegisterCreatedObjectUndo(stageRoot, "Create Stage Root");
        Selection.activeGameObject = stageRoot;
    }
    
    [MenuItem("GameObject/Stage Designer/Create Clip", priority = 11)]
    public static void CreateClip()
    {
        GameObject clipObject = new GameObject("Clip");
        clipObject.AddComponent<ClipBounds>();
        
        // Try to parent to stage root if it exists
        StageManager stageManager = Object.FindObjectOfType<StageManager>();
        if (stageManager != null)
        {
            clipObject.transform.SetParent(stageManager.transform);
        }
        
        Undo.RegisterCreatedObjectUndo(clipObject, "Create Clip");
        Selection.activeGameObject = clipObject;
    }
    
    [MenuItem("GameObject/Stage Designer/Add Stage Object Component", priority = 12)]
    public static void AddStageObjectComponent()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<StageObject>() == null)
            {
                Undo.AddComponent<StageObject>(obj);
            }
        }
    }
    
    [MenuItem("GameObject/Stage Designer/Add Stage Object Component", validate = true)]
    public static bool ValidateAddStageObjectComponent()
    {
        return Selection.gameObjects.Length > 0;
    }
    
    [MenuItem("Assets/Stage Designer/Import Stage JSON", priority = 1000)]
    public static void ImportStageJSON()
    {
        string path = EditorUtility.OpenFilePanel("Import Stage JSON", Application.dataPath, "json");
        if (!string.IsNullOrEmpty(path))
        {
            StageDesignerWindow window = EditorWindow.GetWindow<StageDesignerWindow>();
            window.ImportStageFromPath(path);
        }
    }
    
    [MenuItem("Assets/Stage Designer/Export Stage JSON", priority = 1001)]
    public static void ExportStageJSON()
    {
        StageManager stageManager = Object.FindObjectOfType<StageManager>();
        if (stageManager == null)
        {
            EditorUtility.DisplayDialog("Error", "No StageManager found in scene!", "OK");
            return;
        }
        
        string path = EditorUtility.SaveFilePanel("Export Stage JSON", Application.dataPath, "Stage", "json");
        if (!string.IsNullOrEmpty(path))
        {
            stageManager.SaveCurrentStageToJson(path);
        }
    }
    
    [MenuItem("Assets/Stage Designer/Export Stage JSON", validate = true)]
    public static bool ValidateExportStageJSON()
    {
        return Object.FindObjectOfType<StageManager>() != null;
    }
}
