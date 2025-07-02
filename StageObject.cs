using UnityEngine;

[System.Serializable]
public class StageObject : MonoBehaviour
{
    [Header("Stage Object Data")]
    public string sGroupID = "DefaultGroup";
    public string bundlepath;
    public string property;
    public int clipIndex;
    
    [Header("Snap Settings")]
    public bool snapToGrid = true;
    public bool lockPosition = false;
    public bool lockRotation = false;
    public bool lockScale = false;
    
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastScale;
    
    void Start()
    {
        UpdateLastTransform();
    }
    
    void Update()
    {
        if (HasTransformChanged())
        {
            if (snapToGrid && !lockPosition)
            {
                SnapToGrid();
            }
            UpdateLastTransform();
        }
    }
    
    bool HasTransformChanged()
    {
        return transform.position != lastPosition || 
               transform.rotation != lastRotation || 
               transform.localScale != lastScale;
    }
    
    void UpdateLastTransform()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastScale = transform.localScale;
    }
    
    void SnapToGrid()
    {
        StageDesignerSettings settings = StageDesignerWindow.GetSettings();
        if (settings != null)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Round(pos.x / settings.gridSize) * settings.gridSize;
            pos.y = Mathf.Round(pos.y / settings.gridSize) * settings.gridSize;
            pos.z = Mathf.Round(pos.z / settings.gridSize) * settings.gridSize;
            transform.position = pos;
        }
    }
    
    public GameObjectData ToGameObjectData()
    {
        return new GameObjectData
        {
            sGroupID = this.sGroupID,
            name = gameObject.name,
            position = Vector3Data.FromVector3(transform.position),
            scale = Vector3Data.FromVector3(transform.localScale),
            rotate = QuaternionData.FromQuaternion(transform.rotation),
            bundlepath = this.bundlepath,
            property = this.property
        };
    }
}
