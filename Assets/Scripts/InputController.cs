using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class InputController : MonoBehaviour
{
    public GameObject controlPanel;
    public GameObject cameraOrbit;
    public GameObject mesh;

    public float rotateSpeed = 20f;

    private Mesh meshFilter;
    private Vector3[] vertices;
    private bool mouseDown = false;
    private int leftHandIndex = 770;

    private List<List<int>> neighbors;

    private ARAPDeformation arapDeformation;

    public void newMesh(List<List<int>> neighbors_)
    {
        meshFilter = mesh.GetComponent<MeshFilter>().mesh;
        vertices = meshFilter.vertices;
        neighbors = neighbors_;
        arapDeformation = new ARAPDeformation(meshFilter, neighbors);
        arapDeformation.Initializer();
    }

    void Update()
    {
        // Interaction input control   
        if (Input.GetMouseButton(0))
        {
            float h = rotateSpeed * Input.GetAxis("Mouse X");
            float v = rotateSpeed * Input.GetAxis("Mouse Y");

            if (cameraOrbit.transform.eulerAngles.z + v <= 0.1f
                || cameraOrbit.transform.eulerAngles.z + v >= 179.9f)
            {
                v = 0;
            }

            cameraOrbit.transform.eulerAngles =
                new Vector3(
                    cameraOrbit.transform.eulerAngles.x,
                    cameraOrbit.transform.eulerAngles.y + h,
                    cameraOrbit.transform.eulerAngles.z + v);
        }

        float scrollFactor = Input.GetAxis("Mouse ScrollWheel");

        if (scrollFactor != 0)
        {
            cameraOrbit.transform.localScale =
            cameraOrbit.transform.localScale * (1f - scrollFactor);
        }

        // Deformation control
        if(Input.GetMouseButtonUp(1))
        {
            mouseDown = false;
        }

        if(Input.GetMouseButtonDown(1) || mouseDown)
        {
            mouseDown = true;

            Vector3 cameraPos = Camera.main.transform.position;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 screenPoint = ray.GetPoint(0);
            Vector3 direction = Vector3.Normalize(screenPoint - cameraPos);

            Vector3 target_point = cameraPos +
                direction * (vertices[leftHandIndex] - cameraPos).magnitude;

            arapDeformation.DeformationPreprocess(target_point, leftHandIndex);
            List<Vector<double>> deformed = arapDeformation.CalculateARAPMesh(target_point, leftHandIndex);

            Vector3[] deformedVertices = new Vector3[deformed.Count];

            for(int i = 0; i < deformed.Count; i++)
            {
                deformedVertices[i] = Utilities.ConvertFromMNVectorToUVector(deformed[i]);
            }
            
            meshFilter.SetVertices(deformedVertices);
            meshFilter.RecalculateNormals();
        }
    }
}
