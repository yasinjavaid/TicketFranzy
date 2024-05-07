using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// Tagging component for use with the LocalNavMeshBuilder
// Supports mesh-filter and terrain - can be extended to physics and/or primitives
[DefaultExecutionOrder(-200)]
public class NavMeshSourceTag : MonoBehaviour
{
    // Global containers for all active mesh/terrain tags
    public static List<MeshFilter> m_Meshes = new List<MeshFilter>();
    public static List<Terrain> m_Terrains = new List<Terrain>();

    void OnEnable()
    {
        if(TryGetComponent(out MeshFilter meshFilter))
            m_Meshes.Add(meshFilter);
        if (TryGetComponent(out Terrain terrain))
            m_Terrains.Add(terrain);
    }

    void OnDisable()
    {
        if (TryGetComponent(out MeshFilter meshFilter))
            m_Meshes.Remove(meshFilter);
        if (TryGetComponent(out Terrain terrain))
            m_Terrains.Remove(terrain);
    }

    // Collect all the navmesh build sources for enabled objects tagged by this component
    public static void Collect(ref List<NavMeshBuildSource> sources)
    {
        sources.Clear();

        for (var i = 0; i < m_Meshes.Count; ++i)
        {
            MeshFilter mf = m_Meshes[i];
            if (mf == null) continue;

            Mesh m = mf.sharedMesh;
            if (m == null) continue;

            NavMeshBuildSource s = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = m,
                transform = mf.transform.localToWorldMatrix,
                area = 0
            };
            sources.Add(s);
        }

        for (var i = 0; i < m_Terrains.Count; ++i)
        {
            Terrain t = m_Terrains[i];
            if (t == null) continue;

            NavMeshBuildSource s = new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Terrain,
                sourceObject = t.terrainData,
                // Terrain system only supports translation - so we pass translation only to back-end
                transform = Matrix4x4.TRS(t.transform.position, Quaternion.identity, Vector3.one),
                area = 0
            };
            sources.Add(s);
        }
    }
}