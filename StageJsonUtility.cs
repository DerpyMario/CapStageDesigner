using System;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public static class StageJsonUtility
{
    public static string SerializeStageData(StageData stageData)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include
        };
        
        return JsonConvert.SerializeObject(stageData, settings);
    }
    
    public static StageData DeserializeStageData(string json)
    {
        try
        {
            // Handle the complex property strings that contain encoded JSON
            json = PreprocessJsonString(json);
            
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            
            return JsonConvert.DeserializeObject<StageData>(json, settings);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to deserialize stage data: {e.Message}");
            return null;
        }
    }
    
    static string PreprocessJsonString(string json)
    {
        // Handle the special encoding used in the property field
        // Replace ;2 with \n and other special characters
        json = json.Replace(";2", "\\n");
        json = json.Replace("\"2", "\":");
        
        return json;
    }
    
    public static MapEventData ParseMapEventProperty(string property)
    {
        if (string.IsNullOrEmpty(property)) return null;
        
        try
        {
            // Extract the JSON part from the property string
            int jsonStart = property.IndexOf("{");
            if (jsonStart == -1) return null;
            
            string jsonPart = property.Substring(jsonStart);
            jsonPart = PreprocessJsonString(jsonPart);
            
            return JsonConvert.DeserializeObject<MapEventData>(jsonPart);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse map event property: {e.Message}");
            return null;
        }
    }
    
    public static string SerializeMapEventProperty(MapEventData mapEvent, string prefix = "")
    {
        if (mapEvent == null) return "";
        
        try
        {
            string json = JsonConvert.SerializeObject(mapEvent, Formatting.None);
            json = PostprocessJsonString(json);
            
            return prefix + json;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to serialize map event property: {e.Message}");
            return "";
        }
    }
    
    static string PostprocessJsonString(string json)
    {
        // Convert back to the special encoding
        json = json.Replace("\\n", ";2");
        json = json.Replace("\":", "\"2");
        
        return json;
    }
}

[Serializable]
public class MapEventData
{
    public int mapEvent;
    public Vector3Data MoveToPos;
    public float fDelayTime;
    public float fMoveTime;
    public bool bLoop;
    public int nType;
    public bool bCheckPlayer;
    public bool bCheckEnemy;
    public bool bRunAtInit;
    public string bmgs;
    public string bmge;
    public int nSetID;
    public AnimationCurveData mspd;
    public float B2DX;
    public float B2DY;
    public float B2DW;
    public float B2DH;
    public GameObjectData[] Datas;
}

[Serializable]
public class AnimationCurveData
{
    public string serializedVersion;
    public object[] m_Curve;
    public int m_PreInfinity;
    public int m_PostInfinity;
    public int m_RotationOrder;
}
