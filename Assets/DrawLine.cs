using System;
using System.Collections.Generic;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    [SerializeField]
    private Material material;

    private LineRenderer line;
    private Vector3 mousePos;
    private Vector3 startMousePos;

    [SerializeField]
    private GameObject point1;
    [SerializeField]
    private GameObject point2;
   
    void Start()
    {
        CreateLine();
        point1.SetActive(false);
        point2.SetActive(false);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            line.gameObject.SetActive(true);
            startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //set the z co ordinate to 0 as we are only interested in the xy axes
            startMousePos.z = 0;
            //set the start point and end point of the line renderer
            line.SetPosition(0, startMousePos);
            line.SetPosition(1, startMousePos);
        }
        //if mouse button is held clicked and line exists
        else if (Input.GetMouseButton(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            //set the end position as current position but dont set line as null as the mouse click is not exited
            line.SetPosition(1, mousePos);
            PlacePoints();
        }
        //if line renderer exists and left mouse button is click exited (up)
        else if (Input.GetMouseButtonUp(0))
        {
            line.gameObject.SetActive(false);
            point1.SetActive(false);
            point2.SetActive(false);
            
            TestTriangulation.Cut(
                FSWorldComponent.PhysicsWorld, 
                new FVector2(startMousePos.x, startMousePos.y), 
                new FVector2(mousePos.x, mousePos.y), 
                0.1f);
        }
    }

    private void PlacePoints()
    {
        World world = FSWorldComponent.PhysicsWorld;

        List<Fixture> fixtures = new List<Fixture>();
        List<FVector2> entryPoints = new List<FVector2>();
        List<FVector2> exitPoints = new List<FVector2>();

        FVector2 start = new FVector2(startMousePos.x, startMousePos.y);
        FVector2 end = new FVector2(mousePos.x, mousePos.y);

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

        point1.SetActive(true);
        point2.SetActive(true);

        point1.transform.position = new Vector3(entryPoints[0].X, entryPoints[0].Y, 0);
        point2.transform.position = new Vector3(exitPoints[0].X, exitPoints[0].Y, 0); 
    }

    private void CreateLine()
    {
        line = new GameObject("line").AddComponent<LineRenderer>();
        line.material = material;
        //set the number of points to the line
        line.SetVertexCount(2);
        //set the width
        line.SetWidth(0.05f, 0.05f);
        //render line to the world origin and not to the object's position
        line.useWorldSpace = true;
        line.gameObject.SetActive(false);
    }
}

