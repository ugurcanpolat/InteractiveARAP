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
    private int fixed_vertex_count;
    private List<Vector<double>> mesh_vertices;
    private List<Vector<double>> deformed_vertices;
    private List<Matrix<double>> rotations;

    private LU<double> solver;

    public ARAPDeformation(Mesh mesh_, List<List<int>> neighbors_)
    {
        mesh = mesh_;
        neighbors = neighbors_;
        weights = Matrix<double>.Build.Sparse(mesh.vertexCount, mesh.vertexCount);
        laplace_beltrami_opr = Matrix<double>.Build.Sparse(free_vertex_count,
                                                           free_vertex_count);
        foreach(Vector3 vertex in mesh.vertices)
        {
            mesh_vertices.Add(Utilities.ConvertFromUVectorToMNVector(vertex));
        }
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

    private void DeformationPreprocess(List<int> fixed_vertices_)
    {
        fixed_vertices = fixed_vertices_;

        // Initialize vertices_updated_ with Naive Laplacian editing. This method
        // tries to minimize ||Lp' - Lp||^2 with fixed vertices constraints.
        // Get all the dimensions.

        // Initialize vertices_updated_.
        deformed_vertices = new List<Vector<double>>();

        // Note that L = -weight_.
        // Denote y to be the fixed vertices:
        // ||Lp' - Lp|| => ||-Ax - By - Lp|| => ||Ax - (-By + weight_ * p)||
        // => A'Ax = A'(-By + weight_ * p).
        // Build A and B first.
        Matrix<double> A = Matrix<double>.Build.Sparse(mesh_vertices.Count, free_vertex_count);
        Matrix<double> B = Matrix<double>.Build.Sparse(mesh_vertices.Count, fixed_vertex_count);

        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            foreach (int j in free_vertices)
            {
                A[i, j] = weights[i, j];
            }

            foreach (int j in fixed_vertices)
            {
                B[i, j] = weights[i, j];
            }
        }

        // Build A' * A.   
        Matrix<double> left = A.Transpose() * A;
        LU<double> naive_lap_solver = left.LU();

        Matrix<double> meshMatrix = Matrix<double>.Build.Dense(
            mesh_vertices.Count, 3);
        Matrix<double> fixedMatrix = Matrix<double>.Build.Dense(
            fixed_vertex_count, 3);

        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            meshMatrix.SetRow(i, mesh_vertices[i]);
            if (i < fixed_vertex_count)
            {
                fixedMatrix.SetRow(i, mesh_vertices[fixed_vertices[i]]);
            }
        }

        // Build A' * (-By + weight_ * p).
        for (int c = 0; c < 3; c++)
        {
            Vector<double> b = weights * meshMatrix.Column(c) - B * fixedMatrix.Column(c);
            Vector<double> right = A.Transpose() * b;
            Vector<double> x = naive_lap_solver.Solve(right);

            // Write back the solution.
            for (int i = 0; i < free_vertex_count; ++i)
            {
                deformed_vertices[free_vertices[i]][c] = x[i];
            }
        }

        // Write back fixed vertices constraints.
        for (int i = 0; i < fixed_vertex_count; i++)
        {
            deformed_vertices[fixed_vertices[i]] = mesh_vertices[fixed_vertices[i]];
        }

        // Initialize rotations_ with vertices_updated_.
        List<Matrix<double>> edge_product = new List<Matrix<double>>();
        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            Matrix<double> edge_product_ = Matrix<double>.Build.Dense(3, 3);
            foreach (int j in neighbors[i])
            {
                double weight = weights[i, j];
                Matrix<double> edge = mesh_vertices[i].ToRowMatrix() - mesh_vertices[j].ToRowMatrix();
                Matrix<double> edge_update =
                  deformed_vertices[i].ToRowMatrix() - deformed_vertices[j].ToRowMatrix();
                
                edge_product_ += weight * edge * edge_update.Transpose();
                edge_product.Add(edge_product_);
            }
        }

        rotations.Clear();
        for (int v = 0; v < mesh_vertices.Count; ++v)
        {
            Svd<double> svd = edge_product[v].Svd(true);
            Matrix<double> rotation = svd.U.Transpose() * svd.VT.Transpose();
            rotations.Add(rotation.Transpose());
        }
    }

    private void ComputePositions()
    {
        // solving  Lp' = b  (L: laplace_beltrami_opr, p': solution for the
        // deformed vertices, b: right side of (9))
        Matrix<double> b = Matrix<double>.Build.Dense(free_vertex_count, 3);
        for(int i=0; i<free_vertex_count; i++)
        {
            // Sum for all neighbors j of i:  w/2 * (R_i - R_j) * (p_i - p_j)
            foreach(int j in neighbors[i])
            {
                Vector<double> v = weights[i,j] * 0.5 *
                    (rotations[i] + rotations[j]) *
                    (mesh_vertices[i] - mesh_vertices[j]);

                // add w_ij * p'_j if neighbor is fixed
                if(fixed_vertices.Exists(x => x == j))
                {
                    v += weights[i,j] * deformed_vertices[j];
                }

                b[i,0] += v[0];
                b[i,1] += v[1];
                b[i,2] += v[2];
            }
        }
        Matrix<double> solution = solver.Solve(b);

        for (int i = 0; i < free_vertex_count; ++i)
        {
            int pos = free_vertices[i];
            deformed_vertices[pos] = solution.Row(i);
        }
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

    public void SvdDecomp(Matrix<double> p_guess)
    {
        for (int i = 0; i < free_vertex_count; i++)
        {
            var P = Matrix<double>.Build.Dense(3, neighbors[free_vertices[i]].Count);
            var P_prime = Matrix<double>.Build.Dense(neighbors[i].Count, 3);
            var weights_diag = Matrix<double>.Build.DenseIdentity(neighbors[i].Count);

            Vector<double> iweights = Vector<double>.Build.Dense(neighbors[i].Count);

            foreach (int j in neighbors[i])
            {
                Vector3 edge = mesh.vertices[i] - mesh.vertices[j];
                
                Vector<double> edge_update = deformed_vertices[i] - deformed_vertices[j];

                P.SetColumn(j, Utilities.ConvertFromUVectorToMNVector(edge));
                P_prime.SetRow(j, edge_update);
                iweights[j] = weights[i, j];
            }
            weights_diag.SetDiagonal(iweights);
            var Si = P * weights_diag * P_prime;
            var svd = p_guess.Svd(true);
            rotations[i] = (svd.U.Transpose() * svd.VT.Transpose());
        }
    }
}
