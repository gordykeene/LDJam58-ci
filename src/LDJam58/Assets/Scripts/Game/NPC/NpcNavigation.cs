using System;
using System.Collections.Generic;
using Game.Messages;
using Unity.AI.Navigation;
using UnityEngine;

namespace Game.NPC
{
    public class NpcNavigation : OnMessage<ExhibitPlaced>
    {
        [SerializeField]
        private NavMeshSurface navMeshSurface;

        private List<GameObject> exhibits;

        private void Start()
        {
            exhibits = new List<GameObject>();
        }

        public void Rebake()
        {
            navMeshSurface.BuildNavMesh();
        }


        protected override void Execute(ExhibitPlaced msg)
        {
            exhibits.Add(msg.exhibitInstance);
        }

        public Vector3 GetRandomExhibitPosition()
        {
            if(exhibits.Count == 0) return Vector3.zero;
            return exhibits.Random().transform.position;
        }
    }
}