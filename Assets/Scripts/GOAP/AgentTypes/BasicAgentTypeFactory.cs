using CrashKonijn.Docs.GettingStarted.Capabilities;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace CrashKonijn.Docs.GettingStarted.AgentTypes
{
    public class BasicAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("ScriptBasicAgent");
            factory.AddCapability<IdleCapabilityFactory>();
            return factory.Build();
        }
    }
}