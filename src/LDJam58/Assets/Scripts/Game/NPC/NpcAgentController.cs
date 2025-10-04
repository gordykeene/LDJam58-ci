using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Game.NPC
{
    public class NpcAgentController : MonoBehaviour
    {
        [SerializeField]
        private NavMeshAgent  navMeshAgent;
        [SerializeField]
        private NpcNavigation npcNavigation;
        
        private void MoveToPosition(Vector3 position)
        {
            navMeshAgent.SetDestination(position);
        }

        [Button]
        private void PickExhibit()
        {
            var pos = npcNavigation.GetRandomExhibitPosition();
            MoveToPosition(pos);
        }

        [Button]
        private void StartPickCoroutine()
        {
            InvokeRepeating(nameof(PickExhibit), 0f, 3f);
        }
    }
}