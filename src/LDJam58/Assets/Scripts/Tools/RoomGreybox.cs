using UnityEngine;

/// <summary>
/// Simple greybox room component. Scales the attached mesh to the desired width/depth/height
/// and draws editor-time labels and outlines.
/// </summary>
using System.Collections.Generic;

[ExecuteAlways]
public class RoomGreybox : MonoBehaviour
{
    [SerializeField] private string _roomName = "Room";
    [SerializeField] private Vector2 _sizeXz = new Vector2(10f, 10f); // x = width, y = depth
    [SerializeField] private float _heightY = 3f;
    [SerializeField] private Color _color = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private bool _randomizeColorOnBuild = true;
    [SerializeField] private bool _isCorridor = false;
    [SerializeField] private bool _buildCeiling = false;

    public string RoomName => _roomName;
    public Vector2 SizeXz => _sizeXz;
    public float HeightY => _heightY;

    private void OnEnable()
    {
        RegenerateGeometry();
    }

    private void OnValidate()
    {
        if (_sizeXz.x < 1f) _sizeXz.x = 1f;
        if (_sizeXz.y < 1f) _sizeXz.y = 1f;
        if (_heightY < 0.5f) _heightY = 0.5f;

        if (!string.IsNullOrEmpty(_roomName) && gameObject.name != _roomName)
        {
            gameObject.name = _roomName;
        }

        RegenerateGeometry();
    }

