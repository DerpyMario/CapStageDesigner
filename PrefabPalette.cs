using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabPalette : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();
    private Vector2 scrollPosition;
    private GameObject selectedPrefab;
    private string searchFilter = "";
    private bool isPlacementMode = false;
    
    [MenuItem("Tools/Stage Designer/Prefab Palette")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPalette>("Prefab Palette");
    }
    
    void OnEnable()
    {
        LoadPrefabs();
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    void LoadPrefabs()
    {
        prefabs.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Prefabs"))
        {
            LoadPrefabs();
        }
        
        isPlacementMode = GUILayout.Toggle(isPlacementMode, "Placement Mode", EditorStyles.miniButton);
        EditorGUILayout.EndHorizontal();
        
        searchFilter = EditorGUILayout.TextField("Search", searchFilter);
        
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        int columns = Mathf.Max(1, (int)(position.width / 80));
        int currentColumn = 0;
        
        EditorGUILayout.BeginHorizontal();
        
        foreach (var prefab in prefabs)
        {
            if (!string.IsNullOrEmpty(searchFilter) && 
                !prefab.name.ToLower().Contains(searchFilter.ToLower()))
                continue;
            
            if (currentColumn >= columns)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                currentColumn = 0;
            }
            
            EditorGUILayout.BeginVertical(GUILayout.Width(75));
            
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview != null)
            {
                if (GUILayout.Button(preview, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    selectedPrefab = prefab;
                    if (isPlacementMode)
                    {
                        Selection.activeObject = prefab;
                    }
                }
            }
            else
            {
                if (GUILayout.Button(prefab.name, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    selectedPrefab = prefab;
                    if (isPlacementMode)
                    {
                        Selection.activeObject = prefab;
                    }
                }
            }
            
            EditorGUILayout.LabelField(prefab.name, EditorStyles.miniLabel, GUILayout.Width(75));
            EditorGUILayout.EndVertical();
            
            currentColumn++;
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        
        if (selectedPrefab != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selected: " + selectedPrefab.name, EditorStyles.boldLabel);
            
            if (GUILayout.Button("Place in Scene"))
            {
                PlacePrefabInScene();
            }
        }
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacementMode || selectedPrefab == null) return;
        
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            
            Vector3 placePosition;
            if (Physics.Raycast(ray, out hit))
            {
                placePosition = hit.point;
            }
            else
            {
                placePosition = ray.origin + ray.direction * 10f;
            }
            
            PlacePrefabAtPosition(placePosition);
            e.Use();
        }
        
        // Draw preview at mouse position
        if (e.type == EventType.Repaint)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            
            Vector3 previewPosition;
            if (Physics.Raycast(ray, out hit))
            {
                previewPosition = hit.point;
            }
            else
            {
                previewPosition = ray.origin + ray.direction * 10f;
            }
            
            // Snap to grid if enabled
            StageDesignerSettings settings = StageDesignerWindow.GetSettings();
            if (settings != null && settings.snapToGrid)
            {
                previewPosition = SnapSystem.SnapToGrid(previewPosition, settings.gridSize);
            }
            
            Handles.color = Color.green;
            Handles.DrawWireCube(previewPosition, Vector3.one * 0.5f);
            Handles.Label(previewPosition + Vector3.up, selectedPrefab.name);
        }
        
        sceneView.Repaint();
    }
    
    void PlacePrefabInScene()
    {
        Vector3 position = Vector3.zero;
        if (SceneView.lastActiveSceneView != null)
        {
            position = SceneView.lastActiveSceneView.camera.transform.position + 
                      SceneView.lastActiveSceneView.camera.transform.forward * 5f;
        }
        
        PlacePrefabAtPosition(position);
    }
    
    void PlacePrefabAtPosition(Vector3 position)
    {
        if (selectedPrefab == null) return;
        
        GameObject instance = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
        instance.transform.position = position;
        
        // Add StageObject component if it doesn't exist
        StageObject stageObj = instance.GetComponent<StageObject>();
        if (stageObj == null)
        {
            stageObj = instance.AddComponent<StageObject>();
        }
        
        // Set default values
        StageDesignerSettings settings = StageDesignerWindow.GetSettings();
        if (settings != null)
        {
            stageObj.sGroupID = settings.defaultGroupID;
        }
        
        // Try to find the prefab path
        string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
        stageObj.GetComponent<StageObject>().bundlepath = prefabPath;
        
        // Find appropriate clip to add to
        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager != null)
        {
            ClipBounds[] clips = stageManager.GetComponentsInChildren<ClipBounds>();
            foreach (var clip in clips)
            {
                if (clip.ContainsPoint(position))
                {
                    instance.transform.SetParent(clip.transform);
                    stageObj.clipIndex = System.Array.IndexOf(clips, clip);
                    break;
                }
            }
        }
        
        Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");
        Selection.activeGameObject = instance;
    }
}
