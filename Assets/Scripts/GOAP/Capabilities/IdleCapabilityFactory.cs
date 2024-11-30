using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.GenTest;
using CrashKonijn.Goap.Runtime;

namespace CrashKonijn.Docs.GettingStarted.Capabilities
{
    public class IdleCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("IdleCapability");

            builder.AddGoal<IdleGoal>()
                .AddCondition<IsIdle>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(2);

            builder.AddAction<IdleAction>()
                .SetStoppingDistance(1.2f)
                .AddEffect<IsIdle>(EffectType.Increase)
                .SetTarget<IdleTarget>();

            builder.AddTargetSensor<IdleTargetSensor>()
                .SetTarget<IdleTarget>();

            return builder.Build();
        }
    }
}