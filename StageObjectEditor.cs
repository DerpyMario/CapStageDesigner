using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StageObject))]
public class StageObjectEditor : Editor
{
    private StageObject stageObject;
    private StageDesignerSettings settings;
    
    void OnEnable()
    {
        stageObject = (StageObject)target;
        settings = StageDesignerWindow.GetSettings();
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stage Designer Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Snap to Grid"))
        {
            SnapToGrid();
        }
        if (GUILayout.Button("Snap to Objects"))
        {
            SnapToNearestObjects();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Align to Ground"))
        {
            AlignToGround();
        }
        if (GUILayout.Button("Reset Transform"))
        {
            ResetTransform();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(stageObject);
        }
    }
    
    void SnapToGrid()
    {
        if (settings == null) return;
        
        Undo.RecordObject(stageObject.transform, "Snap to Grid");
        Vector3 pos = stageObject.transform.position;
        pos = SnapSystem.SnapToGrid(pos, settings.gridSize);
        stageObject.transform.position = pos;
    }
    
    void SnapToNearestObjects()
    {
        if (settings == null) return;
        
        StageObject[] allObjects = FindObjectsOfType<StageObject>();
        GameObject[] gameObjects = new GameObject[allObjects.Length - 1];
        int index = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj != stageObject)
            {
                gameObjects[index] = obj.gameObject;
                index++;
            }
        }
        
        Undo.RecordObject(stageObject.transform, "Snap to Objects");
        Vector3 pos = stageObject.transform.position;
        pos = SnapSystem.SnapToNearestObject(pos, gameObjects, settings.snapDistance);
        stageObject.transform.position = pos;
    }
    
    void AlignToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = stageObject.transform.position + Vector3.up * 100f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
        {
            Undo.RecordObject(stageObject.transform, "Align to Ground");
            Vector3 pos = stageObject.transform.position;
            pos.y = hit.point.y;
            stageObject.transform.position = pos;
        }
    }
    
    void ResetTransform()
    {
        Undo.RecordObject(stageObject.transform, "Reset Transform");
        stageObject.transform.localPosition = Vector3.zero;
        stageObject.transform.localRotation = Quaternion.identity;
        stageObject.transform.localScale = Vector3.one;
    }
    
    void OnSceneGUI()
    {
        if (settings == null || !settings.showGrid) return;
        
        // Draw snap indicators
        Handles.color = settings.selectedObjectColor;
        Handles.DrawWireCube(stageObject.transform.position, Vector3.one * 0.1f);
        
        // Show grid snap preview
        if (settings.snapToGrid)
        {
            Vector3 snapPos = SnapSystem.SnapToGrid(stageObject.transform.position, settings.gridSize);
            Handles.color = Color.green;
            Handles.DrawWireCube(snapPos, Vector3.one * 0.05f);
            Handles.DrawDottedLine(stageObject.transform.position, snapPos, 2f);
        }
    }
}
