using UnityEngine;

public class ClipBounds : MonoBehaviour
{
    [Header("Clip Bounds")]
    public float minX;
    public float maxX;
    public float minY = -50f;
    public float maxY = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;
    
    [Header("Visual Settings")]
    public Color boundsColor = Color.cyan;
    public bool showBounds = true;
    public bool showLabels = true;
    
    void OnDrawGizmos()
    {
        if (!showBounds) return;
        
        Gizmos.color = boundsColor;
        
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        
        Gizmos.DrawWireCube(center, size);
        
        if (showLabels)
        {
            Vector3 labelPos = new Vector3(center.x, maxY + 1f, center.z);
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"Clip: {minX:F1} - {maxX:F1}");
            #endif
        }
    }
    
    public bool ContainsPoint(Vector3 point)
    {
        return point.x >= minX && point.x <= maxX &&
               point.y >= minY && point.y <= maxY &&
               point.z >= minZ && point.z <= maxZ;
    }
    
    public bool ContainsObject(GameObject obj)
    {
        return ContainsPoint(obj.transform.position);
    }
    
    public void FitToContents()
    {
        StageObject[] objects = GetComponentsInChildren<StageObject>();
        if (objects.Length == 0) return;
        
        float newMinX = float.MaxValue;
        float newMaxX = float.MinValue;
        
        foreach (var obj in objects)
        {
            Vector3 pos = obj.transform.position;
            newMinX = Mathf.Min(newMinX, pos.x);
            newMaxX = Mathf.Max(newMaxX, pos.x);
        }
        
        minX = newMinX - 1f;
        maxX = newMaxX + 1f;
    }
}
