using UnityEngine;
using UnityEngine.AI;

public class AlienAI : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState = State.Patrol;

    public NavMeshAgent agent;
    public Transform player;
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    public float visionRange = 10f;

    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolBehavior();
                break;
            case State.Chase:
                ChaseBehavior();
                break;
        }

        CheckVision();
    }

    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPointIndex].position);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void ChaseBehavior()
    {
        agent.SetDestination(player.position);
    }

    void CheckVision()
    {
        // ถ้าถูก alert แล้ว ไม่ต้องใช้ vision — ไล่ตลอดเวลา
        if (isAlerted) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < visionRange)
        {
            Vector3 eyeLevel = transform.position + Vector3.up * 1.5f;
            Vector3 directionToPlayer = (player.position - eyeLevel).normalized;

            RaycastHit hit;
            if (Physics.Raycast(eyeLevel, directionToPlayer, out hit, visionRange))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    currentState = State.Chase;
                    return;
                }
            }
        }

        if (currentState == State.Chase) currentState = State.Patrol;
    }

    // เรียกจาก HackingTerminal เมื่อ hack สำเร็จ — ผีไล่ผู้เล่นทันทีโดยไม่ต้องเห็น
    public void ForceChase()
    {
        isAlerted = true;
        currentState = State.Chase;
    }

    public bool isAlerted = false;
}
