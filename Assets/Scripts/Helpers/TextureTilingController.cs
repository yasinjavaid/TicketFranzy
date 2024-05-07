#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

[ExecuteInEditMode]
public class TextureTilingController : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material material;
    [SerializeField, Min(0.01f)] protected float textureToMeshZ = 1;

    protected MeshRenderer prevRenderer;
    protected Material prevMaterial;
    protected Vector3 prevScale;
    protected float prevTextureToMeshZ;
    private const float UNITY_PLANE_SIZE = 10;

    private void Start()
    {
        if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer && meshRenderer.sharedMaterial && !material) DuplicateMaterial();
    }

    private void OnValidate()
    {
        if (!meshRenderer || !material) return;

        if (meshRenderer != prevRenderer ||
            material != prevMaterial ||
            gameObject.transform.lossyScale != prevScale ||
            !Mathf.Approximately(textureToMeshZ, prevTextureToMeshZ))
        {
            UpdateTiling();
            if (meshRenderer) meshRenderer.sharedMaterial = material;
            prevRenderer = meshRenderer;
            prevMaterial = material;
            prevScale = gameObject.transform.lossyScale;
            prevTextureToMeshZ = textureToMeshZ;
        }
    }

    [ContextMenu("UpdateTiling")]
    void UpdateTiling()
    {
        if (!material) return;

        float textureToMeshX = (float)material.mainTexture.width / material.mainTexture.height * textureToMeshZ;

        material.mainTextureScale = new Vector2(
            UNITY_PLANE_SIZE * gameObject.transform.lossyScale.x / textureToMeshX,
            UNITY_PLANE_SIZE * gameObject.transform.lossyScale.z / textureToMeshZ);
    }

    [ContextMenu("DuplicateMaterial")]
    protected bool DuplicateMaterial()
    {
        if (!(meshRenderer && meshRenderer.sharedMaterial)) return false;
        material = new Material(meshRenderer.sharedMaterial);
        string path = EditorUtility.SaveFilePanelInProject("Save New Tiling Material", material.name, "mat", "", "Assets/Materials");
        if (string.IsNullOrEmpty(path)) return false;

        AssetDatabase.CreateAsset(material, path);
        AssetDatabase.SaveAssets();
        material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material && meshRenderer)
            meshRenderer.sharedMaterial = material;
        return material;
    }
}
#endif