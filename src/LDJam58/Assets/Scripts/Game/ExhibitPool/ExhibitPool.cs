using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

namespace Game.ExhibitPool
{
    [CreateAssetMenu(fileName = "ExhibitPool", menuName = "Exhibits/ExhibitPool")]
    public class ExhibitPoolObject : ScriptableObject
    {
        [SerializeField]
        private List<ExhibitTileType> exhibits;
        
        public List<ExhibitTileType> Exhibits => exhibits;
    }
}