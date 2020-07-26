using System;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

public class ARAPDeformation
{
    private Mesh mesh;
    private List<List<int>> neighbors;
    private Matrix<double> weights;
    private Matrix<double> laplace_beltrami_opr;
    private int[] rightFootIndices = { 0, 36, 48, 67, 81, 94, 109, 110, 137, 179,
        181, 189, 206, 209, 216, 229, 231, 232, 235, 239, 240, 241, 293, 296, 307,
        316, 328, 356, 380, 412, 518, 601, 818 };

    private List<int> free_vertices;
    private List<int> fixed_vertices;
    private int free_vertex_count;
    private int fixed_vertex_coun;

    private LU<double> solver;

    public ARAPDeformation(Mesh mesh_, List<List<int>> neighbors_)
    {
        mesh = mesh_;
        neighbors = neighbors_;
        weights = Matrix<double>.Build.Sparse(mesh.vertexCount, mesh.vertexCount);
        laplace_beltrami_opr = Matrix<double>.Build.Sparse(free_vertex_count,
                                                           free_vertex_count);
    }

    private void Initializer()
    {
        for (int i = 0; i < mesh.triangles.Length / 3; i++)
        {
            double[] cotangent = ComputeCotangents(i);

            for (int j = 0; j < 3; j++)
            {
                int idx1 = i + ((i + 1) % 3);
                int idx2 = i + ((i + 2) % 3);
                int first = mesh.triangles[idx1];
                int second = mesh.triangles[idx2];

                double half_cot = cotangent[j] / 2.0d;
                weights[first, second] += half_cot;
                weights[second, first] += half_cot;

                weights[first, first] -= half_cot;
                weights[second, second] -= half_cot;
            }
        }

        for (int i = 0; i < free_vertices.Count; ++i)
        {
            int pos = free_vertices[i];
            foreach (int neighbor_pos in neighbors[pos])
            {
                double weight = weights[pos, neighbor_pos];
                laplace_beltrami_opr[i, i] += weight;
                if (free_vertices.Exists(x => x == neighbor_pos))
                {
                    laplace_beltrami_opr[i, neighbor_pos] -= weight;
                }
            }
        }

        solver = laplace_beltrami_opr.LU();

    }

    private double[] ComputeCotangents(int triIndex)
    {
        double[] cotangents = { 0.0d, 0.0d, 0.0d };
        Vector3 A = mesh.vertices[mesh.triangles[triIndex]];
        Vector3 B = mesh.vertices[mesh.triangles[triIndex + 1]];
        Vector3 C = mesh.vertices[mesh.triangles[triIndex + 2]];
        double aSquared = (float) Math.Pow(((B - C).magnitude), 2.0d);
        double bSquared = (float) Math.Pow(((C - A).magnitude), 2.0d);
        double cSquared = (float) Math.Pow(((A - B).magnitude), 2.0d);
        double area4 = Vector3.Cross(B - A, C - A).magnitude * 2.0d;

        cotangents[0] = (bSquared + cSquared - aSquared) / area4;
        cotangents[1] = (cSquared + aSquared - bSquared) / area4;
        cotangents[2] = (aSquared + bSquared - cSquared) / area4;
        return cotangents;
    }

    private double ComputeEnergy()
    {
        double total_energy = 0.0;
        //for (int i = 0; i < mesh.vertexCount; ++i)
        //{
        //    foreach (int neighbor in neighbors[i])
        //    {
        //        int j = neighbor.first;
        //        double weight = weight_.coeff(i, j);
        //        double edge_energy = 0.0d;
        //        Vector3 vec = (vertices_updated_.row(i) -
        //            vertices_updated_.row(j)).transpose() -
        //            rotations_[i] * (vertices_.row(i) -
        //            vertices_.row(j)).transpose();
        //        edge_energy = weight * vec.squaredNorm();
        //        total += edge_energy;
        //    }
        //}

        return total_energy;
    }
}
