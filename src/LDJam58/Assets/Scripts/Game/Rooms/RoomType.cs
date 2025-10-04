using UnityEngine;

[CreateAssetMenu(fileName = "RoomType", menuName = "RoomType")]
public class RoomType : ScriptableObject
{
    public string Name;
    public ExhibitTag[] Requirement;
    public ExhibitTagMultiplier[] Multipliers;
}