    private void RegenerateGeometry()
    {
        // Ensure a clean parent for generated geometry
        var geometryRoot = transform.Find("Geometry");
        if (geometryRoot == null)
        {
            var rootGo = new GameObject("Geometry");
            rootGo.transform.SetParent(transform, false);
            geometryRoot = rootGo.transform;
        }

        // Clear children
#if UNITY_EDITOR
        // DestroyImmediate is fine in edit-time
        for (var i = geometryRoot.childCount - 1; i >= 0; i--)
        {
            var child = geometryRoot.GetChild(i);
            if (Application.isEditor)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
        }
#else
        for (var i = geometryRoot.childCount - 1; i >= 0; i--)
        {
            var child = geometryRoot.GetChild(i);
            Destroy(child.gameObject);
        }
#endif

        // Optionally randomize color per build (muted palette)
        if (_randomizeColorOnBuild)
        {
            var h = Random.value; // any hue
            var s = 0.05f + Random.value * 0.10f; // very low saturation (muted)
            var v = 0.55f + Random.value * 0.15f; // slightly dimmer
            _color = Color.HSVToRGB(h, s, v);
            if (_isCorridor)
            {
                // Corridors: even more desaturated and slightly darker
                Color.RGBToHSV(_color, out h, out s, out v);
                s *= 0.6f;
                v *= 0.9f;
                _color = Color.HSVToRGB(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
            }
        }

        // Create a shared material for this room (URP Unlit, double-sided)
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", _color); else mat.color = _color;
        // Render both sides to avoid needing duplicate quads
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

        var halfX = _sizeXz.x * 0.5f;
        var halfZ = _sizeXz.y * 0.5f;

        // Floor and ceiling slightly offset to prevent z-fighting with overlaps.
        // Add a tiny, deterministic per-room offset so intersecting rooms don't flicker.
        var baseEps = 0.0008f;
        var hash = ComputeStableHash(_roomName);
        var noise = (hash % 7) * 0.0001f; // 0..0.0006
        var floorY = (_isCorridor ? baseEps : baseEps * 2f) + noise;
        var ceilY = _heightY - baseEps - noise;
        CreateQuad("Floor", geometryRoot, new Vector3(-halfX, floorY, -halfZ), new Vector3(halfX, floorY, -halfZ), new Vector3(halfX, floorY, halfZ), new Vector3(-halfX, floorY, halfZ), Vector3.up, mat);
        if (_buildCeiling)
            CreateQuad("Ceiling", geometryRoot, new Vector3(-halfX, ceilY, -halfZ), new Vector3(halfX, ceilY, -halfZ), new Vector3(halfX, ceilY, halfZ), new Vector3(-halfX, ceilY, halfZ), Vector3.down, mat);

        // Walls (double-sided via material, 1-unit segments)
        BuildPerimeterWalls(geometryRoot, halfX, halfZ, _heightY, mat);
    }

    private void BuildPerimeterWalls(Transform geometryRoot, float halfX, float halfZ, float height, Material mat)
    {
        // Bottom edge (Z = -halfZ), segments along X
        BuildWallLine(geometryRoot, new Vector3(-halfX, 0f, -halfZ), Vector3.right, _sizeXz.x, height, Vector3.back, mat, "Wall_South");
        // Top edge (Z = +halfZ)
        BuildWallLine(geometryRoot, new Vector3(-halfX, 0f, halfZ), Vector3.right, _sizeXz.x, height, Vector3.forward, mat, "Wall_North");
        // Left edge (X = -halfX), segments along Z
        BuildWallLine(geometryRoot, new Vector3(-halfX, 0f, -halfZ), Vector3.forward, _sizeXz.y, height, Vector3.left, mat, "Wall_West");
        // Right edge (X = +halfX)
        BuildWallLine(geometryRoot, new Vector3(halfX, 0f, -halfZ), Vector3.forward, _sizeXz.y, height, Vector3.right, mat, "Wall_East");
    }

    private void BuildWallLine(Transform parent, Vector3 start, Vector3 stepDir, float totalLength, float height, Vector3 outwardNormal, Material mat, string prefix)
    {
        var remaining = totalLength;
        var cursor = start;
        var index = 0;
        while (remaining > 0.0001f)
        {
            var seg = Mathf.Min(1f, remaining);
            var end = cursor + stepDir * seg;

            // Define wall quad corners (bottom line from cursor to end, extrude up by height)
            var bl = cursor;                 // bottom left along the line
            var br = end;                    // bottom right along the line
            var tr = br + Vector3.up * height;
            var tl = bl + Vector3.up * height;

            var segRoot = new GameObject($"{prefix}_{index:00}").transform;
            segRoot.SetParent(parent, false);

            // Single quad; material renders both faces
            CreateQuad("Wall", segRoot, bl, br, tr, tl, outwardNormal, mat);

            remaining -= seg;
            cursor = end;
            index++;
        }
    }

    private void CreateQuad(string name, Transform parent, Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl, Vector3 normal, Material mat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        var mesh = new Mesh();
        var verts = new List<Vector3> { bl, br, tr, tl };
        var norms = new List<Vector3> { normal, normal, normal, normal };
        var uvs = new List<Vector2> { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        var tris = new List<int> { 0, 1, 2, 0, 2, 3 };

        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
    }

    private int ComputeStableHash(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        var h = 23;
        for (var i = 0; i < s.Length; i++)
            h = h * 31 + s[i];
        return h & 0x7fffffff;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw a subtle outline and a label with dimensions
        var t = transform;
        var halfX = _sizeXz.x * 0.5f;
        var halfZ = _sizeXz.y * 0.5f;

        var p0 = t.TransformPoint(new Vector3(-halfX, 0f, -halfZ));
        var p1 = t.TransformPoint(new Vector3(-halfX, 0f, halfZ));
        var p2 = t.TransformPoint(new Vector3(halfX, 0f, halfZ));
        var p3 = t.TransformPoint(new Vector3(halfX, 0f, -halfZ));

        UnityEditor.Handles.color = new Color(_color.r, _color.g, _color.b, 0.9f);
        UnityEditor.Handles.DrawAAPolyLine(3f, p0, p1, p2, p3, p0);

        var label = $"{_roomName}\n{_sizeXz.x:0} x {_sizeXz.y:0} units";
        var labelPos = t.position + Vector3.up * (_heightY + 0.05f);
        var labelStyle = new GUIStyle(UnityEditor.EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.UpperCenter
        };
        labelStyle.normal.textColor = Color.white;
        UnityEditor.Handles.Label(labelPos, label, labelStyle);

        // Draw simple dimension ticks along X and Z near the floor
        var dimColor = new Color(1f, 1f, 1f, 0.3f);
        UnityEditor.Handles.color = dimColor;
        var midLeft = (p0 + p1) * 0.5f;
        var midRight = (p2 + p3) * 0.5f;
        var midBottom = (p0 + p3) * 0.5f;
        var midTop = (p1 + p2) * 0.5f;
        UnityEditor.Handles.DrawAAPolyLine(2f, midLeft, midRight);
        UnityEditor.Handles.DrawAAPolyLine(2f, midBottom, midTop);
        var xLabelPos = (midLeft + midRight) * 0.5f + Vector3.up * 0.02f;
        var zLabelPos = (midBottom + midTop) * 0.5f + Vector3.up * 0.02f;
        UnityEditor.Handles.Label(xLabelPos, $"W: {_sizeXz.x:0}");
        UnityEditor.Handles.Label(zLabelPos, $"D: {_sizeXz.y:0}");
    }
#endif
}

