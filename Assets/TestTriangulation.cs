using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

public class TestTriangulation : MonoBehaviour {

    public GameObject cube;

	void Start ()
	{
        //World world = FSWorldComponent.PhysicsWorld;
        //world.BodyRemoved += BodyRemoved;
        
	    MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
	    Body body = cube.GetComponent<FSBodyComponent>().PhysicsBody;
	    body.UserData = meshRenderer;

        Debug.Log(body.UserData); 
	}

    void Update()
    {
        List<Body> bodies = FSWorldComponent.PhysicsWorld.BodyList;
        MeshRenderer meshRenderer;
        foreach (var body in bodies)
        {
            if (body.UserData != null)
            {
                meshRenderer = body.UserData as MeshRenderer;
                meshRenderer.gameObject.transform.position = new Vector3(body.Position.X, body.Position.Y);
                meshRenderer.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, body.Rotation * 180 / Mathf.PI));
            }
        }
    }

    private void BodyRemoved(Body body)
    {
        MeshRenderer meshRenderer = body.UserData as MeshRenderer;
        body.UserData = null;
        Destroy(meshRenderer.gameObject);
    }

    public static void DeleteBody(Body body)
    {
        World world = FSWorldComponent.PhysicsWorld;
        world.RemoveBody(body);

        MeshRenderer meshRenderer = body.UserData as MeshRenderer;
        body.UserData = null;
        Destroy(meshRenderer.gameObject);
    }

    public static void Cut(World world, FVector2 start, FVector2 end, float thickness)
    {
        List<Fixture> fixtures = new List<Fixture>();
        List<FVector2> entryPoints = new List<FVector2>();
        List<FVector2> exitPoints = new List<FVector2>();
        
        if (world.TestPoint(start) != null || world.TestPoint(end) != null)
            return;
        
        world.RayCast((f, p, n, fr) =>
        {
            fixtures.Add(f);
            entryPoints.Add(p);
            return 1;
        }, start, end);
        
        world.RayCast((f, p, n, fr) =>
        {
            exitPoints.Add(p);
            return 1;
        }, end, start);
        
        if (entryPoints.Count + exitPoints.Count < 2)
            return;

        for (int i = 0; i < fixtures.Count; i++)
        {
            if (fixtures[i].Shape.ShapeType != ShapeType.Polygon)
                continue;

            if (fixtures[i].Body.BodyType != BodyType.Static)
            {
                Vertices first;
                Vertices second;
                CuttingTools.SplitShape(fixtures[i], entryPoints[i], exitPoints[i], thickness, out first, out second);

                MeshRenderer meshRenderer = (MeshRenderer)fixtures[i].Body.UserData; 

                if (CuttingTools.SanityCheck(first)) 
                {
                    Body firstFixture = BodyFactory.CreatePolygon(world, first, fixtures[i].Shape.Density,
                                                                        fixtures[i].Body.Position);
                    firstFixture.Rotation = fixtures[i].Body.Rotation;
                    firstFixture.LinearVelocity = fixtures[i].Body.LinearVelocity;
                    firstFixture.AngularVelocity = fixtures[i].Body.AngularVelocity;
                    firstFixture.BodyType = BodyType.Dynamic;

                    GameObject firstGameObject = new GameObject("firstGameObject");
                    MeshRenderer firstMeshRenderer = firstGameObject.AddComponent<MeshRenderer>();
                    firstMeshRenderer.material = new Material(meshRenderer.material);

                    firstFixture.UserData = firstMeshRenderer; 

                    List<Vector2> points = new List<Vector2>();
                    foreach (var p in first)
                    {
                        points.Add(new Vector2(p.X, p.Y));
                    }

                    Vector3[] verticles; 
                    int[] triangles;
                    Vector2[] uvcoords;

                    Triangulation.GetResult(points, false, Vector3.back, out verticles, out triangles, out uvcoords);
                    
                    MeshFilter meshFilter = firstGameObject.AddComponent<MeshFilter>();
                    Mesh mesh = meshFilter.mesh;
                    mesh.Clear();
                    mesh.vertices = verticles;
                    mesh.uv = uvcoords;
                    mesh.triangles = triangles;

                    FVector2 oldPosition = fixtures[i].Body.Position;
                    firstGameObject.transform.position = new Vector3(oldPosition.X, oldPosition.Y);
                    firstGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, fixtures[i].Body.Rotation * 180 / Mathf.PI));
                }

                if (CuttingTools.SanityCheck(second))
                {
                    Body secondFixture = BodyFactory.CreatePolygon(world, second, fixtures[i].Shape.Density,
                                                                         fixtures[i].Body.Position);
                    secondFixture.Rotation = fixtures[i].Body.Rotation;
                    secondFixture.LinearVelocity = fixtures[i].Body.LinearVelocity;
                    secondFixture.AngularVelocity = fixtures[i].Body.AngularVelocity;
                    secondFixture.BodyType = BodyType.Dynamic;
                }
                DeleteBody(fixtures[i].Body);
            }
        }
    }
}
