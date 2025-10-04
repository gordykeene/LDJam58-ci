using System;
using System.Collections;
using Assets.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Game.NPC
{
    public class NpcAgentController : MonoBehaviour
    {
        private enum State
        {
            Standing = 0,
            Walking = 1,
            Looking = 2,
            Reacting = 3
        }
        
        private enum Mood
        {
            Excited = 4,
            Happy = 3,
            Interested = 2,
            Neutral = 1,
            Angry = 0
        }
        
        
        [SerializeField]
        private NavMeshAgent  navMeshAgent;
        [SerializeField]
        private NpcNavigation npcNavigation;
        [SerializeField]
        private NpcModelPool npcModelPool;

        [SerializeField] private float lookTime;
        [SerializeField] private float standTime;
        [SerializeField] private float reactTime;
        [SerializeField] private float lookDistance;


        private ExhibitTileType currentExhibitTile;
        private NpcObject npcObject;
        private Animator animator;
        private Mood mood;
        private State state;
        private Vector3 targetPosition;
        
        private void Start()
        {
            npcObject = npcModelPool.GetNpcModel();
            
            var inst = Instantiate(npcObject.prefab, transform);
            animator = inst.GetComponent<Animator>();
        }

        private void GetMood()
        {
            var score = npcObject.GetExhibitScore(currentExhibitTile);
            if (score > 10)
            {
                mood = Mood.Excited;
            }
            else if (score > 6)
            {
                mood = Mood.Happy;
            }
            else if (score > 3)
            {
                mood = Mood.Interested;
            }
            else if (score > 0)
            {
                mood = Mood.Neutral;
            }
            else if (score < 0)
            {
                mood = Mood.Angry;
            }
            print($"Evaluated:{currentExhibitTile.DisplayName} with score:{score}, mood:{mood}");

            animator.SetInteger("mood", (int)mood);
        }
        
        private void MoveToPosition(Vector3 position)
        {
            navMeshAgent.SetDestination(position);
            animator.SetBool("isMoving", true);
        }

        [Button]
        private void PickExhibit()
        {
            targetPosition = npcNavigation.GetRandomExhibitPosition(out currentExhibitTile);
            MoveToPosition(targetPosition);
        }

        [Button]
        private void StartPickCoroutine()
        {
            StartCoroutine(NpcStateMachine());
        }

        private IEnumerator NpcStateMachine()
        {
            while (true)
            {
                if (state == State.Standing)
                {
                    animator.SetInteger("state", 0);
                    yield return new WaitForSeconds(standTime);
                    state = State.Walking;
                    PickExhibit();
                }

                if (state == State.Walking)
                {
                    animator.SetInteger("state", 1);
                    while (Vector3.SqrMagnitude(transform.position - targetPosition) > lookDistance * lookDistance)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    state = State.Looking;
                }

                if (state == State.Looking)
                {
                    animator.SetInteger("state", 2);
                    yield return new WaitForSeconds(lookTime);
                    state = State.Reacting;
                }

                if (state == State.Reacting)
                {
                    GetMood();
                    animator.SetInteger("state", 3);
                    yield return new WaitForSeconds(reactTime);
                    state = State.Standing;
                }
            }
        }
    }
}