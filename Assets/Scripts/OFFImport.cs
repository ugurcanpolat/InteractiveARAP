using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// OFFImport parses an .off file based on https://de.wikipedia.org/wiki/Object_File_Format
public class OFFImport : MonoBehaviour
{
    private int corners, areas, edges;
    private List<Vector3> vertex, area;
    
    void Start()
    {
        
    }

    private string line;
    private bool OFFLine;
    private bool infoLine;

    private int lineNum = 0;

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
                continue;

            // Parse Corners, Areas and Edges
            if(OFFLine && !infoLine)
            {
                string[] d = line.Split(' ');
                corners = int.Parse(d[0]);
                areas = int.Parse(d[1]);
                edges = int.Parse(d[2]);

                infoLine = true;
            }
            
            string[] s = line.Split(' ');
            // Parse Vertecis and Areas
            if(s.Length == 3)
            {
                // Vertex
                Vector3 v = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
                vertex.Add(v);
            } else if (s.Length == 4)
            {
                // Area Boundary
                Vector3 v = new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
                area.Add(v);
            }


        }
    }

}
