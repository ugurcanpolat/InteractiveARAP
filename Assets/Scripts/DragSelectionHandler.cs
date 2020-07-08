using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragSelectionHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector2 startPosition;
    Rect selectionRect;
    Mesh mesh;
    List<List<int>> cellIndices = new List<List<int>>();
    List<GameObject> selectionSpheres = new List<GameObject>();
    List<GameObject> allSpheres = new List<GameObject>();
    Color c;

    public void newMesh()
    {
        //mesh= mesh.GetComponent<MeshFilter>().mesh;
        mesh = GameObject.Find("Mesh").GetComponent<MeshFilter>().mesh;
        Debug.Log("mesh has " + mesh.vertices.Length + " vertices");
        mesh.colors = new Color[mesh.vertices.Length];
        mesh.colors32 = new Color32[mesh.vertices.Length];
        c = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        cellIndices.Add(new List<int>());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        startPosition = eventData.position;
        selectionRect = new Rect();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        selectionRect.xMin = Mathf.Min(eventData.position.x, startPosition.x);
        selectionRect.xMax = Mathf.Max(eventData.position.x, startPosition.x);
        selectionRect.yMin = Mathf.Min(eventData.position.y, startPosition.y);
        selectionRect.yMax = Mathf.Max(eventData.position.y, startPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        int num_vert = 0;
        for(int i=0; i<mesh.vertices.Length; i++)
        {
            if(selectionRect.Contains(Camera.main.WorldToScreenPoint(mesh.vertices[i])))
            {
                num_vert++;
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = mesh.vertices[i] + new Vector3(Random.Range(-0.001f, 0.001f), Random.Range(-0.001f, 0.001f), Random.Range(-0.001f, 0.001f));
                sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                sphere.GetComponent<Renderer>().material.color = c;
                selectionSpheres.Add(sphere);
                cellIndices[cellIndices.Count - 1].Add(i);
            }
        }
        Debug.Log("selected " + num_vert + " vertices");
        mesh.RecalculateNormals();
    }
    
    void Update()
    {
        if (Input.GetKeyDown("return"))
        {
            // save all selections
            foreach(GameObject sphere in selectionSpheres)
            {
                GameObject new_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                new_sphere.transform.position = sphere.transform.position;
                new_sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                new_sphere.GetComponent<Renderer>().material.color = c;
                allSpheres.Add(new_sphere);
                Destroy(sphere);
            }
            selectionSpheres.Clear();
            c = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            cellIndices.Add(new List<int>());
        }
        if (Input.GetKeyDown("backspace"))
        {
            // remove all selections
            cellIndices[cellIndices.Count - 1].Clear();
            foreach(GameObject sphere in selectionSpheres)
            {
                Destroy(sphere);
            }
            selectionSpheres.Clear();
            c = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        if (Input.GetKeyDown("p"))
        {
            // print indices of cells
            string output = "[";
            foreach(List<int> cell in cellIndices)
            {
                output += "[";
                output += string.Join(",", cell);
                output += "], ";
            }
            output += "]";
            Debug.Log(output);
        }
    }
}
