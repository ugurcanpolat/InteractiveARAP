using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARAP
{
    private Mesh mesh;
    private Vector3[] vertices;

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
