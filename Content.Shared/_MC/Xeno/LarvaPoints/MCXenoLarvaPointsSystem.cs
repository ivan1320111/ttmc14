using Content.Shared._RMC14.Xenonids.Hive;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.LarvaPoints;

public sealed class MCXenoLarvaPointsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var mapQuery = AllEntityQuery<HiveComponent>();
        while (mapQuery.MoveNext(out var cycle))
        {
            if (!cycle.Running)
                continue;

            if (cycle.LarvaPoints == cycle.NeedLarvaPointsForBurrowedLarva)
            {
                cycle.BurrowedLarva++;
                cycle.LarvaPoints = 0;
            }
        }
    }
}
