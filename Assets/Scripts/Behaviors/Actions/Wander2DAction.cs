using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Wander2D",
    story: "Moves [Agent] to random positions in a [Radius] radius.",
    category: "Action/Navigation",
    id: "72e5dafac2db231589203c1ea6198b5c")]
public partial class Wander2DAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Origin;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> WaitTime = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
    [SerializeReference] public BlackboardVariable<string> AnimatorMoveStateParam = new BlackboardVariable<string>("Motion");
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam = new BlackboardVariable<string>("Speed");
    [SerializeReference] public BlackboardVariable<string> AnimatorDirectionXParam = new BlackboardVariable<string>("DX");
    [SerializeReference] public BlackboardVariable<string> AnimatorDirectionYParam = new BlackboardVariable<string>("DY");

    NavMeshAgent _navMeshAgent;
    Animator _animator;
    Vector2 _direction;

    [CreateProperty]
    Vector3 _currentTarget;
    [CreateProperty]
    bool _waiting;
    [CreateProperty]
    float _waitTimer;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        Initialize();
        _waiting = false;
        _waitTimer = 0.0f;
        MoveToRandomPosition();
        return Status.Running;
    }

    bool IsNavMeshValid()
    {
        return _navMeshAgent != null && _navMeshAgent != null &&
            _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null) return Status.Failure;

        if (_waiting)
        {
            if (_waitTimer > 0.0f)
                _waitTimer -= Time.deltaTime;
            else
            {
                _waitTimer = 0f;
                _waiting = false;
                MoveToRandomPosition();
            }
        }
        else
        {
            if (IsNavMeshValid() && _navMeshAgent.remainingDistance <= DistanceThreshold)
            {
                ResetAnimSpeed();
                _waitTimer = WaitTime.Value;
                _waiting = true;
            }
        }

        if (_animator != null && _navMeshAgent != null)
        {
            if (_navMeshAgent.velocity.magnitude > 0.0f)
                _animator.SetInteger(AnimatorMoveStateParam, 0);
            else
                _animator.SetInteger(AnimatorMoveStateParam, -1);

            _animator.SetFloat(AnimatorDirectionXParam, _direction.x);
            _animator.SetFloat(AnimatorDirectionYParam, _direction.y);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        ResetAnimDirection();
        ResetAnimSpeed();
        if (IsNavMeshValid()) _navMeshAgent.ResetPath();
    }

    protected override void OnDeserialize()
    {
        Initialize();
    }

    void ResetAnimDirection()
    {
        if (_animator != null)
        {
            _direction = Vector2.zero;
            _animator.SetFloat(AnimatorDirectionXParam, 0f);
            _animator.SetFloat(AnimatorDirectionYParam, 0f);
        }
    }

    void ResetAnimSpeed()
    {
        if (_animator != null)
        {
            _animator.SetFloat(AnimatorSpeedParam, 0f);
            _animator.SetInteger(AnimatorMoveStateParam, -1);
        }
    }

    void Initialize()
    {
        Origin.Value = Origin.Value == Vector3.zero ? Agent.Value.transform.position : Origin.Value;

        _animator = Agent.Value.GetComponentInChildren<Animator>();
        ResetAnimDirection();
        ResetAnimSpeed();

        _navMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.updateUpAxis = false;
            _navMeshAgent.autoRepath = true;

            if (_navMeshAgent.isOnNavMesh) _navMeshAgent.ResetPath();
            _navMeshAgent.speed = Speed.Value;
        }
    }

    void MoveToRandomPosition()
    {
        if (IsNavMeshValid())
        {
            Vector3 lastPosition = Agent.Value.transform.position;

            _currentTarget = Origin.Value + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * Radius.Value;
            _navMeshAgent.SetDestination(_currentTarget);

            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
                _navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                _navMeshAgent.ResetPath();
                _navMeshAgent.velocity = Vector3.zero;
                _currentTarget = Origin.Value + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * Radius.Value;
                _navMeshAgent.SetDestination(_currentTarget);
            }

            if (_animator != null)
            {
                _direction = (_currentTarget - lastPosition).normalized;
                _animator.SetFloat(AnimatorSpeedParam, Speed.Value);
                _animator.SetFloat(AnimatorDirectionXParam, _direction.x);
                _animator.SetFloat(AnimatorDirectionYParam, _direction.y);
            }
        }
    }
}
