using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StageObject))]
[CanEditMultipleObjects]
public class MapObjEventEditor : Editor
{
    private StageObject stageObject;
    private MapEventData mapEventData;
    private bool showMapEventDetails = false;
    
    void OnEnable()
    {
        stageObject = (StageObject)target;
        
        if (stageObject.name.Contains("MapObjEvent"))
        {
            mapEventData = StageJsonUtility.ParseMapEventProperty(stageObject.property);
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (stageObject.name.Contains("MapObjEvent"))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Map Event Settings", EditorStyles.boldLabel);
            
            showMapEventDetails = EditorGUILayout.Foldout(showMapEventDetails, "Map Event Details");
            
            if (showMapEventDetails)
            {
                DrawMapEventGUI();
            }
        }
    }
    
    void DrawMapEventGUI()
    {
        if (mapEventData == null)
        {
            mapEventData = new MapEventData();
        }
        
        EditorGUI.BeginChangeCheck();
        
        mapEventData.mapEvent = EditorGUILayout.IntField("Map Event ID", mapEventData.mapEvent);
        
        EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
        if (mapEventData.MoveToPos == null)
            mapEventData.MoveToPos = new Vector3Data();
            
        mapEventData.MoveToPos.x = EditorGUILayout.FloatField("Move To X", mapEventData.MoveToPos.x);
        mapEventData.MoveToPos.y = EditorGUILayout.FloatField("Move To Y", mapEventData.MoveToPos.y);
        mapEventData.MoveToPos.z = EditorGUILayout.FloatField("Move To Z", mapEventData.MoveToPos.z);
        
        mapEventData.fDelayTime = EditorGUILayout.FloatField("Delay Time", mapEventData.fDelayTime);
        mapEventData.fMoveTime = EditorGUILayout.FloatField("Move Time", mapEventData.fMoveTime);
        mapEventData.bLoop = EditorGUILayout.Toggle("Loop", mapEventData.bLoop);
        mapEventData.nType = EditorGUILayout.IntField("Type", mapEventData.nType);
        
        EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
        mapEventData.bCheckPlayer = EditorGUILayout.Toggle("Check Player", mapEventData.bCheckPlayer);
        mapEventData.bCheckEnemy = EditorGUILayout.Toggle("Check Enemy", mapEventData.bCheckEnemy);
        mapEventData.bRunAtInit = EditorGUILayout.Toggle("Run At Init", mapEventData.bRunAtInit);
        
        EditorGUILayout.LabelField("Bounds Settings", EditorStyles.boldLabel);
        mapEventData.B2DX = EditorGUILayout.FloatField("Bounds X", mapEventData.B2DX);
        mapEventData.B2DY = EditorGUILayout.FloatField("Bounds Y", mapEventData.B2DY);
        mapEventData.B2DW = EditorGUILayout.FloatField("Bounds Width", mapEventData.B2DW);
        mapEventData.B2DH = EditorGUILayout.FloatField("Bounds Height", mapEventData.B2DH);
        
        mapEventData.nSetID = EditorGUILayout.IntField("Set ID", mapEventData.nSetID);
        
        if (EditorGUI.EndChangeCheck())
        {
            // Update the property string
            string propertyPrefix = "10,MAPEVENT_OBJ2";
            stageObject.property = StageJsonUtility.SerializeMapEventProperty(mapEventData, propertyPrefix);
            EditorUtility.SetDirty(stageObject);
        }
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Visualize Movement Path"))
        {
            VisualizeMovementPath();
        }
    }
    
    void VisualizeMovementPath()
    {
        if (mapEventData?.MoveToPos == null) return;
        
        Vector3 startPos = stageObject.transform.position;
        Vector3 endPos = mapEventData.MoveToPos.ToVector3();
        
        Debug.DrawLine(startPos, endPos, Color.red, 5f);
        Debug.Log($"Movement path: {startPos} -> {endPos} (Time: {mapEventData.fMoveTime}s)");
    }
    
    void OnSceneGUI()
    {
        if (mapEventData?.MoveToPos == null) return;
        
        Vector3 startPos = stageObject.transform.position;
        Vector3 endPos = mapEventData.MoveToPos.ToVector3();
        
        // Draw movement path
        Handles.color = Color.red;
        Handles.DrawLine(startPos, endPos);
        Handles.DrawWireCube(endPos, Vector3.one * 0.5f);
        
        // Draw bounds
        Handles.color = Color.yellow;
        Vector3 boundsCenter = startPos + new Vector3(mapEventData.B2DX, mapEventData.B2DY, 0);
        Vector3 boundsSize = new Vector3(mapEventData.B2DW, mapEventData.B2DH, 1);
        Handles.DrawWireCube(boundsCenter, boundsSize);
        
        // Handle for move target
        EditorGUI.BeginChangeCheck();
        Vector3 newEndPos = Handles.PositionHandle(endPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(stageObject, "Move Target Position");
            mapEventData.MoveToPos = Vector3Data.FromVector3(newEndPos);
            
            string propertyPrefix = "10,MAPEVENT_OBJ2";
            stageObject.property = StageJsonUtility.SerializeMapEventProperty(mapEventData, propertyPrefix);
            EditorUtility.SetDirty(stageObject);
        }
    }
}
