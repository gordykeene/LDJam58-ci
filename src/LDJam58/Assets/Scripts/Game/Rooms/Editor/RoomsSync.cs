using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class RoomsSync
{
    [MenuItem("Tools/Rooms/Sync All Rooms")]
    public static void SyncAllRoomsAssets()
    {
        var roomTypeGuids = AssetDatabase.FindAssets("t:RoomType");
        var allRoomTypes = new List<RoomType>(roomTypeGuids.Length);
        foreach (var guid in roomTypeGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var roomType = AssetDatabase.LoadAssetAtPath<RoomType>(path);
            if (roomType != null)
            {
                allRoomTypes.Add(roomType);
            }
        }

        var roomsGuids = AssetDatabase.FindAssets("t:Rooms");
        var anyChanged = false;
        foreach (var guid in roomsGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var rooms = AssetDatabase.LoadAssetAtPath<Rooms>(path);
            if (rooms == null)
            {
                continue;
            }

            if (rooms.All == null)
            {
                rooms.All = new List<RoomType>();
            }

            // Ensure exactly one of each RoomType, in a stable order
            var newList = new List<RoomType>(allRoomTypes.Count);
            var seen = new HashSet<RoomType>();
            foreach (var rt in allRoomTypes)
            {
                if (rt == null)
                {
                    continue;
                }
                if (seen.Add(rt))
                {
                    newList.Add(rt);
                }
            }

            var changed = rooms.All.Count != newList.Count || !rooms.All.SequenceEqual(newList);
            if (changed)
            {
                rooms.All = newList;
                EditorUtility.SetDirty(rooms);
                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            AssetDatabase.SaveAssets();
        }
    }
}

public class RoomsAssetPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var shouldSync = false;

        foreach (var path in importedAssets)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj is RoomType)
            {
                shouldSync = true;
                break;
            }
        }

        if (!shouldSync)
        {
            foreach (var path in movedAssets)
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj is RoomType)
                {
                    shouldSync = true;
                    break;
                }
            }
        }

        if (!shouldSync)
        {
            foreach (var path in movedFromAssetPaths)
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj is RoomType)
                {
                    shouldSync = true;
                    break;
                }
            }
        }

        if (shouldSync)
        {
            RoomsSync.SyncAllRoomsAssets();
        }
    }
}
#endif


