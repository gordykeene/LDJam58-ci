using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rooms", menuName = "Rooms")]public class Rooms : ScriptableObject
{
    public List<RoomType> All = new List<RoomType>();
}
