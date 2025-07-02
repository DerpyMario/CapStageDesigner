using UnityEngine;
using System.Collections.Generic;

public static class SnapSystem
{
    public static Vector3 SnapToGrid(Vector3 position, float gridSize)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }
    
    public static Vector3 SnapToNearestObject(Vector3 position, GameObject[] objects, float snapDistance)
    {
        Vector3 bestPosition = position;
        float closestDistance = snapDistance;
        
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            
            Vector3 objPos = obj.transform.position;
            float distance = Vector3.Distance(position, objPos);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestPosition = objPos;
            }
        }
        
        return bestPosition;
    }
    
    public static Vector3 SnapToVertices(Vector3 position, GameObject[] objects, float snapDistance)
    {
        Vector3 bestPosition = position;
        float closestDistance = snapDistance;
        
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;
            
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            
            foreach (var vertex in vertices)
            {
                Vector3 worldVertex = obj.transform.TransformPoint(vertex);
                float distance = Vector3.Distance(position, worldVertex);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestPosition = worldVertex;
                }
            }
        }
        
        return bestPosition;
    }
    
    public static Vector3 SnapToEdges(Vector3 position, GameObject[] objects, float snapDistance)
    {
        Vector3 bestPosition = position;
        float closestDistance = snapDistance;
        
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            
            Bounds bounds = GetObjectBounds(obj);
            Vector3[] edgePoints = GetBoundsEdgePoints(bounds);
            
            foreach (var point in edgePoints)
            {
                float distance = Vector3.Distance(position, point);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestPosition = point;
                }
            }
        }
        
        return bestPosition;
    }
    
    public static Vector3 SnapToFaces(Vector3 position, GameObject[] objects, float snapDistance)
    {
        Vector3 bestPosition = position;
        float closestDistance = snapDistance;
        
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            
            Bounds bounds = GetObjectBounds(obj);
            Vector3 closestPoint = bounds.ClosestPoint(position);
            float distance = Vector3.Distance(position, closestPoint);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestPosition = closestPoint;
            }
        }
        
        return bestPosition;
    }
    
    static Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;
            
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            return collider.bounds;
            
        return new Bounds(obj.transform.position, Vector3.one);
    }
    
    static Vector3[] GetBoundsEdgePoints(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3 center = bounds.center;
        
        return new Vector3[]
        {
            // Bottom face edges
            new Vector3(min.x, min.y, center.z),
            new Vector3(max.x, min.y, center.z),
            new Vector3(center.x, min.y, min.z),
            new Vector3(center.x, min.y, max.z),
            
            // Top face edges
            new Vector3(min.x, max.y, center.z),
            new Vector3(max.x, max.y, center.z),
            new Vector3(center.x, max.y, min.z),
            new Vector3(center.x, max.y, max.z),
            
            // Side edges
            new Vector3(min.x, center.y, min.z),
            new Vector3(min.x, center.y, max.z),
            new Vector3(max.x, center.y, min.z),
            new Vector3(max.x, center.y, max.z)
        };
    }
}
