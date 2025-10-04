using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class MuseumGeneratorWindow : EditorWindow
{
    private const float DefaultUnitScale = 1f; // 1 unit = 1m grid cell
    private float _unitScale = DefaultUnitScale;
    private float _roomHeight = 3f;
    private Vector2 _startPosition = Vector2.zero;

    [MenuItem("Tools/Museum/Generate Museum Greybox")] 
    private static void Open() 
    { 
        GetWindow<MuseumGeneratorWindow>("Museum Generator").Show(); 
    }

    private void OnGUI()
    {
        _unitScale = EditorGUILayout.FloatField("Unit Scale (Unity units per grid unit)", _unitScale);
        _roomHeight = EditorGUILayout.FloatField("Room Height (Y)", _roomHeight);
        _startPosition = EditorGUILayout.Vector2Field("Start Position (X,Z)", _startPosition);

        if (GUILayout.Button("Generate Greybox Layout"))
        {
            Generate();
        }

        if (GUILayout.Button("Generate 8-Room Prototype With Corridors"))
        {
            GenerateSimpleEight();
        }

        if (GUILayout.Button("Generate Concept Plan Layout (with Arc Corridors)"))
        {
            GenerateConceptPlan();
        }
    }

    private void Generate()
    {
        // Create root
        var rootGo = new GameObject("Museum_Greybox_Root");
        var root = rootGo.AddComponent<MuseumLayout>();

        // Helper to spawn a room root with RoomGreybox (builds its own geometry)
        RoomGreybox Spawn(string roomDisplayName, float w, float d, float x, float z)
        {
            var go = new GameObject();
            go.transform.SetParent(rootGo.transform);
            go.transform.position = new Vector3(_startPosition.x + x * _unitScale, 0f, _startPosition.y + z * _unitScale);
            var rb = go.AddComponent<RoomGreybox>();
            go.name = roomDisplayName;
            var sz = new Vector2(w * _unitScale, d * _unitScale);
            var so = new SerializedObject(rb);
            so.FindProperty("_roomName").stringValue = roomDisplayName;
            so.FindProperty("_sizeXz").vector2Value = sz;
            so.FindProperty("_heightY").floatValue = _roomHeight;
            so.FindProperty("_isCorridor").boolValue = roomDisplayName.Contains("Corridor") || roomDisplayName.StartsWith("C");
            so.ApplyModifiedPropertiesWithoutUndo();
            root.Register(rb);
            return rb;
        }

        // Basic manual layout coordinates (simple grid-based placement)
        // Central hub
        var grandEntrance = Spawn("Grand Entrance Hall (15x20)", 15, 20, 0, 0);
        var restrooms = Spawn("Restrooms (8x12)", 8, 12, -12, -14);

        // Natural History cluster
        var natHist = Spawn("Natural History Wing (20x25)", 20, 25, -30, 10);
        var paleo = Spawn("Paleontology (18x18)", 18, 18, -55, 15);
        var ancientLife = Spawn("Ancient Life (14x16)", 14, 16, -75, 15);
        var diorama = Spawn("Wildlife Diorama (15x30)", 15, 30, -30, 45);
        var ocean = Spawn("Ocean Life (16x16)", 16, 16, -10, 55);
        var evolution = Spawn("Evolution (12x18)", 12, 18, -10, 15);
        var humanOrigins = Spawn("Human Origins (14x14)", 14, 14, 10, 15);
        var geology = Spawn("Geology & Minerals (14x16)", 14, 16, -30, -15);

        // Art cluster
        var artWing = Spawn("Art Gallery Wing (18x22)", 18, 22, 30, 10);
        var modern = Spawn("Modern Art (16x18)", 16, 18, 55, 10);
        var abstractArt = Spawn("Abstract Art (14x14)", 14, 14, 55, 28);
        var classical = Spawn("Classical Art (16x18)", 16, 18, 30, -15);
        var portrait = Spawn("Portrait Gallery (12x20)", 12, 20, 30, -40);
        var impression = Spawn("Impressionist (14x16)", 14, 16, 55, -15);
        var sculpture = Spawn("Sculpture Garden (20x20)", 20, 20, 80, 15);

        // Cultural History cluster
        var cultural = Spawn("Cultural History Wing (18x24)", 18, 24, 0, 30);
        var egypt = Spawn("Ancient Egypt (18x20)", 18, 20, 0, 55);
        var civs = Spawn("Ancient Civilizations (16x18)", 16, 18, 0, 80);
        var medieval = Spawn("Medieval Hall (16x20)", 16, 20, -20, 80);
        var renaissance = Spawn("Renaissance (14x16)", 14, 16, -20, 100);
        var asian = Spawn("Asian Art & Culture (16x18)", 16, 18, 20, 80);
        var indigenous = Spawn("Indigenous Cultures (14x16)", 14, 16, 40, 80);

        // Special / Amenities
        var special = Spawn("Special Exhibitions (22x28)", 22, 28, -60, -20);
        var sciTech = Spawn("Science & Technology (18x20)", 18, 20, 10, -15);
        var innovation = Spawn("Innovation Lab (14x16)", 14, 16, 30, -60);
        var gift = Spawn("Gift Shop (14x16)", 14, 16, 15, -40);
        var cafe = Spawn("Café / Rest (18x20)", 18, 20, -15, -40);
        var auditorium = Spawn("Auditorium (20x24)", 20, 24, -35, -60);

        // Connections helper
        void Conn(RoomGreybox a, RoomGreybox b, float width) => root.Connect(a, b, width);

        // Connections from spec (internal only; exterior notes omitted)
        Conn(grandEntrance, natHist, 3);
        Conn(grandEntrance, artWing, 3);
        Conn(grandEntrance, cultural, 3);
        Conn(grandEntrance, special, 3);
        Conn(grandEntrance, gift, 2);
        Conn(grandEntrance, cafe, 2);
        Conn(grandEntrance, restrooms, 2);

        Conn(natHist, paleo, 3);
        Conn(natHist, diorama, 2);
        Conn(natHist, evolution, 2);
        Conn(natHist, geology, 2);
        Conn(paleo, ancientLife, 2);
        Conn(diorama, ocean, 2);
        Conn(ocean, natHist, 2);
        Conn(evolution, ancientLife, 2);
        Conn(evolution, humanOrigins, 2);
        Conn(humanOrigins, cultural, 2);

        Conn(artWing, modern, 2);
        Conn(artWing, classical, 2);
        Conn(artWing, impression, 2);
        Conn(artWing, sculpture, 3);
        Conn(modern, abstractArt, 2);
        Conn(classical, portrait, 2);
        Conn(portrait, impression, 2);
        Conn(impression, modern, 2);
        Conn(abstractArt, sculpture, 2);

        Conn(cultural, humanOrigins, 2);
        Conn(cultural, egypt, 3);
        Conn(cultural, civs, 2);
        Conn(cultural, medieval, 2);
        Conn(cultural, asian, 2);
        Conn(egypt, civs, 2);
        Conn(civs, medieval, 2);
        Conn(medieval, renaissance, 2);
        Conn(renaissance, classical, 2);
        Conn(asian, indigenous, 2);
        Conn(indigenous, cultural, 2);

        Conn(special, grandEntrance, 4);

        Conn(sciTech, grandEntrance, 2);
        Conn(sciTech, geology, 2);
        Conn(sciTech, innovation, 2);

        Conn(gift, grandEntrance, 2);
        Conn(gift, sculpture, 2);
        Conn(cafe, grandEntrance, 2);
        Conn(cafe, sculpture, 2);

        Conn(auditorium, grandEntrance, 3);

        Selection.activeGameObject = rootGo;
        EditorGUIUtility.PingObject(rootGo);
    }

    private void GenerateSimpleEight()
    {
        // Root
        var rootGo = new GameObject("Museum_8Room_Root");
        var root = rootGo.AddComponent<MuseumLayout>();

        // Helper
        RoomGreybox Spawn(string roomDisplayName, float w, float d, float x, float z)
        {
            var go = new GameObject();
            go.transform.SetParent(rootGo.transform);
            go.transform.position = new Vector3(_startPosition.x + x * _unitScale, 0f, _startPosition.y + z * _unitScale);
            var rb = go.AddComponent<RoomGreybox>();
            go.name = roomDisplayName;
            var sz = new Vector2(w * _unitScale, d * _unitScale);
            var so = new SerializedObject(rb);
            so.FindProperty("_roomName").stringValue = roomDisplayName;
            so.FindProperty("_sizeXz").vector2Value = sz;
            so.FindProperty("_heightY").floatValue = _roomHeight;
            so.ApplyModifiedPropertiesWithoutUndo();
            root.Register(rb);
            return rb;
        }

        // Layout parameters
        var roomW = 12f;
        var roomD = 10f;
        var corridorWidth = 2f;
        var rowZ = 12f; // top row at +rowZ, bottom row at -rowZ

        // X positions for 4 rooms per row
        var xs = new float[] { -45f, -15f, 15f, 45f };

        // Rooms
        var r1 = Spawn("R1", roomW, roomD, xs[0], rowZ);
        var r2 = Spawn("R2", roomW, roomD, xs[1], rowZ);
        var r3 = Spawn("R3", roomW, roomD, xs[2], rowZ);
        var r4 = Spawn("R4", roomW, roomD, xs[3], rowZ);
        var r5 = Spawn("R5", roomW, roomD, xs[0], -rowZ);
        var r6 = Spawn("R6", roomW, roomD, xs[1], -rowZ);
        var r7 = Spawn("R7", roomW, roomD, xs[2], -rowZ);
        var r8 = Spawn("R8", roomW, roomD, xs[3], -rowZ);

        // Main horizontal corridor spanning across
        var halfRoomW = roomW * 0.5f;
        var rightmost = xs[3] + halfRoomW + 6f; // small margin
        var leftmost = xs[0] - halfRoomW - 6f;
        var mainLength = rightmost - leftmost; // in units
        var mainCenterX = (rightmost + leftmost) * 0.5f;
        var main = Spawn("Main Corridor", mainLength, corridorWidth, mainCenterX, 0f);

        // Vertical connectors from each room to main corridor
        var halfRoomD = roomD * 0.5f;
        var halfCorridor = corridorWidth * 0.5f;
        float ConnectorLength(float roomCenterZ, bool isTopRow)
        {
            // distance from room bottom/top edge to corridor edge
            return isTopRow
                ? (roomCenterZ - (halfRoomD + halfCorridor))
                : (-roomCenterZ - (halfRoomD + halfCorridor));
        }

        RoomGreybox SpawnConnector(string name, float x, bool top)
        {
            // simpler: place between edges precisely
            var roomEdgeZ = top ? (rowZ - halfRoomD) : -(rowZ - halfRoomD);
            var corridorEdgeZ = top ? halfCorridor : -halfCorridor;
            var cz = (roomEdgeZ + corridorEdgeZ) * 0.5f;
            var l = Mathf.Abs(roomEdgeZ - corridorEdgeZ);
            return Spawn(name, corridorWidth, l, x, cz);
        }

        var c1 = SpawnConnector("C1", xs[0], true);
        var c2 = SpawnConnector("C2", xs[1], true);
        var c3 = SpawnConnector("C3", xs[2], true);
        var c4 = SpawnConnector("C4", xs[3], true);
        var c5 = SpawnConnector("C5", xs[0], false);
        var c6 = SpawnConnector("C6", xs[1], false);
        var c7 = SpawnConnector("C7", xs[2], false);
        var c8 = SpawnConnector("C8", xs[3], false);

        // Connect graph
        void Conn(RoomGreybox a, RoomGreybox b, float width) => root.Connect(a, b, width);

        Conn(r1, c1, corridorWidth);
        Conn(r2, c2, corridorWidth);
        Conn(r3, c3, corridorWidth);
        Conn(r4, c4, corridorWidth);
        Conn(r5, c5, corridorWidth);
        Conn(r6, c6, corridorWidth);
        Conn(r7, c7, corridorWidth);
        Conn(r8, c8, corridorWidth);

        Conn(c1, main, corridorWidth);
        Conn(c2, main, corridorWidth);
        Conn(c3, main, corridorWidth);
        Conn(c4, main, corridorWidth);
        Conn(c5, main, corridorWidth);
        Conn(c6, main, corridorWidth);
        Conn(c7, main, corridorWidth);
        Conn(c8, main, corridorWidth);

        Selection.activeGameObject = rootGo;
        EditorGUIUtility.PingObject(rootGo);
    }

    private void GenerateConceptPlan()
    {
        var rootGo = new GameObject("Museum_ConceptPlan_Root");
        var root = rootGo.AddComponent<MuseumLayout>();

        RoomGreybox Spawn(string roomDisplayName, float w, float d, float x, float z)
        {
            var go = new GameObject();
            go.transform.SetParent(rootGo.transform);
            go.transform.position = new Vector3(_startPosition.x + x * _unitScale, 0f, _startPosition.y + z * _unitScale);
            var rb = go.AddComponent<RoomGreybox>();
            go.name = roomDisplayName;
            var sz = new Vector2(w * _unitScale, d * _unitScale);
            var so = new SerializedObject(rb);
            so.FindProperty("_roomName").stringValue = roomDisplayName;
            so.FindProperty("_sizeXz").vector2Value = sz;
            so.FindProperty("_heightY").floatValue = _roomHeight;
            so.FindProperty("_isCorridor").boolValue = roomDisplayName.Contains("Corridor") || roomDisplayName.StartsWith("C");
            so.ApplyModifiedPropertiesWithoutUndo();
            root.Register(rb);
            return rb;
        }

        RoomGreybox SpawnRot(string roomDisplayName, float w, float d, float x, float z, float rotDeg)
        {
            var r = Spawn(roomDisplayName, w, d, x, z);
            r.transform.rotation = Quaternion.Euler(0f, rotDeg, 0f);
            return r;
        }

        // Core masses
        var greatHall = Spawn("Great Hall", 30, 24, 0, 0);
        var northEntry = Spawn("North Entry", 22, 12, 0, 24);
        var southFoyer = Spawn("South Foyer", 20, 12, 0, -24);
        var westWing = Spawn("West Wing", 24, 18, -30, 0);
        var eastGalleries = Spawn("East Galleries", 24, 18, 30, 0);
        var rotundaCore = Spawn("Rotunda Core", 14, 14, 44, 0);

        // Connect cores
        void Conn(RoomGreybox a, RoomGreybox b, float width) => root.Connect(a, b, width);
        Conn(greatHall, northEntry, 3);
        Conn(greatHall, southFoyer, 3);
        Conn(greatHall, westWing, 3);
        Conn(greatHall, eastGalleries, 3);
        Conn(eastGalleries, rotundaCore, 3);

        // Short corridors between cores
        var cNorth = Spawn("Corridor North", 6, 2, 0, 12);
        var cSouth = Spawn("Corridor South", 6, 2, 0, -12);
        var cWest = Spawn("Corridor West", 6, 2, -15, 0);
        var cEast = Spawn("Corridor East", 6, 2, 15, 0);
        Conn(greatHall, cNorth, 2);
        Conn(cNorth, northEntry, 2);
        Conn(greatHall, cSouth, 2);
        Conn(cSouth, southFoyer, 2);
        Conn(greatHall, cWest, 2);
        Conn(cWest, westWing, 2);
        Conn(greatHall, cEast, 2);
        Conn(cEast, eastGalleries, 2);

        // Arc of corridor segments approximating the circular gallery to the east
        var arcRadius = 60f;    // center at greatHall.x ~= 0, offset east
        var arcCenter = new Vector2(30f, 0f); // around east galleries
        var arcHalfWidth = 2f;  // corridor width
        var arcSegLen = 12f;    // segment length
        var arcAngles = new float[] { -60f, -40f, -20f, 0f, 20f, 40f, 60f };
        var arcRooms = new List<RoomGreybox>();
        foreach (var ang in arcAngles)
        {
            var rad = ang * Mathf.Deg2Rad;
            var cx = arcCenter.x + Mathf.Cos(rad) * arcRadius;
            var cz = arcCenter.y + Mathf.Sin(rad) * arcRadius;
            var tangentDeg = ang + 90f;
            var seg = SpawnRot($"Arc Corridor {ang:0}", arcSegLen, arcHalfWidth, cx, cz, tangentDeg);
            arcRooms.Add(seg);
        }

        // Connect arc chain and hook to rotunda/east galleries
        for (var i = 0; i < arcRooms.Count - 1; i++)
            Conn(arcRooms[i], arcRooms[i + 1], arcHalfWidth);
        Conn(arcRooms[arcRooms.Count / 2], rotundaCore, arcHalfWidth);
        Conn(arcRooms[0], eastGalleries, arcHalfWidth);
        Conn(arcRooms[arcRooms.Count - 1], eastGalleries, arcHalfWidth);

        // Additional satellite rooms around west and south to echo plan density
        var westStudios = Spawn("Studios", 16, 14, -48, -6);
        var westArchive = Spawn("Archive", 14, 12, -48, 12);
        Conn(westWing, westStudios, 2);
        Conn(westWing, westArchive, 2);

        var education = Spawn("Education Center", 18, 14, 0, -40);
        Conn(southFoyer, education, 2);

        Selection.activeGameObject = rootGo;
        EditorGUIUtility.PingObject(rootGo);
    }
}
#endif


