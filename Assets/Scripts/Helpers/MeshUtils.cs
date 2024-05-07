using System.Linq;

using UnityEngine;

public class MeshUtils : MonoBehaviour
{
    public static Mesh CombineMeshes( params Mesh[] meshs)
    {
        Mesh combinedMesh = new Mesh();
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        combinedMesh.CombineMeshes(meshs.Select(mesh => new CombineInstance() { mesh = mesh, transform = matrix }).ToArray());
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();
        return combinedMesh;
    }

    public static Vector3[] GenerateRectFaceTris(float sizeX = 1f, float sizeZ = 1f, Vector3 translate = new Vector3(), Quaternion rotation = new Quaternion())
    {
        Vector3[] meshVertices = new Vector3[6];

        // 1____2
        //  |  /
        //  | /
        // 3|/
        meshVertices[0] = (rotation * new Vector3(-sizeX / 2, 0f, sizeZ / 2)) + translate;
        meshVertices[1] = (rotation * new Vector3(sizeX / 2, 0f, sizeZ / 2)) + translate;
        meshVertices[2] = (rotation * new Vector3(-sizeX / 2, 0f, -sizeZ / 2)) + translate;


        //    /|2
        //   / |
        //  /  |
        // 1‾‾‾‾3
        meshVertices[3] = (rotation * new Vector3(-sizeX / 2, 0f, -sizeZ / 2)) + translate;
        meshVertices[4] = (rotation * new Vector3(sizeX / 2, 0f, sizeZ / 2)) + translate;
        meshVertices[5] = (rotation * new Vector3(sizeX / 2, 0f, -sizeZ / 2)) + translate;

        return meshVertices;
    }

    public static Vector2[] GenerateOverlappingUVArrayForTris(int vertLength)
    {
        if (vertLength % 6 != 0)
            Debug.LogError("GenerateOverlappingUVArray(int vertLength): Please pass in a multiple of 6. Can only generate UVs for groups of rect tri faces.");

        Vector2[] meshUVs = new Vector2[vertLength];

        for (int i = 0; i < meshUVs.Length; i += 6)
        {
            // (0, 1) 1____2 (1, 1)
            //         |  /
            //         | /
            // (0, 0) 3|/
            meshUVs[i] = new Vector2(0, 1);
            meshUVs[i + 1] = new Vector2(1, 1);
            meshUVs[i + 2] = new Vector2(0, 0);

            //           /|2 (1, 1)
            //          / |
            //         /  |
            // (0, 0) 1‾‾‾‾3 (1, 0)
            meshUVs[i + 3] = new Vector2(0, 0);
            meshUVs[i + 4] = new Vector2(1, 1);
            meshUVs[i + 5] = new Vector2(1, 0);
        }

        return meshUVs;
    }

    public static int[] GenerateTriArrayForTris(int vertLength) => Enumerable.Range(0, vertLength).ToArray();
}