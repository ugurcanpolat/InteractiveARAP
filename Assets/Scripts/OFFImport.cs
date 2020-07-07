using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// OFFImport parses an .off file based on https://de.wikipedia.org/wiki/Object_File_Format
public class OFFImport : MonoBehaviour
{
    private int corners, areas, edges;
    private List<Vector3> vertex, area;

    [SerializeField]
    private string path;

    void Start()
    {
        vertex = new List<Vector3>();
        area = new List<Vector3>();
        ParseFile(path);
    }

    private string line;
    private bool OFFLine = false;
    private bool infoLine = false;

    private int lineNum = 0;
    private int vertexNum = 0;
    private int areaNum = 0;

    void ParseFile(string filepath)
    {
        System.IO.StreamReader MyReader = new System.IO.StreamReader(filepath);

        while ((line = MyReader.ReadLine()) != null)
        {
            lineNum++;

            // Make sure the first line says "OFF"
            if (lineNum == 1 && line == "OFF")
            {
                OFFLine = true;
                continue;
            }
            else if (lineNum == 1 && line != "OFF")
            {
                OFFLine = false;
                break;
            }

            // Ignore all comments and empty lines
            if (line[0] == '#' || line == "")
            {
                Debug.Log("Ignored line " + lineNum + " while parsing the .off file.");
                continue;
            }

            // Parse Corners, Areas and Edges
            if(OFFLine && !infoLine)
            {
                string[] d = line.Split(' ');
                corners = int.Parse(d[0]);
                areas = int.Parse(d[1]);
                edges = int.Parse(d[2]);

                infoLine = true;
                continue;
            }
            
            string[] s = line.Split(' ');
            // Parse Vertecis and Areas
            if (s.Length == 4 && vertexNum < corners)
            {
                vertexNum++;
                // Vertex
                Vector3 v = new Vector3(float.Parse(s[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(s[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(s[2], System.Globalization.CultureInfo.InvariantCulture));
                vertex.Add(v);
            } else if (vertexNum >= corners && areaNum < areas)
            {
                areaNum++;
                // Area Boundary
                Vector3 v = new Vector3(float.Parse(s[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(s[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(s[2], System.Globalization.CultureInfo.InvariantCulture));
                area.Add(v);
            }


        }
    }

    private Vector3[] ListToVertexArray(List<Vector3> list)
    {
        Vector3[] a = new Vector3[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            a[i] = list[i];
        }
        return a;
    }

    private int[] ListToIntArray(List<Vector3> list)
    {
        int[] a = new int[list.Count * 3];
        int j = 0;
        for(int i = 0; i < list.Count; i++)
        {
            a[j] = (int) list[i].x;
            j++;
            a[j] = (int)list[i].y;
            j++;
            a[j] = (int)list[i].z;
            j++;
        }
        return a;
    }

    // Builds a Mesh object based on https://docs.unity3d.com/ScriptReference/Mesh.html
    Mesh CreateMesh()
    {
        Mesh m = new Mesh();
        m.vertices = ListToVertexArray(vertex);
        m.triangles = ListToIntArray(area);

        return m;
    }

}
