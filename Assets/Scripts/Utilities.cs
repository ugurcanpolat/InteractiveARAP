using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class Utilities
{
    static public void SaveMeshAsFile(Vector3[] vertices, int[] triangles)
    {
        // TODO: Implement save function
        using (StreamWriter sw = new StreamWriter("armadillo" /*change with mesh_options.armadillo */ + "_saved.obj"))
        {
            sw.Write(MeshExportAsObj(vertices, triangles));
        }
#if UNITY_EDITOR
        Debug.Log("Mesh is saved.");
        #endif
    }

    public static string MeshExportAsObj(Vector3[] vertices, int[] triangles)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Format("####"));
        sb.Append(string.Format("\n#  Vertices: " + vertices.Length));
        sb.Append(string.Format("\n#  Faces: " + (triangles.Length) / 3));
        sb.Append(string.Format("\n#"));
        sb.Append(string.Format("\n####\n"));
        foreach (Vector3 v in vertices)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.normalized.x, v.normalized.y, v.normalized.z));
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append(string.Format("\n# {0} vertices, {1} vertices normals ", vertices.Length, vertices.Length));

        sb.Append("\n");
        for (int i = 0; i < (triangles.Length) / 3; i += 1)
        {
            sb.Append(string.Format("f {0}//{0} {1}//{1} {2}//{2}\n",
                triangles[3 * i], triangles[3 * i + 1], triangles[3 * i + 2]));
        }
        sb.Append(string.Format("\n# {0} faces, {1} coords texture", (triangles.Length) / 3, 0));
        sb.Append("\n");
        sb.Append("# End of file");
        return sb.ToString();
    }


    static public Mesh CreateMeshFromOFFFile(string file_path)
    {
        System.IO.StreamReader MyReader = new System.IO.StreamReader(file_path);

        List<Vector3> vertices = new List<Vector3>();
        int[] faces = null;

        string line;
        bool is_off_line = false;
        bool is_info_line = false;

        int line_count = 0;
        int vertex_count = 0;
        int face_count = 0;
        int num_vertices = 0;
        int num_faces = 0;

        while ((line = MyReader.ReadLine()) != null)
        {
            line_count++;

            // Make sure the first line says "OFF"
            if (line_count == 1 && line == "OFF")
            {
                is_off_line = true;
                continue;
            }
            else if (line_count == 1 && line != "OFF")
            {
                break;
            }

            // Ignore all comments and empty lines
            if (line[0] == '#' || line == "")
            {
                #if UNITY_EDITOR
                    Debug.Log("Line " + line_count + " in .off file is ignored.");
                #endif

                continue;
            }

            string[] s = line.Split(' ');
            // Parse numbers of vertices and faces
            if (is_off_line && !is_info_line)
            {
                num_vertices = int.Parse(s[0]);
                num_faces = int.Parse(s[1]);

                faces = new int[num_faces * 3];

                is_info_line = true;
                continue;
            }

            // Parse Vertices and Faces
            if (s.Length == 4 && vertex_count < num_vertices)
            {
                // Vertex
                Vector3 v = new Vector3(
                    float.Parse(
                        s[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(
                        s[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(
                        s[2], System.Globalization.CultureInfo.InvariantCulture)
                    );

                vertices.Add(v);
                vertex_count++;
            }

            else if (vertex_count >= num_vertices && face_count < num_faces)
            {
                // Vertices of a face
                faces[face_count*3] = int.Parse(
                    s[1], System.Globalization.CultureInfo.InvariantCulture);
                faces[face_count * 3 + 1] = int.Parse(
                    s[2], System.Globalization.CultureInfo.InvariantCulture);
                faces[face_count * 3 + 2] = int.Parse(
                    s[3], System.Globalization.CultureInfo.InvariantCulture);

                face_count++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.triangles = faces;
        mesh.RecalculateNormals();

        return mesh;
    }
}
