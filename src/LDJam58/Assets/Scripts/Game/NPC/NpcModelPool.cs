using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.NPC
{
    [CreateAssetMenu(fileName = "NPCModelPool", menuName = "NPC/Create NPC Model Pool")]
    public class NpcModelPool : ScriptableObject
    {
        [AssetsOnly]
        [SerializeField]
        private List<NpcObject> npcObjects;

        public NpcObject GetNpcModel()
        {
            return npcObjects.Random();
        }
    }
}