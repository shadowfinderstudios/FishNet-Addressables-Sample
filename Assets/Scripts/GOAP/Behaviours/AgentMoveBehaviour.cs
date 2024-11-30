using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using FishNet.Utility.Template;
using UnityEngine;
using UnityEngine.AI;

namespace CrashKonijn.Goap.GenTest
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(AgentBehaviour))]
    public class AgentMoveBehaviour : TickNetworkBehaviour
    {
        const float AnimUpdateFreq = 0.8f;

        [SerializeField] float MinMoveDistance = 0.25f;

        AgentBehaviour _agent;
        ITarget _currentTarget;
        bool _shouldMove;
        float animUpdateFrequency = 0f;

        Vector3 _lastNavPos = Vector3.zero;
        Vector3 _lastPosition;
        NavMeshAgent _navMeshAgent;
        Animator _animator;

        void OnEnable()
        {
            _agent = this.GetComponent<AgentBehaviour>();
            _animator = this.GetComponent<Animator>();
            
            SetupAgent();

            _agent.Events.OnTargetInRange += OnTargetInRange;
            _agent.Events.OnTargetChanged += OnTargetChanged;
            _agent.Events.OnTargetNotInRange += TargetNotInRange;
            _agent.Events.OnTargetLost += TargetLost;
        }

        void OnDisable()
        {
            _agent.Events.OnTargetInRange -= OnTargetInRange;
            _agent.Events.OnTargetChanged -= OnTargetChanged;
            _agent.Events.OnTargetNotInRange -= TargetNotInRange;
            _agent.Events.OnTargetLost -= TargetLost;
        }

        void SetupAgent()
        {
            if (_navMeshAgent == null || _navMeshAgent.enabled == false)
            {
                _navMeshAgent = GetComponent<NavMeshAgent>();
                _navMeshAgent.enabled = true;
                _navMeshAgent.updateRotation = false;
                _navMeshAgent.updateUpAxis = false;
                _navMeshAgent.autoRepath = true;
            }
        }

        void TargetLost()
        {
            _currentTarget = null;
            _shouldMove = false;
        }

        void OnTargetInRange(ITarget target)
        {
            _shouldMove = false;
        }

        void OnTargetChanged(ITarget target, bool inRange)
        {
            _currentTarget = target;
            _shouldMove = !inRange;

            if (_shouldMove)
            {
                var curpos = _currentTarget.Position;
                curpos.z = 0f;
                _navMeshAgent.SetDestination(curpos);
            }
        }

        void TargetNotInRange(ITarget target)
        {
            _shouldMove = true;
        }

        protected override void TimeManager_OnTick()
        {
            if (!_shouldMove) return;
            if (_currentTarget == null) return;

            var curpos = _currentTarget.Position;
            curpos.z = 0f;

            var distance = Vector3.Distance(curpos, _lastPosition);
            if (MinMoveDistance < distance || Time.fixedTime - animUpdateFrequency > AnimUpdateFreq)
            {
                _lastPosition = curpos;

                if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.SetDestination(curpos);
                }

                if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || _navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    StopAgentAndClearPath();
                    var targetPosition = _navMeshAgent.transform.position + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * 5f;
                    _navMeshAgent.SetDestination(targetPosition);
                    return;
                }

                if (_lastNavPos != _navMeshAgent.transform.position)
                {
                    _lastNavPos = _navMeshAgent.transform.position;

                    Vector2 direction = (_currentTarget.Position - transform.position).normalized;
                    if (Time.fixedTime - animUpdateFrequency > AnimUpdateFreq)
                    {
                        animUpdateFrequency = Time.fixedTime;
                        _animator.SetFloat("DX", direction.x);
                        _animator.SetFloat("DY", direction.y);
                    }

                    if (direction.x != 0f && direction.y != 0f)
                        _animator.SetBool("Walk", true);
                    else
                        _animator.SetBool("Walk", false);
                }
            }
        }

        void StopAgentAndClearPath()
        {
            _navMeshAgent.ResetPath();
            _navMeshAgent.velocity = Vector3.zero;
        }

        void OnDrawGizmos()
        {
            if (_currentTarget == null) return;
            Gizmos.DrawLine(transform.position, _currentTarget.Position);
        }
    }
}
