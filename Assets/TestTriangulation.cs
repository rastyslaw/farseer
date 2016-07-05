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
    private World world;

	void Start ()
	{
        world = FSWorldComponent.PhysicsWorld;
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

    private void DeleteBody(Body body)
    {
        World world = FSWorldComponent.PhysicsWorld;
        world.RemoveBody(body);

        MeshRenderer meshRenderer = body.UserData as MeshRenderer;
        body.UserData = null;
        Destroy(meshRenderer.gameObject);
    }

    public void Cut(World world, FVector2 start, FVector2 end, float thickness = 0)
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
                
                if (CuttingTools.SanityCheck(first)) 
                {
                    CreateNewObject(fixtures[i], first);  
                } 

                if (CuttingTools.SanityCheck(second))
                {
                    CreateNewObject(fixtures[i], second);
                }
                DeleteBody(fixtures[i].Body);
            }
        }
    }

    private void CreateNewObject(Fixture fixture, Vertices vertices)
    {
        Body firstFixture = BodyFactory.CreatePolygon(world, vertices, fixture.Shape.Density,
                                                                        fixture.Body.Position);
        firstFixture.Rotation = fixture.Body.Rotation;
        firstFixture.LinearVelocity = fixture.Body.LinearVelocity;
        firstFixture.AngularVelocity = fixture.Body.AngularVelocity;
        firstFixture.BodyType = BodyType.Dynamic;

        MeshRenderer meshRenderer = (MeshRenderer)fixture.Body.UserData; 

        GameObject firstGameObject = new GameObject("cuttingObject");
        MeshRenderer firstMeshRenderer = firstGameObject.AddComponent<MeshRenderer>();
        firstMeshRenderer.material = new Material(meshRenderer.material);

        firstFixture.UserData = firstMeshRenderer;

        List<Vector2> points = new List<Vector2>();
        foreach (var p in vertices)
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

        FVector2 oldPosition = fixture.Body.Position;
        firstGameObject.transform.position = new Vector3(oldPosition.X, oldPosition.Y);
        firstGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, fixture.Body.Rotation * 180 / Mathf.PI));
    }
}
