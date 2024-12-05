using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "MoveTo2D",
    story: "[Agent] moves to [Target] target or [Position] position. Should [NotAnimate] not animate?",
    category: "Action/Navigation",
    id: "ed225365bd0aac3358564323707bda12")]
public partial class MoveTo2DAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<float> WaitTime = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
    [SerializeReference] public BlackboardVariable<string> AnimatorMoveStateParam = new BlackboardVariable<string>("Motion");
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam = new BlackboardVariable<string>("Speed");
    [SerializeReference] public BlackboardVariable<string> AnimatorDirectionXParam = new BlackboardVariable<string>("DX");
    [SerializeReference] public BlackboardVariable<string> AnimatorDirectionYParam = new BlackboardVariable<string>("DY");
    [SerializeReference] public BlackboardVariable<bool> NotAnimate;

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
        _waiting = true;
        _waitTimer = WaitTime.Value;
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
                if (!MoveToPosition()) return Status.Failure;
            }
        }
        else
        {
            if (_animator != null && _navMeshAgent != null && !NotAnimate.Value)
                _animator.SetFloat(AnimatorSpeedParam, _navMeshAgent.velocity.magnitude);

            if (IsNavMeshValid() && _navMeshAgent.remainingDistance <= DistanceThreshold)
            {
                return Status.Success;
            }
        }

        if (_animator != null && _navMeshAgent != null && !NotAnimate.Value)
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
        if (_animator != null && !NotAnimate.Value)
        {
            _direction = Vector2.zero;
            _animator.SetFloat(AnimatorDirectionXParam, 0f);
            _animator.SetFloat(AnimatorDirectionYParam, 0f);
        }
    }

    void ResetAnimSpeed()
    {
        if (_animator != null && !NotAnimate.Value)
        {
            _animator.SetFloat(AnimatorSpeedParam, 0f);
            _animator.SetInteger(AnimatorMoveStateParam, -1);
        }
    }

    void Initialize()
    {
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

    bool MoveToPosition()
    {
        if (IsNavMeshValid())
        {
            Vector3 lastPosition = Agent.Value.transform.position;

            Vector3 targetPosition;
            if (Target.Value == null) targetPosition = Position.Value;
            else targetPosition = Target.Value.position;

            _currentTarget = targetPosition;
            _navMeshAgent.SetDestination(_currentTarget);

            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
                _navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                _navMeshAgent.ResetPath();
                _navMeshAgent.velocity = Vector3.zero;
                return false;
            }

            if (_animator != null && !NotAnimate.Value)
            {
                _direction = (_currentTarget - lastPosition).normalized;
                _animator.SetFloat(AnimatorSpeedParam, Speed.Value);
                _animator.SetFloat(AnimatorDirectionXParam, _direction.x);
                _animator.SetFloat(AnimatorDirectionYParam, _direction.y);
            }
            return true;
        }
        return false;
    }
}

