using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Game.NPC
{
    public class NpcSmoothReturn : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Rigidbody rb; // For kinematic movement if needed
        public float smoothReturnSpeed = 2f; // Speed to move back smoothly
        public float maxReturnDistance = 10f; // Max distance to search for nearest NavMesh point
        private bool isReturning;
        private Vector3 targetReturnPos = Vector3.zero;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>(); // Assume added, set to Kinematic in Inspector
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }

            agent.updatePosition = false; // Decouple auto-positioning
            //agent.updateRotation = false; // Optional: Handle rotation yourself if needed
        }

        private void LateUpdate() // Check after all movements/physics in Update/FixedUpdate
        {
            // Instant off-mesh detection
            if (!isReturning && !agent.isOnNavMesh) StartCoroutine(ReturnToNavMeshSmoothly());

            // If not returning, follow agent's desired movement
            if (!isReturning && agent.hasPath)
                transform.position =
                    Vector3.MoveTowards(transform.position, agent.nextPosition, agent.speed * Time.deltaTime);
        }

        private IEnumerator ReturnToNavMeshSmoothly()
        {
            isReturning = true;

            // Find nearest valid NavMesh point
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, maxReturnDistance, NavMesh.AllAreas))
            {
                targetReturnPos = hit.position;

                while (Vector3.Distance(transform.position, targetReturnPos) > 0.1f)
                {
                    // Smoothly move transform (use Lerp for easing, or MoveTowards for constant)
                    transform.position = Vector3.MoveTowards(transform.position, targetReturnPos,
                        smoothReturnSpeed * Time.deltaTime);

                    // Sync to agent during return (helps path resume smoothly)
                    agent.nextPosition = transform.position;

                    yield return null;
                }

                // Final sync once back
                agent.nextPosition = transform.position;
            }
            else
            {
                Debug.LogWarning("No nearby NavMesh found for " + gameObject.name);
                // Fallback: Maybe idle or destroy
            }

            // Resume normal pathing
            if (agent.hasPath) agent.SetDestination(agent.destination); // Refresh path if needed

            isReturning = false;
        }

        // Example: To set a destination (your normal NPC logic)
        public void SetNPCDestination(Vector3 dest)
        {
            agent.SetDestination(dest);
        }
    }
}