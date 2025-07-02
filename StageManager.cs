using UnityEngine;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    [Header("Stage Data")]
    public StageData stageData;
    
    [Header("Runtime Settings")]
    public bool autoLoadOnStart = false;
    public string stageJsonPath;
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    void Start()
    {
        if (autoLoadOnStart && !string.IsNullOrEmpty(stageJsonPath))
        {
            LoadStageFromPath(stageJsonPath);
        }
    }
    
    public void LoadStageFromPath(string path)
    {
        try
        {
            string jsonContent = System.IO.File.ReadAllText(path);
            stageData = Newtonsoft.Json.JsonConvert.DeserializeObject<StageData>(jsonContent);
            BuildStage();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load stage from {path}: {e.Message}");
        }
    }
    
    public void BuildStage()
    {
        ClearStage();
        
        if (stageData == null) return;
        
        for (int clipIndex = 0; clipIndex < stageData.Datas.Count; clipIndex++)
        {
            var clipData = stageData.Datas[clipIndex];
            GameObject clipRoot = new GameObject($"Clip_{clipIndex}");
            clipRoot.transform.SetParent(transform);
            
            var clipBounds = clipRoot.AddComponent<ClipBounds>();
            clipBounds.minX = float.Parse(clipData.fClipMinx);
            clipBounds.maxX = float.Parse(clipData.fClipMaxx);
            
            foreach (var objData in clipData.Datas)
            {
                GameObject obj = CreateObjectFromData(objData, clipRoot.transform, clipIndex);
                if (obj != null)
                    spawnedObjects.Add(obj);
            }
        }
    }
    
    GameObject CreateObjectFromData(GameObjectData objData, Transform parent, int clipIndex)
    {
        GameObject obj = null;
        
        // Try to load from Resources if bundle path is specified
        if (!string.IsNullOrEmpty(objData.bundlepath))
        {
            obj = Resources.Load<GameObject>(objData.bundlepath);
            if (obj != null)
                obj = Instantiate(obj);
        }
        
        // Fallback to creating empty object
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
    
    public void ClearStage()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        spawnedObjects.Clear();
        // Clear all child objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    
    public void SaveCurrentStageToJson(string path)
    {
        UpdateStageDataFromScene();
        
        try
        {
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(stageData, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(path, jsonContent);
            Debug.Log($"Stage saved to {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save stage to {path}: {e.Message}");
        }
    }
    
    void UpdateStageDataFromScene()
    {
        if (stageData == null)
            stageData = new StageData();
            
        stageData.Datas.Clear();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform clipTransform = transform.GetChild(i);
            ClipBounds clipBounds = clipTransform.GetComponent<ClipBounds>();
            
            if (clipBounds != null)
            {
                ClipData clipData = new ClipData
                {
                    fClipMinx = clipBounds.minX.ToString(),
                    fClipMaxx = clipBounds.maxX.ToString(),
                    Datas = new List<GameObjectData>()
                };
                
                StageObject[] stageObjects = clipTransform.GetComponentsInChildren<StageObject>();
                foreach (var stageObj in stageObjects)
                {
                    clipData.Datas.Add(stageObj.ToGameObjectData());
                }
                
                stageData.Datas.Add(clipData);
            }
        }
    }
    
    public List<StageObject> GetObjectsInGroup(string groupID)
    {
        List<StageObject> objects = new List<StageObject>();
        StageObject[] allObjects = GetComponentsInChildren<StageObject>();
        
        foreach (var obj in allObjects)
        {
            if (obj.sGroupID == groupID)
                objects.Add(obj);
        }
        
        return objects;
    }
    
    public List<StageObject> GetObjectsInClip(int clipIndex)
    {
        List<StageObject> objects = new List<StageObject>();
        StageObject[] allObjects = GetComponentsInChildren<StageObject>();
        
        foreach (var obj in allObjects)
        {
            if (obj.clipIndex == clipIndex)
                objects.Add(obj);
        }
        
        return objects;
    }
}
