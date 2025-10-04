using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds a set of rooms and their connections; draws scene gizmos for doorways.
/// </summary>
public class MuseumLayout : MonoBehaviour
{
    [Serializable]
    public class Connection
    {
        public RoomGreybox A;
        public RoomGreybox B;
        public float Width = 2f;
    }

    [SerializeField] private List<RoomGreybox> _rooms = new List<RoomGreybox>();
    [SerializeField] private List<Connection> _connections = new List<Connection>();

    public IReadOnlyList<RoomGreybox> Rooms => _rooms;
    public IReadOnlyList<Connection> Connections => _connections;

    public void Register(RoomGreybox room)
    {
        if (room == null) return;
        if (!_rooms.Contains(room)) _rooms.Add(room);
    }

    public void Connect(RoomGreybox a, RoomGreybox b, float width)
    {
        if (a == null || b == null || a == b) return;
        var c = new Connection { A = a, B = b, Width = Mathf.Max(0.5f, width) };
        _connections.Add(c);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_connections == null) return;

        foreach (var c in _connections)
        {
            if (c == null || c.A == null || c.B == null) continue;
            var a = c.A.transform.position;
            var b = c.B.transform.position;
            var mid = (a + b) * 0.5f;

            UnityEditor.Handles.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            UnityEditor.Handles.DrawAAPolyLine(3f, a, b);
            var label = $"{c.Width:0}u";
            var style = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.cyan;
            UnityEditor.Handles.Label(mid + Vector3.up * 0.1f, label, style);
        }
    }
#endif
}


