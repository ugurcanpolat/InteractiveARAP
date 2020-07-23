﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.Networking.Types;
using System;

public class ARAP
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] rightFootIndices = {0,36,48,67,81,94,109,110,137,179,181,189,206,209,216,229,231,232,235,239,240,241,293,296,307,316,328,356,380,412,518,601,818};
    private List<List<int>> cells = new List<List<int>>();
    public void newMesh()
    {
        mesh = GameObject.Find("Mesh").GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            List<int> neighbours = new List<int>();
            for (int t = 0; t < mesh.triangles.Length; t++)
            {
                if (i == mesh.triangles[t])
                {
                    if(t%3 == 0)
                    {

                        if ((neighbours.IndexOf(mesh.triangles[t + 1]) == -1))
                        {
                            neighbours.Add(mesh.triangles[t + 1]);
                        }
                        if ((neighbours.IndexOf(mesh.triangles[t + 2 ]) == -1))
                        {
                            neighbours.Add(mesh.triangles[t + 2]);
                        }
                    }
                    else if (t % 3 == 1)
                    {
                        if ((neighbours.IndexOf(mesh.triangles[t + 1 ]) == -1))
                        {
                            neighbours.Add(mesh.triangles[t + 1]);
                        }
                        if ((neighbours.IndexOf(mesh.triangles[t - 1]) == -1))
                        {
                            neighbours.Add(mesh.triangles[t - 1]);
                        }
                    }
                    else if (t % 3 == 2)
                    {
                        if((neighbours.IndexOf(mesh.triangles[t - 2]) == -1))
                        {
                            neighbours.Add(mesh.triangles[t - 2]);
                        }
                        if ((neighbours.IndexOf(mesh.triangles[t - 1]) == -1))
                        {
                            neighbours.Add(mesh.triangles[t - 1]);
                        }

                        neighbours.Add(mesh.triangles[t - 1]);
                    }
                }
            }
            cells.Add(neighbours);
            
        }
        
    }

    public void calculateARAPmesh(Vector3 targetPosition, int vertexIndex)
    {
        
        
        /*
        // non rigid deformation as initial guess
        Vector3 targetDistance = targetPosition - vertices[vertexIndex];
        Vector3 startPosition = vertices[vertexIndex];
        float maxDistance = 0.0f;
        for(int i=0; i<vertices.Length ; i++)
        {
            if((vertices[i] - vertices[vertexIndex]).magnitude > maxDistance)
            {
                maxDistance = (vertices[i] - vertices[vertexIndex]).magnitude;
            }
        }
        for(int i=0; i<vertices.Length ; i++)
        {
            float originalDistance = (vertices[i] - startPosition).magnitude / maxDistance;
            vertices[i] += targetDistance * (1 - originalDistance);
        }

        // ARAP calculations
        float energyImprovement = 0.0f;
        float improvementThreshhold = 0.1f;
        Matrix<float> R = Matrix<float>.Build.Dense(3,3);
        while(energyImprovement < improvementThreshhold)
        {
            float energy = 0.0f;
            // calculate the rotation
            // calculate new positions
            // calculate new energy
            //for( all vertices )
            //{
                //for(all neighbors )
                //{
                    //newEnergy = ((transformedVertex - transformedNeighbor) - R * (vertex - neighbor)).Length 
                    //energyImprovement = energy - newEnergy
                //}
            //}
        }
        
        mesh.SetVertices(vertices);
        vertices = mesh.vertices;
        mesh.RecalculateNormals();
        */
    }
}
