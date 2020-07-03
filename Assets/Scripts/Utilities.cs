using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utilities
{
    static public void SaveMeshAsFile(Vector3[] vertices, int[] triangles)
    {
        // TODO: Implement save function

        #if UNITY_EDITOR
            Debug.Log("Mesh is saved.");
        #endif
    }
}
