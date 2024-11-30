using UnityEngine;
using UnityEngine.AI;
using FishNet.Utility.Template;

public class NPC : TickNetworkBehaviour
{
    const float AnimUpdateFreq = 0.8f;

    [SerializeField] float radiusFromSpawn = 7f;
    [SerializeField] bool isSleeping = false;
    [SerializeField] float energyLifeInSeconds = 480f;
    [SerializeField] float maxEnergyLife = 480f;

    bool isRunning = false;
    Animator anim;
    Vector2 targetPosition;
    Vector3 lastpos = Vector3.zero;
    float bounceOverlap = 0.2f;
    float animUpdateFrequency = 0f;
    Vector2 originalPosition;
    NavMeshAgent agent;

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        targetPosition = transform.position;
        originalPosition = transform.position;

        SetupAgent();

        Invoke("SetRunning", 5);
    }

    void SetupAgent()
    {
        if (agent == null || agent.enabled == false)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.autoRepath = true;
        }
    }

    public void SetRunning()
    {
        isRunning = true;
    }

    void StopAgentAndClearPath()
    {
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    void HandleMovement(float axisx, float axisy)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(targetPosition);
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                StopAgentAndClearPath();
                targetPosition = transform.position + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * 5f;
                agent.SetDestination(targetPosition);
            }
        }

        if (lastpos != transform.position)
        {
            lastpos = transform.position;

            if (Time.fixedTime - animUpdateFrequency > AnimUpdateFreq)
            {
                animUpdateFrequency = Time.fixedTime;
                anim.SetFloat("DX", axisx);
                anim.SetFloat("DY", axisy);
            }

            if (axisx != 0f && axisy != 0f) anim.SetBool("Walk", true);
            else anim.SetBool("Walk", false);
        }
    }

    void HandleEnergy()
    {
        if (isSleeping)
        {
            energyLifeInSeconds += Time.deltaTime;

            if (energyLifeInSeconds >= maxEnergyLife)
            {
                energyLifeInSeconds = maxEnergyLife;
                isSleeping = false;
                anim.SetBool("Sleep", false);
            }
        }
        else
        {
            energyLifeInSeconds -= Time.deltaTime;

            if (energyLifeInSeconds <= 0f)
            {
                energyLifeInSeconds = 0f;
                isSleeping = true;
                anim.SetBool("Walk", false);
                anim.SetBool("Sleep", true);
                anim.SetFloat("DX", 0f);
                anim.SetFloat("DY", 0f);
                StopAgentAndClearPath();
            }
        }
    }

    protected override void TimeManager_OnTick()
    {
        if (!isRunning) return;

        SetupAgent();

        HandleEnergy();

        if (isSleeping) return;

        if (Vector2.Distance(transform.position, originalPosition) >= radiusFromSpawn)
        {
            targetPosition = originalPosition;
        }
        else
        {
            if (Vector2.Distance(transform.position, targetPosition) < 1f)
                targetPosition = transform.position + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * 5f;
        }

        bool canReachTarget = NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas);

        if (canReachTarget)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.SetDestination(targetPosition);
        }
        else
        {
            targetPosition = GetRandomPointOnNavMesh();

            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.SetDestination(targetPosition);

        }

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        HandleMovement(direction.x, direction.y);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isRunning) return;
        if (other.CompareTag("Tilemap"))
        {
            Vector2 v = (Vector2)transform.position - (Vector2)other.transform.position;
            Vector2 direction = v.normalized;
            float overlap = Vector2.Dot(v, direction);
            if (overlap > bounceOverlap)
                targetPosition = transform.position + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * 5f;
        }
    }

    Vector2 GetClosestPointOnNavMesh(Vector3 targetPosition, float maxDistance)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            // Handle the case where no point is found on the NavMesh
            return targetPosition; // Or return a default position
        }
    }

    Vector2 GetRandomPointOnNavMesh()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);

        if (path.corners.Length == 0)
        {
            // Handle the case where no path is found
            return transform.position; // Or return a default position
        }

        Bounds navMeshBounds = new Bounds(path.corners[0], Vector3.zero);
        foreach (Vector3 corner in path.corners)
        {
            navMeshBounds.Encapsulate(corner);
        }

        Vector3 randomPoint = new Vector3(Random.Range(navMeshBounds.min.x, navMeshBounds.max.x),
                                          Random.Range(navMeshBounds.min.y, navMeshBounds.max.y), 0);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            // Retry if the sample failed
            return GetRandomPointOnNavMesh();
        }
    }
}
