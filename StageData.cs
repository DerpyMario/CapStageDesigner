using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData
{
    public int nVer = 0;
    public float fStageClipWidth = 25.0f;
    public List<ClipData> Datas = new List<ClipData>();
}

[Serializable]
public class ClipData
{
    public string fClipMinx;
    public string fClipMaxx;
    public List<GameObjectData> Datas = new List<GameObjectData>();
}

[Serializable]
public class GameObjectData
{
    public string sGroupID = "DefaultGroup";
    public string name;
    public Vector3Data position = new Vector3Data();
    public Vector3Data scale = new Vector3Data { x = 1, y = 1, z = 1 };
    public QuaternionData rotate = new QuaternionData { x = 0, y = 0, z = 0, w = 1 };
    public string path;
    public string bundlepath;
    public string property;
}

[Serializable]
public class Vector3Data
{
    public float x, y, z;
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
    
    public static Vector3Data FromVector3(Vector3 v)
    {
        return new Vector3Data { x = v.x, y = v.y, z = v.z };
    }
}

[Serializable]
public class QuaternionData
{
    public float x, y, z, w;
    
    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
    
    public static QuaternionData FromQuaternion(Quaternion q)
    {
        return new QuaternionData { x = q.x, y = q.y, z = q.z, w = q.w };
    }
}
