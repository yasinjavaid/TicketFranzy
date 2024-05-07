#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

[ExecuteInEditMode]
public class ProceduralPlane : MonoBehaviour
{
    [SerializeField] protected MeshFilter meshFilter;
    [SerializeField] protected MeshCollider meshCollider;
    [SerializeField] protected Mesh mesh;
    [SerializeField, Min(0.01f)] protected float scale = 1;

    protected MeshFilter prevMeshFilter;
    protected float prevScale;

    [ContextMenu("RecalculateMesh")]
    public void RecalculateMesh() => UpdateMesh(scale);

    private void Start() { if (!mesh) GenerateNewMesh(); }

    private void OnValidate()
    {
        if (!mesh) return;

        if (!meshFilter) meshFilter = GetComponent<MeshFilter>();
        if (!meshCollider) meshCollider = GetComponent<MeshCollider>();

        if (meshFilter && meshFilter.sharedMesh != mesh) meshFilter.sharedMesh = mesh;
        if (meshCollider && meshCollider.sharedMesh != mesh) meshCollider.sharedMesh = mesh;

        if (!Application.isPlaying && prevScale != scale)
        {
            RecalculateMesh();
            prevScale = scale;
        }
    }

    public void UpdateMesh(float scale, Vector3 translate = default, Quaternion rotation = default)
    {
        if (!mesh || scale <= 0) return;

        mesh.Clear();

        mesh.vertices = MeshUtils.GenerateRectFaceTris(scale, scale, translate, rotation);
        mesh.uv = MeshUtils.GenerateOverlappingUVArrayForTris(mesh.vertices.Length);
        mesh.triangles = MeshUtils.GenerateTriArrayForTris(mesh.vertices.Length);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    [ContextMenu("GenerateNewMesh")]
    protected bool GenerateNewMesh()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save New Procedural Plane Mesh", "ProceduralPlaneMesh", "asset", "", "Assets/Models");
        if (string.IsNullOrEmpty(path)) return false;

        mesh = new Mesh();
        RecalculateMesh();
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        return mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
    }
}
#endif