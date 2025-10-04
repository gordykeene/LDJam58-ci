using System;
using System.Collections.Generic;
using Assets.Scripts;
using Game.Messages;
using Unity.AI.Navigation;
using UnityEngine;

namespace Game.NPC
{
    public class NpcNavigation : OnMessage<ExhibitPlaced>
    {
        private class ExhibitEntry
        {
            public GameObject exhibitInstance;
            public ExhibitTileType exhibitTileType;

            public ExhibitEntry(GameObject exhibitInstance, ExhibitTileType exhibitTileType)
            {
                this.exhibitInstance = exhibitInstance;
                this.exhibitTileType = exhibitTileType;
            }
        }
        
        [SerializeField]
        private NavMeshSurface navMeshSurface;

        private List<ExhibitEntry> exhibits;

        private void Start()
        {
            exhibits = new List<ExhibitEntry>();
        }

        public void Rebake()
        {
            navMeshSurface.BuildNavMesh();
        }


        protected override void Execute(ExhibitPlaced msg)
        {
            
            exhibits.Add(new  ExhibitEntry(msg.exhibitInstance, msg.exhibitTileType));
        }

        public Vector3 GetRandomExhibitPosition(out ExhibitTileType exhibitTileType)
        {
            if (exhibits.Count == 0)
            {
                exhibitTileType = null;
                return Vector3.zero;
            }
            var rand = exhibits.Random();
            
            exhibitTileType = rand.exhibitTileType;
            return rand.exhibitInstance.transform.position;
        }
    }
}