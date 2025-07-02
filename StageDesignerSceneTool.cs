using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Stage Designer Tool")]
public class StageDesignerSceneTool : EditorTool
{
    private StageDesignerSettings settings;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    
    public override GUIContent toolbarIcon
    {
        get { return new GUIContent("Stage", "Stage Designer Tool"); }
    }
    
    void OnEnable()
    {
        settings = StageDesignerWindow.GetSettings();
    }
    
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;
        
        Event e = Event.current;
        HandleInput(e);
        DrawVisualHelpers();
    }
    
    void HandleInput(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0) // Left mouse button
                {
                    HandleMouseDown(e);
                }
                break;
                
            case EventType.MouseDrag:
                if (e.button == 0 && isDragging)
                {
                    HandleMouseDrag(e);
                }
                break;
                
            case EventType.MouseUp:
                if (e.button == 0)
                {
                    HandleMouseUp(e);
                }
                break;
                
            case EventType.KeyDown:
                HandleKeyDown(e);
                break;
        }
    }
    
    void HandleMouseDown(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            StageObject stageObj = hitObject.GetComponent<StageObject>();
            
            if (stageObj != null)
            {
                Selection.activeGameObject = hitObject;
                isDragging = true;
                lastMousePosition = e.mousePosition;
                e.Use();
            }
        }
    }
    
    void HandleMouseDrag(Event e)
    {
        if (Selection.activeGameObject == null) return;
        
        StageObject stageObj = Selection.activeGameObject.GetComponent<StageObject>();
        if (stageObj == null) return;
        
        Vector2 mouseDelta = e.mousePosition - lastMousePosition;
        Vector3 worldDelta = Camera.current.transform.right * mouseDelta.x * 0.01f +
                            Camera.current.transform.up * -mouseDelta.y * 0.01f;
        
        Undo.RecordObject(stageObj.transform, "Move Stage Object");
        
        Vector3 newPosition = stageObj.transform.position + worldDelta;
        
        // Apply snapping
        if (settings != null && settings.snapToGrid && stageObj.snapToGrid)
        {
            newPosition = SnapSystem.SnapToGrid(newPosition, settings.gridSize);
        }
        
        stageObj.transform.position = newPosition;
        lastMousePosition = e.mousePosition;
        
        e.Use();
    }
    
    void HandleMouseUp(Event e)
    {
        isDragging = false;
        
        if (Selection.activeGameObject != null)
        {
            StageObject stageObj = Selection.activeGameObject.GetComponent<StageObject>();
            if (stageObj != null && settings != null)
            {
                // Final snap check
                Vector3 finalPos = stageObj.transform.position;
                
                if (settings.snapToGrid && stageObj.snapToGrid)
                {
                    finalPos = SnapSystem.SnapToGrid(finalPos, settings.gridSize);
                }
                
                // Check for object snapping
                if (settings.snapToVertices || settings.snapToEdges || settings.snapToFaces)
                {
                    StageObject[] allObjects = Object.FindObjectsOfType<StageObject>();
                    GameObject[] gameObjects = System.Array.ConvertAll(allObjects, obj => obj.gameObject);
                    
                    if (settings.snapToVertices)
                    {
                        Vector3 vertexSnap = SnapSystem.SnapToVertices(finalPos, gameObjects, settings.snapDistance);
                        if (Vector3.Distance(finalPos, vertexSnap) < settings.snapDistance)
                            finalPos = vertexSnap;
                    }
                    
                    if (settings.snapToEdges)
                    {
                        Vector3 edgeSnap = SnapSystem.SnapToEdges(finalPos, gameObjects, settings.snapDistance);
                        if (Vector3.Distance(finalPos, edgeSnap) < settings.snapDistance)
                            finalPos = edgeSnap;
                    }
                    
                    if (settings.snapToFaces)
                    {
                        Vector3 faceSnap = SnapSystem.SnapToFaces(finalPos, gameObjects, settings.snapDistance);
                        if (Vector3.Distance(finalPos, faceSnap) < settings.snapDistance)
                            finalPos = faceSnap;
                    }
                }
                
                stageObj.transform.position = finalPos;
            }
        }
    }
    
    void HandleKeyDown(Event e)
    {
        switch (e.keyCode)
        {
            case KeyCode.G: // Grab/Move mode
                if (Selection.activeGameObject != null)
                {
                    isDragging = true;
                    e.Use();
                }
                break;
                
            case KeyCode.R: // Rotate mode
                if (Selection.activeGameObject != null)
                {
                    RotateSelectedObject();
                    e.Use();
                }
                break;
                
            case KeyCode.S: // Scale mode
                if (Selection.activeGameObject != null)
                {
                    ScaleSelectedObject();
                    e.Use();
                }
                break;
                
            case KeyCode.X: // Delete
                if (Selection.activeGameObject != null)
                {
                    DeleteSelectedObject();
                    e.Use();
                }
                break;
                
            case KeyCode.D: // Duplicate
                if (Selection.activeGameObject != null && e.control)
                {
                    DuplicateSelectedObject();
                    e.Use();
                }
                break;
        }
    }
    
    void RotateSelectedObject()
    {
        if (Selection.activeGameObject == null) return;
        
        Undo.RecordObject(Selection.activeGameObject.transform, "Rotate Stage Object");
        Selection.activeGameObject.transform.Rotate(0, 90, 0);
    }
    
    void ScaleSelectedObject()
    {
        if (Selection.activeGameObject == null) return;
        
        Undo.RecordObject(Selection.activeGameObject.transform, "Scale Stage Object");
        Vector3 scale = Selection.activeGameObject.transform.localScale;
        scale *= 1.1f;
        Selection.activeGameObject.transform.localScale = scale;
    }
    
    void DeleteSelectedObject()
    {
        if (Selection.activeGameObject == null) return;
        
        Undo.DestroyObjectImmediate(Selection.activeGameObject);
        Selection.activeGameObject = null;
    }
    
    void DuplicateSelectedObject()
    {
        if (Selection.activeGameObject == null) return;
        
        GameObject duplicate = Object.Instantiate(Selection.activeGameObject);
        duplicate.transform.position += Vector3.right * (settings?.gridSize ?? 1.0f);
        
        Undo.RegisterCreatedObjectUndo(duplicate, "Duplicate Stage Object");
        Selection.activeGameObject = duplicate;
    }
    
    void DrawVisualHelpers()
    {
        if (settings == null) return;
        
        // Draw selection outline
        if (Selection.activeGameObject != null)
        {
            StageObject stageObj = Selection.activeGameObject.GetComponent<StageObject>();
            if (stageObj != null)
            {
                Handles.color = settings.selectedObjectColor;
                Bounds bounds = GetObjectBounds(Selection.activeGameObject);
                Handles.DrawWireCube(bounds.center, bounds.size);
                
                // Draw snap preview
                if (isDragging && settings.snapToGrid && stageObj.snapToGrid)
                {
                    Vector3 snapPos = SnapSystem.SnapToGrid(stageObj.transform.position, settings.gridSize);
                    Handles.color = Color.green;
                    Handles.DrawWireCube(snapPos, bounds.size);
                    Handles.DrawDottedLine(stageObj.transform.position, snapPos, 2f);
                }
            }
        }
        
        // Draw grid
        if (settings.showGrid)
        {
            DrawSceneGrid();
        }
    }
    
    void DrawSceneGrid()
    {
        Handles.color = new Color(settings.gridColor.r, settings.gridColor.g, settings.gridColor.b, 0.3f);
        
        Vector3 cameraPos = SceneView.currentDrawingSceneView.camera.transform.position;
        float gridSize = settings.gridSize;
        int gridCount = 50;
        
        Vector3 gridCenter = new Vector3(
            Mathf.Round(cameraPos.x / gridSize) * gridSize,
            0,
            Mathf.Round(cameraPos.z / gridSize) * gridSize
        );
        
        // Draw horizontal lines
        for (int i = -gridCount; i <= gridCount; i++)
        {
            Vector3 start = gridCenter + new Vector3(-gridCount * gridSize, 0, i * gridSize);
            Vector3 end = gridCenter + new Vector3(gridCount * gridSize, 0, i * gridSize);
            Handles.DrawLine(start, end);
        }
        
        // Draw vertical lines
        for (int i = -gridCount; i <= gridCount; i++)
        {
            Vector3 start = gridCenter + new Vector3(i * gridSize, 0, -gridCount * gridSize);
            Vector3 end = gridCenter + new Vector3(i * gridSize, 0, gridCount * gridSize);
            Handles.DrawLine(start, end);
        }
    }
    
    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;
            
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            return collider.bounds;
            
        return new Bounds(obj.transform.position, Vector3.one);
    }
}
