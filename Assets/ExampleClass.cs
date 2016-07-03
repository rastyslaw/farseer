using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleClass : MonoBehaviour
{
    [SerializeField]
    private Material material;

    void Start()
    {
        /*
        List<Vector2> points = new List<Vector2>()
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };
        List<Vector2> triangles = Triangulation.GetResult(points, true);

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>(); 
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) };
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1) };
        mesh.triangles = new int[] { 0, 1, 2, 0, 3, 1};

        */

        List<Vector2> points = new List<Vector2>()
        {
            new Vector2(-1, -1),
            new Vector2(-1, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, -1)
        };
         
        Vector3[] verticles;
        int[] triangles;
        Vector2[] uvcoords;

        Triangulation.GetResult(points, true, Vector3.forward, out verticles, out triangles, out uvcoords);

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;  
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();
        mesh.vertices = verticles;
        mesh.uv = uvcoords;
        mesh.triangles = triangles;
    }
}
