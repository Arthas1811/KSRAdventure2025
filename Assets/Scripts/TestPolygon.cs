using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SphereMask : MonoBehaviour
{
    [Header("Paste the Python string here")]
    [TextArea(2, 2)]
    public string UvString = "0.8870081018518519,0.556712962962963|0.9827835648148148,0.5642361111111112|0.9842303240740741,0.4658564814814815|0.8872974537037037,0.45630787037037035";

    [Header("Visual only")]
    public Color fillColor = new Color(0, 1, 1, 0.3f);

    private void OnValidate() => Rebuild();
    private void Awake()      => Rebuild();

    void Rebuild()
    {
        if (string.IsNullOrEmpty(UvString)) return;

        /* 1. parse uv */
        var uv = UvString.Split('|').Select(p => p.Split(',')).Select(a => new Vector2(float.Parse(a[0]), float.Parse(a[1]))).ToList();

        /* 2. build vertices at the sphere’s current radius */
        float radius = 0.7f;          // ← use whatever scale you have
        var verts = new List<Vector3>();
        foreach (var t in uv)
        {
            float u = t.x, v = t.y;
            float lon = u * 2f * Mathf.PI;
            float lat = (1f - v) * Mathf.PI;
            float x = radius * Mathf.Sin(lat) * Mathf.Cos(lon);
            float y = radius * Mathf.Cos(lat);
            float z = radius * Mathf.Sin(lat) * Mathf.Sin(lon);
            verts.Add(new Vector3(x, y, z));
        }

        /* 3. triangulate (fan) */
        /* 3. triangulate (fan) */
        int[] tris = new int[(verts.Count - 2) * 3];
        for (int i = 0; i < verts.Count - 2; i++)
        {
            tris[i * 3 + 0] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }
        System.Array.Reverse(tris);   // flip winding order → faces inside

        /* 4. create mesh */
        Mesh m = new Mesh
        {
            name = "SphereMask",
            vertices = verts.ToArray(),
            triangles = tris,
            uv = uv.ToArray()
        };
        m.RecalculateNormals();

        /* 5. assign */
        GetComponent<MeshFilter>().sharedMesh = m;

        /* 6. transparent material */
        /* 6. transparent material */
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material mat;

        if (mr.sharedMaterial == null || mr.sharedMaterial.name == "Default-Material")
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
            {
                name = "MaskMat"
            };
            mr.sharedMaterial = mat;
        }
        else
        {
            mat = mr.sharedMaterial;
        }

        // correct color + transparency setup for URP
        mat.SetColor("_BaseColor", fillColor);   // ← key line!
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_Blend", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;


    }
}