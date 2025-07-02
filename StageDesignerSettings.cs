using UnityEngine;

[CreateAssetMenu(fileName = "StageDesignerSettings", menuName = "Stage Designer/Settings")]
public class StageDesignerSettings : ScriptableObject
{
    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public bool snapToGrid = true;
    public Color gridColor = Color.white;
    public bool showGrid = true;
    
    [Header("Snap Settings")]
    public float snapDistance = 0.5f;
    public bool snapToVertices = true;
    public bool snapToEdges = true;
    public bool snapToFaces = true;
    
    [Header("Stage Settings")]
    public float defaultClipWidth = 25.0f;
    public string defaultGroupID = "DefaultGroup";
    public string prefabBasePath = "Assets/";
    
    [Header("Visual Settings")]
    public Color selectedObjectColor = Color.yellow;
    public Color clipBoundsColor = Color.cyan;
    public float handleSize = 0.1f;
}
