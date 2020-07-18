using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARAP
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] rightFootIndices = {0,36,48,67,81,94,109,110,137,179,181,189,206,209,216,229,231,232,235,239,240,241,293,296,307,316,328,356,380,412,518,601,818};

    public void newMesh()
    {
        mesh = GameObject.Find("Mesh").GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
    }

    public void calculateARAPmesh(Vector3 targetPosition, int vertexIndex)
    {
        // TODO calculate static ARAP here
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



        mesh.SetVertices(vertices);
        vertices = mesh.vertices;
        mesh.RecalculateNormals();
    }
}
