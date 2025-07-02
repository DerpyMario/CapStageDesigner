using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class StageDesignerWindow : EditorWindow
{
    private static StageDesignerSettings settings;
    private StageData currentStage;
    private Vector2 scrollPosition;
    private int selectedClipIndex = -1;
    private int selectedObjectIndex = -1;
    private bool showClipSettings = true;
    private bool showObjectSettings = true;
    private bool showGridSettings = true;
    
    private GameObject stageRoot;
    private List<GameObject> clipObjects = new List<GameObject>();
    
    [MenuItem("Tools/Stage Designer")]
    public static void ShowWindow()
    {
        GetWindow<StageDesignerWindow>("Stage Designer");
    }
    
    void OnEnable()
    {
        LoadSettings();
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    public static StageDesignerSettings GetSettings()
    {
        if (settings == null)
        {
            LoadSettings();
        }
        return settings;
    }
    
    static void LoadSettings()
    {
        settings = Resources.Load<StageDesignerSettings>("StageDesignerSettings");
        if (settings == null)
        {
            settings = CreateInstance<StageDesignerSettings>();
            if (!Directory.Exists("Assets/Resources"))
                Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.CreateAsset(settings, "Assets/Resources/StageDesignerSettings.asset");
            AssetDatabase.SaveAssets();
        }
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawToolbar();
        DrawStageSettings();
        DrawClipSettings();
        DrawObjectSettings();
        DrawGridSettings();
        
        EditorGUILayout.EndScrollView();
    }
    
    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("New Stage", EditorStyles.toolbarButton))
        {
            CreateNewStage();
        }
        
        if (GUILayout.Button("Load JSON", EditorStyles.toolbarButton))
        {
            LoadStageFromJSON();
        }
        
        if (GUILayout.Button("Save JSON", EditorStyles.toolbarButton))
        {
            SaveStageToJSON();
        }
        
        if (GUILayout.Button("Build Scene", EditorStyles.toolbarButton))
        {
            BuildSceneFromData();
        }
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
    }
    
    void DrawStageSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stage Settings", EditorStyles.boldLabel);
        
        if (currentStage != null)
        {
            currentStage.nVer = EditorGUILayout.IntField("Version", currentStage.nVer);
            currentStage.fStageClipWidth = EditorGUILayout.FloatField("Stage Clip Width", currentStage.fStageClipWidth);
        }
    }
    
    void DrawClipSettings()
    {
        EditorGUILayout.Space();
        showClipSettings = EditorGUILayout.Foldout(showClipSettings, "Clip Settings");
        
        if (showClipSettings && currentStage != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Clip"))
            {
                AddNewClip();
            }
            if (GUILayout.Button("Remove Selected Clip") && selectedClipIndex >= 0)
            {
                RemoveClip(selectedClipIndex);
            }
            EditorGUILayout.EndHorizontal();
            
            for (int i = 0; i < currentStage.Datas.Count; i++)
            {
                DrawClipData(i, currentStage.Datas[i]);
            }
        }
    }
    
    void DrawClipData(int index, ClipData clip)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        bool isSelected = selectedClipIndex == index;
        if (GUILayout.Toggle(isSelected, $"Clip {index}", EditorStyles.foldout) != isSelected)
        {
            selectedClipIndex = isSelected ? -1 : index;
        }
        EditorGUILayout.EndHorizontal();
        
        if (selectedClipIndex == index)
        {
            clip.fClipMinx = EditorGUILayout.TextField("Clip Min X", clip.fClipMinx);
            clip.fClipMaxx = EditorGUILayout.TextField("Clip Max X", clip.fClipMaxx);
            
            EditorGUILayout.LabelField($"Objects: {clip.Datas.Count}");
            
            if (GUILayout.Button("Add Object to Clip"))
            {
                AddObjectToClip(index);
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawObjectSettings()
    {
        EditorGUILayout.Space();
        showObjectSettings = EditorGUILayout.Foldout(showObjectSettings, "Object Settings");
        
        if (showObjectSettings && currentStage != null && selectedClipIndex >= 0)
        {
            var clip = currentStage.Datas[selectedClipIndex];
            
            for (int i = 0; i < clip.Datas.Count; i++)
            {
                DrawObjectData(i, clip.Datas[i]);
            }
        }
    }
    
    void DrawObjectData(int index, GameObjectData objData)
    {
        EditorGUILayout.BeginVertical("box");
        
        bool isSelected = selectedObjectIndex == index;
        if (GUILayout.Toggle(isSelected, objData.name, EditorStyles.foldout) != isSelected)
        {
            selectedObjectIndex = isSelected ? -1 : index;
        }
        
        if (selectedObjectIndex == index)
        {
            objData.name = EditorGUILayout.TextField("Name", objData.name);
            objData.sGroupID = EditorGUILayout.TextField("Group ID", objData.sGroupID);
            objData.path = EditorGUILayout.TextField("Path", objData.path);
            objData.bundlepath = EditorGUILayout.TextField("Bundle Path", objData.bundlepath);
            objData.property = EditorGUILayout.TextField("Property", objData.property);
            
            // Position
            EditorGUILayout.LabelField("Position");
            objData.position.x = EditorGUILayout.FloatField("X", objData.position.x);
            objData.position.y = EditorGUILayout.FloatField("Y", objData.position.y);
            objData.position.z = EditorGUILayout.FloatField("Z", objData.position.z);
            
            // Scale
            EditorGUILayout.LabelField("Scale");
            objData.scale.x = EditorGUILayout.FloatField("X", objData.scale.x);
            objData.scale.y = EditorGUILayout.FloatField("Y", objData.scale.y);
            objData.scale.z = EditorGUILayout.FloatField("Z", objData.scale.z);
            
            // Rotation
            EditorGUILayout.LabelField("Rotation");
            objData.rotate.x = EditorGUILayout.FloatField("X", objData.rotate.x);
            objData.rotate.y = EditorGUILayout.FloatField("Y", objData.rotate.y);
            objData.rotate.z = EditorGUILayout.FloatField("Z", objData.rotate.z);
            objData.rotate.w = EditorGUILayout.FloatField("W", objData.rotate.w);
            
            if (GUILayout.Button("Remove Object"))
            {
                RemoveObjectFromClip(selectedClipIndex, index);
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawGridSettings()
    {
        EditorGUILayout.Space();
        showGridSettings = EditorGUILayout.Foldout(showGridSettings, "Grid & Snap Settings");
        
        if (showGridSettings && settings != null)
        {
            settings.gridSize = EditorGUILayout.FloatField("Grid Size", settings.gridSize);
            settings.snapToGrid = EditorGUILayout.Toggle("Snap to Grid", settings.snapToGrid);
            settings.showGrid = EditorGUILayout.Toggle("Show Grid", settings.showGrid);
            settings.gridColor = EditorGUILayout.ColorField("Grid Color", settings.gridColor);
            settings.snapDistance = EditorGUILayout.FloatField("Snap Distance", settings.snapDistance);
            settings.snapToVertices = EditorGUILayout.Toggle("Snap to Vertices", settings.snapToVertices);
            settings.snapToEdges = EditorGUILayout.Toggle("Snap to Edges", settings.snapToEdges);
            settings.snapToFaces = EditorGUILayout.Toggle("Snap to Faces", settings.snapToFaces);
            
            EditorUtility.SetDirty(settings);
        }
    }
    
    void CreateNewStage()
    {
        currentStage = new StageData
        {
            nVer = 0,
            fStageClipWidth = settings.defaultClipWidth,
            Datas = new List<ClipData>()
        };
        
        selectedClipIndex = -1;
        selectedObjectIndex = -1;
        
        // Create stage root object
        if (stageRoot != null)
            DestroyImmediate(stageRoot);
            
        stageRoot = new GameObject("StageRoot");
        stageRoot.AddComponent<StageManager>();
    }
    
    void LoadStageFromJSON()
    {
        string path = EditorUtility.OpenFilePanel("Load Stage JSON", Application.dataPath, "json");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string jsonContent = File.ReadAllText(path);
                currentStage = JsonConvert.DeserializeObject<StageData>(jsonContent);
                Debug.Log("Stage loaded successfully!");
                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load JSON: {e.Message}", "OK");
            }
        }
    }
    
    void SaveStageToJSON()
    {
        if (currentStage == null)
        {
            EditorUtility.DisplayDialog("Error", "No stage data to save!", "OK");
            return;
        }
        
        // Update stage data from scene objects
        UpdateStageDataFromScene();
        
        string path = EditorUtility.SaveFilePanel("Save Stage JSON", Application.dataPath, "NewStage", "json");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(currentStage, Formatting.Indented);
                File.WriteAllText(path, jsonContent);
                Debug.Log("Stage saved successfully!");
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save JSON: {e.Message}", "OK");
            }
        }
    }
    
    void BuildSceneFromData()
    {
        if (currentStage == null)
        {
            EditorUtility.DisplayDialog("Error", "No stage data to build!", "OK");
            return;
        }
        
        // Clear existing objects
        ClearScene();
        
        // Create stage root
        stageRoot = new GameObject("StageRoot");
        var stageManager = stageRoot.AddComponent<StageManager>();
        stageManager.stageData = currentStage;
        
        clipObjects.Clear();
        
        for (int clipIndex = 0; clipIndex < currentStage.Datas.Count; clipIndex++)
        {
            var clipData = currentStage.Datas[clipIndex];
            GameObject clipRoot = new GameObject($"Clip_{clipIndex}");
            clipRoot.transform.SetParent(stageRoot.transform);
            clipObjects.Add(clipRoot);
            
            // Add clip bounds component
            var clipBounds = clipRoot.AddComponent<ClipBounds>();
            clipBounds.minX = float.Parse(clipData.fClipMinx);
            clipBounds.maxX = float.Parse(clipData.fClipMaxx);
            
            foreach (var objData in clipData.Datas)
            {
                CreateObjectFromData(objData, clipRoot.transform, clipIndex);
            }
        }
        
        Debug.Log("Scene built successfully!");
    }
    
    GameObject CreateObjectFromData(GameObjectData objData, Transform parent, int clipIndex)
    {
        GameObject obj = null;
        
        // Try to load prefab from path
        if (!string.IsNullOrEmpty(objData.path))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(objData.path);
            if (prefab != null)
            {
                obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
        }
        
        // If prefab loading failed, create empty object
        if (obj == null)
        {
            obj = new GameObject(objData.name);
        }
        
        // Set transform
        obj.transform.SetParent(parent);
        obj.transform.position = objData.position.ToVector3();
        obj.transform.rotation = objData.rotate.ToQuaternion();
        obj.transform.localScale = objData.scale.ToVector3();
        obj.name = objData.name;
        
        // Add stage object component
        var stageObj = obj.GetComponent<StageObject>();
        if (stageObj == null)
            stageObj = obj.AddComponent<StageObject>();
            
        stageObj.sGroupID = objData.sGroupID;
        stageObj.bundlepath = objData.bundlepath;
        stageObj.property = objData.property;
        stageObj.clipIndex = clipIndex;
        
        return obj;
    }
    
    void UpdateStageDataFromScene()
    {
        if (stageRoot == null) return;
        
        currentStage.Datas.Clear();
        
        for (int i = 0; i < stageRoot.transform.childCount; i++)
        {
            Transform clipTransform = stageRoot.transform.GetChild(i);
            ClipBounds clipBounds = clipTransform.GetComponent<ClipBounds>();
            
            if (clipBounds != null)
            {
                ClipData clipData = new ClipData
                {
                    fClipMinx = clipBounds.minX.ToString(),
                    fClipMaxx = clipBounds.maxX.ToString(),
                    Datas = new List<GameObjectData>()
                };
                
                // Get all stage objects in this clip
                StageObject[] stageObjects = clipTransform.GetComponentsInChildren<StageObject>();
                foreach (var stageObj in stageObjects)
                {
                    clipData.Datas.Add(stageObj.ToGameObjectData());
                }
                
                currentStage.Datas.Add(clipData);
            }
        }
    }
    
    void ClearScene()
    {
        if (stageRoot != null)
        {
            DestroyImmediate(stageRoot);
        }
        clipObjects.Clear();
    }
    
    void AddNewClip()
    {
        if (currentStage == null) return;
        
        float minX = currentStage.Datas.Count * currentStage.fStageClipWidth;
        float maxX = minX + currentStage.fStageClipWidth;
        
        ClipData newClip = new ClipData
        {
            fClipMinx = minX.ToString(),
            fClipMaxx = maxX.ToString(),
            Datas = new List<GameObjectData>()
        };
        
        currentStage.Datas.Add(newClip);
        selectedClipIndex = currentStage.Datas.Count - 1;
    }
    
    void RemoveClip(int index)
    {
        if (currentStage == null || index < 0 || index >= currentStage.Datas.Count) return;
        
        currentStage.Datas.RemoveAt(index);
        if (selectedClipIndex >= currentStage.Datas.Count)
            selectedClipIndex = currentStage.Datas.Count - 1;
    }
    
    void AddObjectToClip(int clipIndex)
    {
        if (currentStage == null || clipIndex < 0 || clipIndex >= currentStage.Datas.Count) return;
        
        GameObjectData newObj = new GameObjectData
        {
            name = "New Object",
            sGroupID = settings.defaultGroupID,
            position = new Vector3Data(),
            scale = new Vector3Data { x = 1, y = 1, z = 1 },
            rotate = new QuaternionData { x = 0, y = 0, z = 0, w = 1 }
        };
        
        currentStage.Datas[clipIndex].Datas.Add(newObj);
    }
    
    void RemoveObjectFromClip(int clipIndex, int objectIndex)
    {
        if (currentStage == null || clipIndex < 0 || clipIndex >= currentStage.Datas.Count) return;
        
        var clip = currentStage.Datas[clipIndex];
        if (objectIndex < 0 || objectIndex >= clip.Datas.Count) return;
        
        clip.Datas.RemoveAt(objectIndex);
        if (selectedObjectIndex >= clip.Datas.Count)
            selectedObjectIndex = clip.Datas.Count - 1;
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (settings == null || !settings.showGrid) return;
        
        DrawGrid();
        DrawClipBounds();
        HandleObjectSnapping();
    }
    
    void DrawGrid()
    {
        Handles.color = settings.gridColor;
        
        Vector3 pos = Vector3.zero;
        float gridSize = settings.gridSize;
        int gridCount = 100;
        
        // Draw horizontal lines
        for (int i = -gridCount; i <= gridCount; i++)
        {
            Vector3 start = new Vector3(-gridCount * gridSize, i * gridSize, 0);
            Vector3 end = new Vector3(gridCount * gridSize, i * gridSize, 0);
            Handles.DrawLine(start, end);
        }
        
        // Draw vertical lines
        for (int i = -gridCount; i <= gridCount; i++)
        {
            Vector3 start = new Vector3(i * gridSize, -gridCount * gridSize, 0);
            Vector3 end = new Vector3(i * gridSize, gridCount * gridSize, 0);
            Handles.DrawLine(start, end);
        }
    }
    
    void DrawClipBounds()
    {
        if (currentStage == null) return;
        
        Handles.color = settings.clipBoundsColor;
        
        foreach (var clipData in currentStage.Datas)
        {
            if (float.TryParse(clipData.fClipMinx, out float minX) && 
                float.TryParse(clipData.fClipMaxx, out float maxX))
            {
                Vector3[] points = new Vector3[]
                {
                    new Vector3(minX, -50, 0),
                    new Vector3(minX, 50, 0),
                    new Vector3(maxX, 50, 0),
                    new Vector3(maxX, -50, 0)
                };
                
                Handles.DrawSolidRectangleWithOutline(points, Color.clear, settings.clipBoundsColor);
            }
        }
    }
    
    void HandleObjectSnapping()
    {
        if (!settings.snapToGrid) return;
        
        Event e = Event.current;
        if (e.type == EventType.MouseDrag && Selection.activeGameObject != null)
        {
            StageObject stageObj = Selection.activeGameObject.GetComponent<StageObject>();
            if (stageObj != null && stageObj.snapToGrid)
            {
                Vector3 pos = stageObj.transform.position;
                pos.x = Mathf.Round(pos.x / settings.gridSize) * settings.gridSize;
                pos.y = Mathf.Round(pos.y / settings.gridSize) * settings.gridSize;
                pos.z = Mathf.Round(pos.z / settings.gridSize) * settings.gridSize;
                stageObj.transform.position = pos;
            }
        }
    }
}
