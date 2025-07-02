using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class StageBatchOperations : EditorWindow
{
    private Vector2 scrollPosition;
    private string groupFilter = "";
    private int selectedClipFilter = -1;
    private List<StageObject> filteredObjects = new List<StageObject>();
    
    [MenuItem("Tools/Stage Designer/Batch Operations")]
    public static void ShowWindow()
    {
        GetWindow<StageBatchOperations>("Batch Operations");
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Stage Batch Operations", EditorStyles.boldLabel);
        
        DrawFilters();
        DrawObjectList();
        DrawBatchOperations();
    }
    
    void DrawFilters()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        
        groupFilter = EditorGUILayout.TextField("Group ID Filter", groupFilter);
        selectedClipFilter = EditorGUILayout.IntField("Clip Filter (-1 = All)", selectedClipFilter);
        
        if (GUILayout.Button("Apply Filters"))
        {
            ApplyFilters();
        }
        EditorGUILayout.EndVertical();
    }
    
    void DrawObjectList()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Filtered Objects ({filteredObjects.Count})", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        
        for (int i = 0; i < filteredObjects.Count; i++)
        {
            var obj = filteredObjects[i];
            if (obj == null) continue;
            
            EditorGUILayout.BeginHorizontal();
            
            bool isSelected = Selection.gameObjects.Contains(obj.gameObject);
            bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
            
            if (newSelected != isSelected)
            {
                if (newSelected)
                {
                    var selectedList = Selection.gameObjects.ToList();
                    selectedList.Add(obj.gameObject);
                    Selection.objects = selectedList.ToArray();
                }
                else
                {
                    var selectedList = Selection.gameObjects.ToList();
                    selectedList.Remove(obj.gameObject);
                    Selection.objects = selectedList.ToArray();
                }
            }
            
            EditorGUILayout.LabelField(obj.name, GUILayout.Width(150));
            EditorGUILayout.LabelField($"Group: {obj.sGroupID}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"Clip: {obj.clipIndex}", GUILayout.Width(60));
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = obj.gameObject;
                EditorGUIUtility.PingObject(obj.gameObject);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            Selection.objects = filteredObjects.Select(o => o.gameObject).ToArray();
        }
        if (GUILayout.Button("Deselect All"))
        {
            Selection.objects = new Object[0];
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawBatchOperations()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Transform Operations", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Align X"))
        {
            AlignSelectedObjects(0);
        }
        if (GUILayout.Button("Align Y"))
        {
            AlignSelectedObjects(1);
        }
        if (GUILayout.Button("Align Z"))
        {
            AlignSelectedObjects(2);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Distribute X"))
        {
            DistributeSelectedObjects(0);
        }
        if (GUILayout.Button("Distribute Y"))
        {
            DistributeSelectedObjects(1);
        }
        if (GUILayout.Button("Distribute Z"))
        {
            DistributeSelectedObjects(2);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Property Operations", EditorStyles.boldLabel);
        
        string newGroupID = EditorGUILayout.TextField("New Group ID", "");
        if (GUILayout.Button("Set Group ID") && !string.IsNullOrEmpty(newGroupID))
        {
            SetGroupIDForSelected(newGroupID);
        }
        
        int newClipIndex = EditorGUILayout.IntField("New Clip Index", 0);
        if (GUILayout.Button("Move to Clip"))
        {
            MoveSelectedToClip(newClipIndex);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Snap Operations", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Snap to Grid"))
        {
            SnapSelectedToGrid();
        }
        if (GUILayout.Button("Snap to Ground"))
        {
            SnapSelectedToGround();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Utility Operations", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Group"))
        {
            CreateGroupFromSelected();
        }
        
        if (GUILayout.Button("Delete Selected"))
        {
            DeleteSelected();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void ApplyFilters()
    {
        filteredObjects.Clear();
        StageObject[] allObjects = FindObjectsOfType<StageObject>();
        
        foreach (var obj in allObjects)
        {
            bool passesFilter = true;
            
            if (!string.IsNullOrEmpty(groupFilter) && obj.sGroupID != groupFilter)
            {
                passesFilter = false;
            }
            
            if (selectedClipFilter >= 0 && obj.clipIndex != selectedClipFilter)
            {
                passesFilter = false;
            }
            
            if (passesFilter)
            {
                filteredObjects.Add(obj);
            }
        }
        
        Repaint();
    }
    
    void AlignSelectedObjects(int axis)
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length < 2) return;
        
        Vector3 referencePos = selected[0].transform.position;
        
        Undo.RecordObjects(selected.Select(o => o.transform).ToArray(), "Align Objects");
        
        foreach (var obj in selected)
        {
            Vector3 pos = obj.transform.position;
            pos[axis] = referencePos[axis];
            obj.transform.position = pos;
        }
    }
    
    void DistributeSelectedObjects(int axis)
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length < 3) return;
        
        // Sort by position on the specified axis
        var sortedObjects = selected.OrderBy(o => o.transform.position[axis]).ToArray();
        
        float startPos = sortedObjects[0].transform.position[axis];
        float endPos = sortedObjects[sortedObjects.Length - 1].transform.position[axis];
        float step = (endPos - startPos) / (sortedObjects.Length - 1);
        
        Undo.RecordObjects(sortedObjects.Select(o => o.transform).ToArray(), "Distribute Objects");
        
        for (int i = 1; i < sortedObjects.Length - 1; i++)
        {
            Vector3 pos = sortedObjects[i].transform.position;
            pos[axis] = startPos + step * i;
            sortedObjects[i].transform.position = pos;
        }
    }
    
    void SetGroupIDForSelected(string groupID)
    {
        StageObject[] selectedStageObjects = Selection.gameObjects
            .Select(o => o.GetComponent<StageObject>())
            .Where(so => so != null)
            .ToArray();
        
        if (selectedStageObjects.Length == 0) return;
        
        Undo.RecordObjects(selectedStageObjects, "Set Group ID");
        
        foreach (var obj in selectedStageObjects)
        {
            obj.sGroupID = groupID;
            EditorUtility.SetDirty(obj);
        }
    }
    
    void MoveSelectedToClip(int clipIndex)
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null) return;
        
        ClipBounds[] clips = stageManager.GetComponentsInChildren<ClipBounds>();
        if (clipIndex < 0 || clipIndex >= clips.Length) return;
        
        GameObject[] selected = Selection.gameObjects;
        Undo.RecordObjects(selected.Select(o => o.transform).ToArray(), "Move to Clip");
        
        foreach (var obj in selected)
        {
            StageObject stageObj = obj.GetComponent<StageObject>();
            if (stageObj != null)
            {
                obj.transform.SetParent(clips[clipIndex].transform);
                stageObj.clipIndex = clipIndex;
                EditorUtility.SetDirty(stageObj);
            }
        }
    }
    
    void SnapSelectedToGrid()
    {
        StageDesignerSettings settings = StageDesignerWindow.GetSettings();
        if (settings == null) return;
        
        GameObject[] selected = Selection.gameObjects;
        Undo.RecordObjects(selected.Select(o => o.transform).ToArray(), "Snap to Grid");
        
        foreach (var obj in selected)
        {
            Vector3 pos = SnapSystem.SnapToGrid(obj.transform.position, settings.gridSize);
            obj.transform.position = pos;
        }
    }
    
    void SnapSelectedToGround()
    {
        GameObject[] selected = Selection.gameObjects;
        Undo.RecordObjects(selected.Select(o => o.transform).ToArray(), "Snap to Ground");
        
        foreach (var obj in selected)
        {
            RaycastHit hit;
            Vector3 rayStart = obj.transform.position + Vector3.up * 100f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
            {
                Vector3 pos = obj.transform.position;
                pos.y = hit.point.y;
                obj.transform.position = pos;
            }
        }
    }
    
    void CreateGroupFromSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0) return;
        
        GameObject groupParent = new GameObject("Group_" + System.DateTime.Now.Ticks);
        
        // Calculate center position
        Vector3 center = Vector3.zero;
        foreach (var obj in selected)
        {
            center += obj.transform.position;
        }
        center /= selected.Length;
        groupParent.transform.position = center;
        
        Undo.RegisterCreatedObjectUndo(groupParent, "Create Group");
        Undo.RecordObjects(selected.Select(o => o.transform).ToArray(), "Group Objects");
        
        foreach (var obj in selected)
        {
            obj.transform.SetParent(groupParent.transform);
        }
        
        Selection.activeGameObject = groupParent;
    }
    
    void DeleteSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0) return;
        
        if (EditorUtility.DisplayDialog("Delete Objects", 
            $"Are you sure you want to delete {selected.Length} objects?", 
            "Delete", "Cancel"))
        {
            foreach (var obj in selected)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }
    }
    
    void OnFocus()
    {
        ApplyFilters();
    }
}
