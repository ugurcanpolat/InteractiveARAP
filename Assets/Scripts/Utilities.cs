using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MathNet.Numerics.LinearAlgebra;

public class Utilities
{
    static public void SaveMeshAsFile(Vector3[] vertices, int[] triangles)
    {
        // TODO: Generalize the naming for different meshes
        using (StreamWriter sw = new StreamWriter("armadillo_saved.off"))
        {
            sw.Write(MeshExportAsOff(vertices, triangles));
        }
        #if UNITY_EDITOR
            Debug.Log("Mesh is saved.");
        #endif
    }

    public static string MeshExportAsOff(Vector3[] vertices, int[] triangles)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Format("OFF"));
        sb.Append(string.Format("\n{0} {1} {2}" , vertices.Length , (triangles.Length) / 3, 0));
        sb.Append(string.Format("\n"));
        foreach (Vector3 v in vertices)
        {
            sb.Append(string.Format("{0} {1} {2} \n", v.x, v.y, v.z));
        }
        
        for (int i = 0; i < (triangles.Length) / 3; i += 1)
        {
            sb.Append(string.Format("3 {0} {1} {2}\n",
                triangles[3 * i], triangles[3 * i + 1], triangles[3 * i + 2]));
        }

        return sb.ToString();
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

    static public Matrix<float> CreateCells(int[] faces, int verticesLength)
    {
        Matrix<float> cells = Matrix<float>.Build.Sparse(verticesLength,
                                                         verticesLength);

        for (int i = 0; i < faces.Length; i += 3)
        {
            cells[faces[i], faces[i+1]] = 1;
            cells[faces[i], faces[i+2]] = 1;
            cells[faces[i+1], faces[i]] = 1;
            cells[faces[i+1], faces[i+2]] = 1;
            cells[faces[i+2], faces[i]] = 1;
            cells[faces[i+2], faces[i+1]] = 1;
        }

        return cells;
    }

    static public void WriteCellsToFile(Matrix<float> cells,
                                        string fName = "armadillo_1k_cells.txt")
    {
        StringBuilder sb = new StringBuilder();
        Vector<float> rowSums = cells.RowSums();

        for (int i = 0; i < cells.RowCount; i++)
        {
            int currentRowSum = 0;
            for (int j = 0; j < cells.ColumnCount; j++)
            {
                if (cells[i, j] == 1)
                {
                    sb.Append(string.Format("{0}", j));
                    currentRowSum++;

                    if (currentRowSum == rowSums[i])
                    {
                        sb.Append(string.Format("\n"));
                        break;
                    }
                    else
                    {
                        sb.Append(string.Format(" "));
                    }
                }

            }
        }

        using (StreamWriter sw = new StreamWriter("Assets/Resources/" + fName))
        {
            sw.Write(sb.ToString());
        }
    }

    static public Matrix<float> ReadCellsFromFile(int verticesLength,
                                                string fName = "armadillo_1k_cells.txt")
    {
        Matrix<float> cells = Matrix<float>.Build.Sparse(verticesLength,
                                                         verticesLength);

        System.IO.StreamReader MyReader = new System.IO.StreamReader(
            "Assets/Resources/" + fName);
    
        string line;

        for (int i = 0;  (line = MyReader.ReadLine()) != null; i++)
        {
            string[] indices = line.Split(' ');
            foreach (string j_s in indices) {
                int j = int.Parse(j_s,
                    System.Globalization.CultureInfo.InvariantCulture);
                cells[i, j] = 1;
            }
        }

        return cells;
    }


    static public List<List<int>> CreateNeighbors(Matrix<float> cells)
    {
        List<List<int>> neighbors = new List<List<int>>();

        for (int i = 0; i < cells.RowCount; i++)
        {
            List<int> currentRowNeighbors = new List<int>();
            for (int j = 0; j < cells.ColumnCount; j++)
            {
                if (cells[i,j] == 1)
                {
                    currentRowNeighbors.Add(j);
                }
            }
            neighbors.Add(currentRowNeighbors);
        }

        return neighbors;
    }


    // Convert from Unity Vector3 to Math.Net Vector
    static public Vector<double> ConvertFromUVectorToMNVector(Vector3 unityVec)
    {
        Vector<double> mathNetVec = Vector<double>.Build.Dense(3);
        mathNetVec[0] = unityVec.x;
        mathNetVec[1] = unityVec.y;
        mathNetVec[2] = unityVec.z;

        return mathNetVec;
    }

    // Convert from Math.Net Vector to Unity Vector3
    static public Vector3 ConvertFromMNVectorToUVector(Vector<double> mathNetVec)
    {
        Vector3 unityVec;
        unityVec.x = (float) mathNetVec[0];
        unityVec.y = (float) mathNetVec[1];
        unityVec.z = (float) mathNetVec[2];

        return unityVec;
    }
}
