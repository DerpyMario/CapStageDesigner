using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class StageValidator : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ValidationResult> validationResults = new List<ValidationResult>();
    
    [MenuItem("Tools/Stage Designer/Stage Validator")]
    public static void ShowWindow()
    {
        GetWindow<StageValidator>("Stage Validator");
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Stage Validator", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Run Validation"))
        {
            RunValidation();
        }
        
        EditorGUILayout.Space();
        
        if (validationResults.Count > 0)
        {
            EditorGUILayout.LabelField($"Validation Results ({validationResults.Count})", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var result in validationResults)
            {
                DrawValidationResult(result);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Fix All Auto-Fixable Issues"))
            {
                FixAllAutoFixableIssues();
            }
        }
    }
    
    void DrawValidationResult(ValidationResult result)
    {
        Color originalColor = GUI.color;
        
        switch (result.severity)
        {
            case ValidationSeverity.Error:
                GUI.color = Color.red;
                break;
            case ValidationSeverity.Warning:
                GUI.color = Color.yellow;
                break;
            case ValidationSeverity.Info:
                GUI.color = Color.cyan;
                break;
        }
        
        EditorGUILayout.BeginVertical("box");
        GUI.color = originalColor;
        
        EditorGUILayout.LabelField($"[{result.severity}] {result.title}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(result.description, EditorStyles.wordWrappedLabel);
        
        if (result.targetObject != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("Object", result.targetObject, typeof(GameObject), true);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = result.targetObject;
                EditorGUIUtility.PingObject(result.targetObject);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if (result.canAutoFix && GUILayout.Button("Auto Fix"))
        {
            result.autoFixAction?.Invoke();
            RunValidation(); // Re-run validation after fix
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void RunValidation()
    {
        validationResults.Clear();
        
        ValidateStageStructure();
        ValidateStageObjects();
        ValidateClipBounds();
        ValidatePrefabReferences();
        ValidatePerformance();
        
        Repaint();
    }
    
    void ValidateStageStructure()
    {
        StageManager[] stageManagers = FindObjectsOfType<StageManager>();
        
        if (stageManagers.Length == 0)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Warning,
                title = "No Stage Manager Found",
                description = "Scene doesn't contain a StageManager component. This may cause issues with stage functionality.",
                canAutoFix = true,
                autoFixAction = () => {
                    GameObject stageRoot = new GameObject("StageRoot");
                    stageRoot.AddComponent<StageManager>();
                }
            });
        }
        else if (stageManagers.Length > 1)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Error,
                title = "Multiple Stage Managers Found",
                description = "Scene contains multiple StageManager components. Only one should exist per scene.",
                targetObject = stageManagers[1].gameObject
            });
        }
        
        // Check for proper hierarchy
        foreach (var stageManager in stageManagers)
        {
            ClipBounds[] clips = stageManager.GetComponentsInChildren<ClipBounds>();
            if (clips.Length == 0)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    title = "No Clips Found",
                    description = "StageManager has no clip bounds. Stage may not function properly.",
                    targetObject = stageManager.gameObject
                });
            }
        }
    }
    
    void ValidateStageObjects()
    {
        StageObject[] stageObjects = FindObjectsOfType<StageObject>();
        
        foreach (var stageObj in stageObjects)
        {
            // Check for missing required data
            if (string.IsNullOrEmpty(stageObj.sGroupID))
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    title = "Missing Group ID",
                    description = "StageObject has no Group ID assigned.",
                    targetObject = stageObj.gameObject,
                    canAutoFix = true,
                    autoFixAction = () => {
                        stageObj.sGroupID = "DEFAULT";
                        EditorUtility.SetDirty(stageObj);
                    }
                });
            }
            
            // Check for invalid clip index
            StageManager stageManager = FindObjectOfType<StageManager>();
            if (stageManager != null)
            {
                ClipBounds[] clips = stageManager.GetComponentsInChildren<ClipBounds>();
                if (stageObj.clipIndex >= clips.Length || stageObj.clipIndex < 0)
                {
                    validationResults.Add(new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        title = "Invalid Clip Index",
                        description = $"StageObject has invalid clip index: {stageObj.clipIndex}",
                        targetObject = stageObj.gameObject,
                        canAutoFix = true,
                        autoFixAction = () => {
                            stageObj.clipIndex = 0;
                            EditorUtility.SetDirty(stageObj);
                        }
                    });
                }
            }
            
            // Check if object is in correct clip bounds
            if (stageManager != null)
            {
                ClipBounds[] clips = stageManager.GetComponentsInChildren<ClipBounds>();
                if (stageObj.clipIndex < clips.Length)
                {
                    ClipBounds targetClip = clips[stageObj.clipIndex];
                    if (!targetClip.ContainsObject(stageObj.gameObject))
                    {
                        validationResults.Add(new ValidationResult
                        {
                            severity = ValidationSeverity.Warning,
                            title = "Object Outside Clip Bounds",
                            description = "StageObject is positioned outside its assigned clip bounds.",
                            targetObject = stageObj.gameObject,
                            canAutoFix = true,
                            autoFixAction = () => {
                                stageObj.transform.SetParent(targetClip.transform);
                            }
                        });
                    }
                }
            }
            
            // Validate MapObjEvent properties
            if (stageObj.name.Contains("MapObjEvent"))
            {
                MapEventData mapEvent = StageJsonUtility.ParseMapEventProperty(stageObj.property);
                if (mapEvent == null)
                {
                    validationResults.Add(new ValidationResult
                    {
                        severity = ValidationSeverity.Error,
                        title = "Invalid MapObjEvent Data",
                        description = "MapObjEvent has invalid or corrupted property data.",
                        targetObject = stageObj.gameObject
                    });
                }
            }
        }
    }
    
    void ValidateClipBounds()
    {
        ClipBounds[] clips = FindObjectsOfType<ClipBounds>();
        
        for (int i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            
            // Check for overlapping clips
            for (int j = i + 1; j < clips.Length; j++)
            {
                var otherClip = clips[j];
                if (ClipsOverlap(clip, otherClip))
                {
                    validationResults.Add(new ValidationResult
                    {
                        severity = ValidationSeverity.Warning,
                        title = "Overlapping Clip Bounds",
                        description = $"Clip bounds overlap between {clip.name} and {otherClip.name}",
                        targetObject = clip.gameObject
                    });
                }
            }
            
            // Check for empty clips
            StageObject[] objectsInClip = clip.GetComponentsInChildren<StageObject>();
            if (objectsInClip.Length == 0)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Info,
                    title = "Empty Clip",
                    description = "Clip contains no stage objects.",
                    targetObject = clip.gameObject
                });
            }
            
            // Check clip size
            float clipWidth = clip.maxX - clip.minX;
            if (clipWidth <= 0)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Error,
                    title = "Invalid Clip Size",
                    description = "Clip has invalid width (maxX <= minX).",
                    targetObject = clip.gameObject,
                    canAutoFix = true,
                    autoFixAction = () => {
                        clip.maxX = clip.minX + 10f;
                        EditorUtility.SetDirty(clip);
                    }
                });
            }
        }
    }
    
    void ValidatePrefabReferences()
    {
        StageObject[] stageObjects = FindObjectsOfType<StageObject>();
        
        foreach (var stageObj in stageObjects)
        {
            if (!string.IsNullOrEmpty(stageObj.bundlepath))
            {
                // Check if prefab exists
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(stageObj.bundlepath);
                if (prefab == null)
                {
                    // Try loading from Resources
                    prefab = Resources.Load<GameObject>(stageObj.bundlepath);
                    if (prefab == null)
                    {
                        validationResults.Add(new ValidationResult
                        {
                            severity = ValidationSeverity.Error,
                            title = "Missing Prefab Reference",
                            description = $"Cannot find prefab at path: {stageObj.bundlepath}",
                            targetObject = stageObj.gameObject
                        });
                    }
                }
            }
        }
    }
    
    void ValidatePerformance()
    {
        StageObject[] stageObjects = FindObjectsOfType<StageObject>();
        
        // Check object count per clip
        Dictionary<int, int> objectsPerClip = new Dictionary<int, int>();
        foreach (var obj in stageObjects)
        {
            if (!objectsPerClip.ContainsKey(obj.clipIndex))
                objectsPerClip[obj.clipIndex] = 0;
            objectsPerClip[obj.clipIndex]++;
        }
        
        foreach (var kvp in objectsPerClip)
        {
            if (kvp.Value > 100) // Arbitrary threshold
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    title = "High Object Count",
                    description = $"Clip {kvp.Key} contains {kvp.Value} objects. Consider optimization.",
                });
            }
        }
        
        // Check for objects with high polygon count
        foreach (var stageObj in stageObjects)
        {
            MeshFilter meshFilter = stageObj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                int triangleCount = meshFilter.sharedMesh.triangles.Length / 3;
                if (triangleCount > 1000) // Arbitrary threshold
                {
                    validationResults.Add(new ValidationResult
                    {
                        severity = ValidationSeverity.Info,
                        title = "High Polygon Count",
                        description = $"Object has {triangleCount} triangles. Consider using LOD.",
                        targetObject = stageObj.gameObject
                    });
                }
            }
        }
    }
    
    bool ClipsOverlap(ClipBounds clip1, ClipBounds clip2)
    {
        return !(clip1.maxX <= clip2.minX || clip2.maxX <= clip1.minX);
    }
    
    void FixAllAutoFixableIssues()
    {
        foreach (var result in validationResults)
        {
            if (result.canAutoFix && result.autoFixAction != null)
            {
                result.autoFixAction.Invoke();
            }
        }
        
        RunValidation(); // Re-run validation after fixes
    }
}

[System.Serializable]
public class ValidationResult
{
    public ValidationSeverity severity;
    public string title;
    public string description;
    public GameObject targetObject;
    public bool canAutoFix;
    public System.Action autoFixAction;
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}
