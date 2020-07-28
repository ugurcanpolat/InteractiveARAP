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
    private List<int> free_indices;
    private List<int> fixed_indices;
    private List<int> free_vertices;
    private List<int> fixed_vertices;
    private List<Vector<double>> mesh_vertices;
    private List<Vector<double>> deformed_vertices;
    private List<Matrix<double>> rotations;
    private LU<double> solver;

    private int[] rightFootIndices = { 0, 36, 48, 67, 81, 94, 109, 110, 137, 179,
        181, 189, 206, 209, 216, 229, 231, 232, 235, 239, 240, 241, 293, 296, 307,
        316, 328, 356, 380, 412, 518, 601, 818 };

    public ARAPDeformation(Mesh mesh_, List<List<int>> neighbors_)
    {
        mesh = mesh_;
        neighbors = neighbors_;

        fixed_indices = new List<int>(rightFootIndices);
        free_indices = new List<int>();

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            free_indices.Add(i);
        }

        for (int i = 0; i < fixed_indices.Count; i++)
        {
            free_indices.Remove(fixed_indices[i]);
        }

        weights = Matrix<double>.Build.Sparse(mesh.vertexCount, mesh.vertexCount);
        laplace_beltrami_opr = Matrix<double>.Build.Sparse(free_indices.Count,
                                                           free_indices.Count);

        mesh_vertices = new List<Vector<double>>();

        foreach (Vector3 vertex in mesh.vertices)
        {
            mesh_vertices.Add(Utilities.ConvertFromUVectorToMNVector(vertex));
        }
    }

    public List<Vector<double>> CalculateARAPMesh(Vector3 targetPosition, int vertexIndex)
    {
        return deformed_vertices;
    }

    public void Initializer()
    {
        weights = Matrix<double>.Build.Sparse(mesh.vertexCount, mesh.vertexCount);
        for (int i = 0; i < mesh.triangles.Length; i+=3)
        {
            double[] cotangent = ComputeCotangents(i);

            for (int j = 0; j < 3; j++)
            {
                int idx1 = i + ((j + 1) % 3);
                int idx2 = i + ((j + 2) % 3);
                int first = mesh.triangles[idx1];
                int second = mesh.triangles[idx2];

                double half_cot = cotangent[j] / 2.0d;
                weights[first, second] += half_cot;
                weights[second, first] += half_cot;

                weights[first, first] -= half_cot;
                weights[second, second] -= half_cot;
            }
        }

        laplace_beltrami_opr = Matrix<double>.Build.Sparse(free_indices.Count,
                                                           free_indices.Count);
        for (int i = 0; i < free_indices.Count; ++i)
        {
            int pos = free_indices[i];
            foreach (int neighbor_pos in neighbors[pos])
            {
                double weight = weights[pos, neighbor_pos];
                laplace_beltrami_opr[i, i] += weight;
                if (free_indices.Exists(x => x == neighbor_pos))
                {
                    laplace_beltrami_opr[i, free_indices.FindIndex(x => x == neighbor_pos)] -= weight;
                }
            }
        }

        solver = laplace_beltrami_opr.LU();
    }


    public void DeformationPreprocess(Vector3 target_position, int target_idx)
    {
        free_indices.Remove(target_idx);
        fixed_indices.Add(target_idx);

        Matrix<double> meshMatrix = Matrix<double>.Build.Dense(
            mesh_vertices.Count, 3);
        Matrix<double> fixedMatrix = Matrix<double>.Build.Dense(
            fixed_indices.Count, 3);
        Matrix<double> deformedMatrix = Matrix<double>.Build.Dense(mesh_vertices.Count, 3);

        /*
        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            meshMatrix.SetRow(i, mesh_vertices[i]);
            if (i < fixed_indices.Count)
            {
                fixedMatrix.SetRow(i, mesh_vertices[fixed_indices[i]]);
            }
        }

        fixedMatrix.SetRow(fixed_indices.Count-1,
            Utilities.ConvertFromUVectorToMNVector(target_position));

        Matrix<double> A = Matrix<double>.Build.Sparse(mesh_vertices.Count,
            free_indices.Count);
        Matrix<double> B = Matrix<double>.Build.Sparse(mesh_vertices.Count,
            fixed_indices.Count);

        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            for (int j = 0; j < free_indices.Count; j++)
            {
                A[i, j] = weights[i, free_indices[j]];
            }

            for (int j = 0; j < fixed_indices.Count; j++)
            {
                B[i, j] = weights[i, fixed_indices[j]];
            }
        }

        Matrix<double> left = A.Transpose() * A;
        LU<double> naive_lap_solver = left.LU();

        for (int c = 0; c < 3; c++)
        {
            Vector<double> b = weights * meshMatrix.Column(c) - B * fixedMatrix.Column(c);
            Vector<double> right = A.Transpose() * b;
            Vector<double> x = naive_lap_solver.Solve(right);

            for (int i = 0; i < free_indices.Count; ++i)
            {
                deformedMatrix[free_indices[i], c] = x[i];
            }
        }
        */
        // simple initial guess
        List<Vector<double>> def_vertices = mesh_vertices;
        Vector<double> targetDistance = Utilities.ConvertFromUVectorToMNVector(target_position) - def_vertices[target_idx];
        Vector<double> startPosition = def_vertices[target_idx];
        double maxDistance = 0.0f;
        for(int i=0; i<def_vertices.Count; i++)
        {
            if((def_vertices[i] - def_vertices[target_idx]).L2Norm() > maxDistance)
            {
                 maxDistance = (def_vertices[i] - def_vertices[target_idx]).L2Norm();
            }
        }
        for(int i=0; i<def_vertices.Count; i++)
        {
            double originalDistance = (def_vertices[i] - startPosition).L2Norm() / maxDistance;
            def_vertices[i] += targetDistance * (1 - originalDistance);
            for (int c = 0; c < 3; c++)
            {
                deformedMatrix[i, c] = def_vertices[i][c];
            }
        }


        // initial rotation
        deformed_vertices = new List<Vector<double>>();
        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            deformed_vertices.Add(deformedMatrix.Row(i));
        }

        for (int i = 0; i < fixed_indices.Count; i++)
        {
            deformed_vertices[fixed_indices[i]] = fixedMatrix.Row(i);
        }

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
                
                edge_product_ += weight * edge_update.Transpose() * edge;
                edge_product.Add(edge_product_);
            }
        }

        rotations = new List<Matrix<double>>();
        for (int v = 0; v < mesh_vertices.Count; ++v)
        {
            Svd<double> svd = edge_product[v].Svd(true);
            Matrix<double> rotation = svd.U.Transpose() * svd.VT.Transpose();
            rotations.Add(rotation);
        }
    }

    private void ComputePositions()
    {
        // solving  Lp' = b  (L: laplace_beltrami_opr, p': solution for the
        // deformed vertices, b: right side of (9))
        Matrix<double> b = Matrix<double>.Build.Dense(free_indices.Count, 3);
        for(int i=0; i < free_indices.Count; i++)
        {
            int pos = free_indices[i];
            // Sum for all neighbors j of i:  w/2 * (R_i - R_j) * (p_i - p_j)
            foreach(int j in neighbors[pos])
            {
                Vector<double> v = weights[pos,j] * 0.5 *
                    (rotations[pos] + rotations[j]) *
                    (mesh_vertices[pos] - mesh_vertices[j]);

                // add w_ij * p'_j if neighbor is fixed
                if(fixed_indices.Exists(x => x == j))
                {
                    v += weights[pos,j] * deformed_vertices[j];
                }

                b.SetRow(i, b.Row(i) + v);
            }
        }

        Matrix<double> solution = solver.Solve(b);

        for (int i = 0; i < free_indices.Count; ++i)
        {
            deformed_vertices[free_indices[i]] = solution.Row(i);
        }
    }

    private double ComputeEnergy()
    {
        double total_energy = 0.0;
        for (int i = 0; i < mesh_vertices.Count; i++)
        {
            foreach (int j in neighbors[i])
            {
                double weight = weights[i, j];
                double edge_energy = 0.0d;
                Matrix<double> vec = (deformed_vertices[i].ToRowMatrix() -
                    deformed_vertices[j].ToRowMatrix()).Transpose() -
                    rotations[i] * (mesh_vertices[i].ToRowMatrix() -
                    mesh_vertices[j].ToRowMatrix()).Transpose();
                
                edge_energy = weight * vec.Column(0).L2Norm() * vec.Column(0).L2Norm();
                total_energy += edge_energy;
            }
        }

        return total_energy;
    }

    private double[] ComputeCotangents(int triIndex)
    {
        double[] cotangents = { 0.0d, 0.0d, 0.0d };
        Vector3 A = mesh.vertices[mesh.triangles[triIndex]];
        Vector3 B = mesh.vertices[mesh.triangles[triIndex + 1]];
        Vector3 C = mesh.vertices[mesh.triangles[triIndex + 2]];
        double aSquared = (B - C).sqrMagnitude;
        double bSquared = (C - A).sqrMagnitude;
        double cSquared = (A - B).sqrMagnitude;
        double area4 = Vector3.Cross(B - A, C - A).magnitude * 2.0d;

        cotangents[0] = (bSquared + cSquared - aSquared) / area4;
        cotangents[1] = (cSquared + aSquared - bSquared) / area4;
        cotangents[2] = (aSquared + bSquared - cSquared) / area4;
        return cotangents;
    }

    private void SvdDecomp(Matrix<double> p_guess)
    {
        for (int i = 0; i < free_indices.Count; i++)
        {
            var P = Matrix<double>.Build.Dense(3, neighbors[free_indices[i]].Count);
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
