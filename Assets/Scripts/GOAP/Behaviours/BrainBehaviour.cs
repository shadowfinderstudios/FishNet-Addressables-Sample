using CrashKonijn.Goap.GenTest;
using CrashKonijn.Goap.Runtime;
using FishNet.Object;

namespace CrashKonijn.Docs.GettingStarted.Behaviours
{
    public class BrainBehaviour : NetworkBehaviour
    {
        GoapActionProvider _provider;
        GoapBehaviour _goap;

        void OnEnable()
        {
            _goap = FindFirstObjectByType<GoapBehaviour>();
            _provider = GetComponent<GoapActionProvider>();

            if (_provider.AgentTypeBehaviour == null)
                _provider.AgentType = _goap.GetAgentType("ScriptBasicAgent");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _provider.RequestGoal<IdleGoal>();
        }
    }
}